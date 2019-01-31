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
    Tile destinationTile
    {
        get;
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

    Inventory myInventory; //The item we are carrying, not equipment or something

    public Character()
    {
        //use only for serialization
    }

    public Character(Tile _tile)
    {
        currentTile = destinationTile = nextTile = _tile; //set both currentTile and destinationTile equal to _tile
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
                destinationTile = currentTile;
                return;
            }
        }
        //We have a job and it is reachable

        //Does the job have all of its materials?
        if (myJob.HasAllMaterial() == false)
        {
            //No, it doesn't have all the materials, Are we carring the needed materials?
            //If not, are we carrying materials that we need to drop?
            if(myInventory != null)
            {
                if (myJob.NeedsInventoryType(myInventory))
                {
                    //if we are, deliver the materials at the job tile

                    if(currentTile == destinationTile)
                    {
                        //We are at the job's tile, drop the inventory
                        currentTile.World.inventoryManager.PlaceInventory(myJob, myInventory);
                        //are we still carrying things?
                        if (myInventory.stackSize == 0)
                            myInventory = null;
                        else
                        {
                            Debug.LogError("Character still carrying inventory. Setting it to null, but we are leaking inventory");
                            myInventory = null;
                        }

                    }
                    else //we still need to walk to the job
                        destinationTile = myJob.tile;
                }
            }

            //if not, go to a tile that has the needed materials
            //if we are there pick up the materials

            return; //we won't continue until all the materials are ready
        }
        //the job has all of the needed materials
        //make sure our destination tile is the job's tile
        destinationTile = myJob.tile;

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
        if (currentTile == destinationTile)
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
                pathAStar = new Path_AStar(currentTile.World, currentTile, destinationTile); //calculate a path from the current tile to the destination tile
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

        destinationTile = myJob.tile;
        myJob.RegisterJobCancelCallback(OnJobEnded);
        myJob.RegisterJobCompleteCallback(OnJobEnded);

        //check to see if final job tile is reachable
        //generate a path to check
        pathAStar = new Path_AStar(currentTile.World, currentTile, destinationTile); //calculate a path from the current tile to the destination tile
        if (pathAStar.Length() == 0)
        {
            Debug.LogError("Character -- Update_DoMovement: Path_AStar returned no path to target job");
            AbandonJob();
            destinationTile = currentTile;
        }

    }
    
    public void SetDestination(Tile _tile)
    {
        if(currentTile.isNeighboor(_tile, true) == false)
        {
            Debug.Log("Character -- SetDestination: The destinationTile is not adjacent to the currentTile!");
        }

        destinationTile = _tile;

    }
    //stop pathfinding and put job back in queue
    public void AbandonJob()
    {
        nextTile = destinationTile = currentTile;
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
