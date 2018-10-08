using System;
using System.Collections.Generic;
using UnityEngine;

public class Character {

    public float X
    {
        get { return Mathf.Lerp(currentTile.X, destinationTile.X, movementPercentage); }
    }
    public float Y
    {
        get { return Mathf.Lerp(currentTile.Y, destinationTile.Y, movementPercentage); }
    }

    public Tile currentTile
    {
        get; protected set;
    }
    Tile destinationTile; //If we aren't moving, will equal current tile
    float movementPercentage; //goes from 0 to 1  as we move between the 2 tiles

    float speed = 2f; //Tiles per second

    Action<Character> cbCharacterChanged;

    Job myJob;

    public Character(Tile _tile)
    {
        currentTile = destinationTile = _tile; //set both currentTile and destinationTile equal to _tile
    }
    //This will allow us to control our own time, instead of using default Update. So we can make the game run faster or slower
    public void Update(float _deltaTime)
    {

        //Do I have a job?
        if(myJob == null)
        {
            //Get a new job
            myJob = currentTile.World.jobQueue.Dequeue();

            //if I got a job
            if(myJob != null)
            {
                destinationTile = myJob.Tile;

                myJob.RegisterJobCancelCallback(OnJobEnded);
                myJob.RegisterJobCompleteCallback(OnJobEnded);
            }
        }




        //if we are already there
        if (currentTile == destinationTile)
        {
            if(myJob != null)
            {
                myJob.DoWork(_deltaTime);
            }



            return;
        }

        //total distance from point A to point B
        float distanceToTravel = Mathf.Sqrt(Mathf.Pow(currentTile.X - destinationTile.X, 2) + Mathf.Pow(currentTile.Y - destinationTile.Y, 2));
        //How much distance can we travel this update
        float distanceThisFrame = speed * _deltaTime;
        //How mush is that in terms of percentage to our destination
        float percentageThisFrame = distanceThisFrame / distanceToTravel;

        //add to overall percentage travel
        movementPercentage += percentageThisFrame;

        //if we have reached the destination
        if (movementPercentage >= 1)
        {
            currentTile = destinationTile;
            movementPercentage = 0;
        }

        if(cbCharacterChanged != null)
        {
            cbCharacterChanged(this);
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

}
