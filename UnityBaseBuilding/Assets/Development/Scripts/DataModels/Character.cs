﻿using System.Collections;
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

    Tile currentTile;
    Tile destinationTile; //If we aren't moving, will equal current tile
    float movementPercentage; //goes from 0 to 1  as we move between the 2 tiles

    float speed = 2f; //Tiles per second

    public Character(Tile _tile)
    {
        currentTile = destinationTile = _tile; //set both currentTile and destinationTile equal to _tile
    }
    //This will allow us to control our own time, instead of using default Update. So we can make the game run faster or slower
    public void Update(float _deltaTime)
    {
        //if we are already there
        if (currentTile == destinationTile)
            return;
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
    }

    public void SetDestination(Tile _tile)
    {
        if(currentTile.isNeighboor(_tile, true) == false)
        {
            Debug.Log("Character -- SetDestination: The destinationTile is not adjacent to the currentTile!");
        }

        destinationTile = _tile;

    }

}
