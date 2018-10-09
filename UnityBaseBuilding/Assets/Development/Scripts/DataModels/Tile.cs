﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


    //TileType is base type of the tile, to tell if it's part of the 
    //station or empty space or some other tile type
    //It is outside the class so it can be used everywhere and not 
    //needing a reference to the Tile class
    public enum TileType { Empty, Floor };

public class Tile {



    TileType _type = TileType.Empty;
    //the function(s) we callback any time our tile data changes
    Action<Tile> cbTileChanged;

    public TileType Type
    {
        get
        {
            return _type;
        }
        set
        {
            TileType oldType = _type;
            _type = value;
            //Call the callback to let things know type has changed
            if (cbTileChanged != null && _type != oldType)
            {
                cbTileChanged(this);
            }
        }
    }


    Inventory inventory;
    public InstalledObject InstalledObject { get; protected set; }

    public Job pendingInstalledObjectJob;

    public World World {get; protected set;}
    public int X { get; protected set; }
    public int Y { get; protected set; }

    public float MovementCost
    {
        get
        {
            if (Type == TileType.Empty)
                return 0;
            if (InstalledObject == null)
                return 1;
            return 1 * InstalledObject.MovementCost;
        }

    }

    public Tile( World world, int x, int y )
    {
        World = world;
        X = x;
        Y = y;
    }

    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged += callback;
    }
    public void UnRegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged -= callback;
    }
    //takes in an instance of an InstalledObject, not a prototype
    public bool PlaceObject(InstalledObject _objInstance)
    {
        if(_objInstance == null)
        {
            //we are uninstalling whatever was here
            InstalledObject = null;
            return true;
        }
        //Now objInstance is not null
        if(InstalledObject != null)
        {
            Debug.LogError("Trying to assign an installed object to a tile that already has one!");
            return false;
        }

        //At this point everything should be fine
        InstalledObject = _objInstance;
        return true;
    }

    //tells us if two tiles are adjacent
    public bool isNeighboor(Tile _tile, bool diagonalOkay = false)
    {
        //Check to see if the difference is exactly one between the two tile positions. If yes, then they are neighbours
        return Mathf.Abs(X - _tile.X) + Mathf.Abs(Y - _tile.Y) == 1 || (diagonalOkay && (Mathf.Abs(X - _tile.X) == 1 && Mathf.Abs(Y - _tile.Y) == 1));
    }

    public Tile[] GetNeighbours( bool diagonalOkay = false)
    {
        Tile[] neighbourTiles;
        if(diagonalOkay == false)
        {
            neighbourTiles = new Tile[4]; //Tile order N, E, S, W
        }
        else
        {
            neighbourTiles = new Tile[8]; //Tile order N, E, S, W, NE, SE, SW, NW
        }

        Tile neighbour;

        neighbour = World.GetTileAt(X, Y + 1);
        neighbourTiles[0] = neighbour; //May be null
        neighbour = World.GetTileAt(X + 1, Y);
        neighbourTiles[1] = neighbour; //May be null
        neighbour = World.GetTileAt(X, Y - 1);
        neighbourTiles[2] = neighbour; //May be null
        neighbour = World.GetTileAt(X - 1, Y);
        neighbourTiles[3] = neighbour; //May be null

        if (diagonalOkay)
        {
            neighbour = World.GetTileAt(X + 1, Y + 1);
            neighbourTiles[4] = neighbour; //May be null
            neighbour = World.GetTileAt(X + 1, Y - 1);
            neighbourTiles[5] = neighbour; //May be null
            neighbour = World.GetTileAt(X - 1, Y - 1);
            neighbourTiles[6] = neighbour; //May be null
            neighbour = World.GetTileAt(X - 1, Y + 1);
            neighbourTiles[7] = neighbour; //May be null
        }

        return neighbourTiles;

    }

}
