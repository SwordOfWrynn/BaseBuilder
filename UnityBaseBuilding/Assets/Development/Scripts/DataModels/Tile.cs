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

}
