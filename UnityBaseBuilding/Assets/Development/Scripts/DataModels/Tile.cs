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


    LooseObject looseObject;
    InstalledObject installedObject;

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
}
