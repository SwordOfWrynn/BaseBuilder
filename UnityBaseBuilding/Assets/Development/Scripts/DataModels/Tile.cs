using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile {

    //TileType is base type of the tile, to tell if it's
    //part of the station or empty space
    public enum TileType { Empty, Floor };

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
    int x;
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
