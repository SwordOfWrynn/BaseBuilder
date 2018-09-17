using System.Collections;
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

    Action<Tile> cbTileTypeChanged;

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
            if (cbTileTypeChanged != null && _type != oldType)
            {
                cbTileTypeChanged(this);
            }
        }
    }


    Inventory inventory;
    public InstalledObject InstalledObject { get; protected set; }

    World world;
    public int X { get; protected set; }
    public int Y { get; protected set; }

    public Tile( World world, int x, int y )
    {
        this.world = world;
        X = x;
        Y = y;
    }

    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileTypeChanged += callback;
    }
    public void UnRegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileTypeChanged -= callback;
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

}
