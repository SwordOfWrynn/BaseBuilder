using System;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Character : IXmlSerializable{

    public float X
    {
        get { return Mathf.Lerp(currentTile.X, nextTile.X, movementPercentage); }
    }
    public float Y
    {
        get { return Mathf.Lerp(currentTile.Y, nextTile.Y, movementPercentage); }
    }

    public Tile currentTile
    {
        get; protected set;
    }
    //If we aren't moving, will equal current tile
    Tile destinationTile;
    Tile DestinationTile
    {
        get { return destinationTile; }
        set
        {
            if(destinationTile != value)
            {
                destinationTile = value;
                pathAStar = null; //If it is a new destination we need to make a new path
            }
        }
    }

    Tile nextTile; //the next tile in the pathfinding sequence
    Path_AStar pathAStar;
    float movementPercentage; //goes from 0 to 1  as we move between the 2 tiles

    float speed = 2f; //Tiles per second

    Action<Character> cbCharacterChanged;

    Job myJob;

    public Inventory inventory; //The item we are carrying, not equipment or something

    public Character()
    {
        //use only for serialization
    }

    public Character(Tile _tile)
    {
        currentTile = DestinationTile = nextTile = _tile; //set both currentTile and destinationTile equal to _tile
    }

    //This will allow us to control our own time, instead of using default Update. So we can make the game run faster or slower
    public void Update(float _deltaTime)
    {

        Update_DoJob(_deltaTime);

        Update_DoMovement(_deltaTime);

        if (cbCharacterChanged != null)
        {
            cbCharacterChanged(this);
        }

    }

    void Update_DoJob(float _deltaTime)
    {
        //Do I have a job?
        if (myJob == null)
        {
            GetNewJob();

            //if the queue has no jobs for us, so return
            if (myJob == null)
            {
                DestinationTile = currentTile;
                return;
            }
        }
        //We have a job and it is reachable

        //Does the job have all of its materials?
        if (myJob.HasAllMaterial() == false)
        {
            //No, it doesn't have all the materials, Are we carring the needed materials?
            //If not, are we carrying materials that we need to drop?
            if (inventory != null)
            {
                if (myJob.NeedsInventoryType(inventory) > 0)
                {
                    //if we are, deliver the materials at the job tile

                    if (currentTile == DestinationTile)
                    {
                        //We are at the job's tile, drop the inventory
                        currentTile.World.inventoryManager.PlaceInventory(myJob, inventory);
                        //are we still carrying things?
                        if (inventory.StackSize == 0)
                            inventory = null;
                        else
                        {
                            Debug.LogError("Character still carrying inventory. Setting it to null, but we are leaking inventory");
                            inventory = null;
                        }

                    }
                    else
                    { //we still need to walk to the job
                        DestinationTile = myJob.tile;
                        return;
                    }
                }
                else
                {
                    //we are carring inventory that the job does not need, so we need to drop it
                    if (currentTile.World.inventoryManager.PlaceInventory(currentTile, inventory) == false)
                    {
                        //At some point, implement finding nearest valid tile to dump, but for now just set null
                        Debug.LogError("Character tried to dump inventory in invalid tile! setting character inventory to null");
                        inventory = null;
                    }
                }
            }
            else
            {
                //At this point, the job still requires inventory, but we don't have what is needed

                //Are we standing on a tile with what we need?
                if (currentTile.Inventory != null && myJob.NeedsInventoryType(currentTile.Inventory) > 0)
                {
                    //pick up the inventory
                    currentTile.World.inventoryManager.PlaceInventory(this, currentTile.Inventory, myJob.NeedsInventoryType(currentTile.Inventory));

                }

                else
                {
                    //Find first material the job needs
                    Inventory desired = myJob.GetFirstNeededInventory();

                    Inventory supplier = currentTile.World.inventoryManager.GetClosestInventoryOfType(
                        desired.objectType, currentTile, desired.maxStackSize - desired.StackSize);

                    if (supplier == null)
                    {
                        Debug.LogFormat("No tile contains objects of type '{0}' to satisfied job requirments. Abandoning Job.", desired.objectType);
                        AbandonJob();
                        return;
                    }

                    DestinationTile = supplier.tile;

                    return;
                }
            }


            return; //we won't continue until all the materials are ready
        }
        //the job has all of the needed materials
        //make sure our destination tile is the job's tile
        DestinationTile = myJob.tile;

        //if we are already there
        if (currentTile == myJob.tile)
        {
            //job DoWork is mostly a countdown, that will call the job completed when finished
            myJob.DoWork(_deltaTime);
        }

        //nothing left to do, Update_DoMovement will handle moving to destinations
    }

    void Update_DoMovement(float _deltaTime)
    {
        //if we don't need to move, don't move, we're already here
        if (currentTile == DestinationTile)
        {
            pathAStar = null;
            return;
        }

        if(nextTile == null || nextTile == currentTile)
        {
            //Get next tile from pathfinder
            if(pathAStar == null || pathAStar.Length() == 0)
            {
                //generate a path
                pathAStar = new Path_AStar(currentTile.World, currentTile, DestinationTile); //calculate a path from the current tile to the destination tile
                if(pathAStar.Length()==0)
                {
                    Debug.LogError("Character -- Update_DoMovement: Path_AStar returned no path to destination");
                    AbandonJob();
                    return;
                }
                //ignore the first tile, it is what we're on
            nextTile = pathAStar.Dequeue();
            }


            //Grab next waypoint
            nextTile = pathAStar.Dequeue();
            if(nextTile == currentTile)
            {
                Debug.LogError("Character-- Update_DoMovement: nextTile is equal to currentTile");
            }

        }

        //if (pathAStar.Length() == 1)
        //    return;

        //At this point we should have a valid nextTile


        //total distance from point A to point B
        float distanceToTravel = Mathf.Sqrt(Mathf.Pow(currentTile.X - nextTile.X, 2) + Mathf.Pow(currentTile.Y - nextTile.Y, 2));

        if(nextTile.IsEnterable() == ENTERABILITY.Never)
        {
            Debug.LogError("Character -- Update_DoMovement: A character tried to enter an unwalkable tile");
            //the next tile shouldn't be walked on, so the path info is outdated and need to be updated (e.g. a wall was built here after the path was made)
            nextTile = null;
            pathAStar = null;
            return;
        }
        else if(nextTile.IsEnterable() == ENTERABILITY.Soon)
        {
            //We can't enter the tile now, but we should be able to in the near future. This tile is likely a door, so we will return until the tile is enterable
            //then we process the movement and move thrugh the tile

            return;
        }

        //How much distance can we travel this update
        float distanceThisFrame = speed / nextTile.MovementCost * _deltaTime;


        //How mush is that in terms of percentage to our destination
        float percentageThisFrame = distanceThisFrame / distanceToTravel;

        //add to overall percentage travel
        movementPercentage += percentageThisFrame;

        //if we have reached the destination
        if (movementPercentage >= 1)
        {
            currentTile = nextTile;
            movementPercentage = 0;
        }
    }

    void GetNewJob() {
        myJob = currentTile.World.jobQueue.Dequeue();

        if (myJob == null)
            return;

        DestinationTile = myJob.tile;
        myJob.RegisterJobCancelCallback(OnJobEnded);
        myJob.RegisterJobCompleteCallback(OnJobEnded);

        //check to see if final job tile is reachable
        //generate a path to check
        pathAStar = new Path_AStar(currentTile.World, currentTile, DestinationTile); //calculate a path from the current tile to the destination tile
        if (pathAStar.Length() == 0)
        {
            Debug.LogError("Character -- Update_DoMovement: Path_AStar returned no path to target job");
            AbandonJob();
            DestinationTile = currentTile;
        }

    }
    
    public void SetDestination(Tile _tile)
    {
        if(currentTile.isNeighboor(_tile, true) == false)
        {
            Debug.Log("Character -- SetDestination: The destinationTile is not adjacent to the currentTile!");
        }

        DestinationTile = _tile;

    }
    //stop pathfinding and put job back in queue
    public void AbandonJob()
    {
        nextTile = DestinationTile = currentTile;
        currentTile.World.jobQueue.Enqueue(myJob);
        myJob = null;
    }

    public void RegisterOnChangedCallback(Action<Character> _cb)
    {
        cbCharacterChanged += _cb;
    }
    public void UnregisterOnChangedCallback(Action<Character> _cb)
    {
        cbCharacterChanged -= _cb;
    }

    public void OnJobEnded(Job _j)
    {
        //job completed or was cancelled

        if(_j != myJob)
        {
            Debug.LogError("Character -- OnJobEnded: A Character is being told about a job that is not their own! Make sure everything is unregistered.");
            return;
        }
        myJob = null;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter _writer)
    {
        _writer.WriteAttributeString("X", currentTile.X.ToString());
        _writer.WriteAttributeString("Y", currentTile.Y.ToString());
    }

    public void ReadXml(XmlReader _reader)
    {

    }

}
