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
    Tile destinationTile; //If we aren't moving, will equal current tile
    Tile nextTile; //the next tile in the pathfinding sequence
    Path_AStar pathAStar;
    float movementPercentage; //goes from 0 to 1  as we move between the 2 tiles

    float speed = 2f; //Tiles per second

    Action<Character> cbCharacterChanged;

    Job myJob;

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
            //Get a new job
            myJob = currentTile.World.jobQueue.Dequeue();

            //if I got a job
            if (myJob != null)
            {
                destinationTile = myJob.Tile;

                myJob.RegisterJobCancelCallback(OnJobEnded);
                myJob.RegisterJobCompleteCallback(OnJobEnded);
            }
        }

        //if we are already there
        if (currentTile == destinationTile)
        //if(pathAStar != null && pathAStar.Length() == 1) //we are next to job site
        {
            if (myJob != null)
            {
                myJob.DoWork(_deltaTime);
            }
            
        }
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
                    pathAStar = null;
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

        if(nextTile.MovementCost == 0)
        {
            Debug.LogError("Character -- Update_DoMovement: A character tried to enter an unwalkable tile");
            //the next tile shouldn't be walked on, so the path info is outdated and need to be updated (e.g. a wall was built here after the path was made)
            nextTile = null;
            pathAStar = null;
            return;
        }
        else
        {
            //The tile we are tring to enter is technically walkable (i.e. a wall), but are we actuallyt allowed to enter it right now (e.g. a closed door)?
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
        pathAStar = null;
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
