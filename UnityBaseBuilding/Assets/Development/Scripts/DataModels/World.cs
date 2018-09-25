using System;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    Tile[,] tiles;

    Dictionary<string, InstalledObject> installedObjectPrototypes;

    public int Width { get; protected set; }

    public int Height { get; protected set; }
    //a list of functions to be called on InstalledObject creation
    Action<InstalledObject> cbInstalledObjectCreated;
    Action<Tile> cbTileChanged;

    public Queue<Job> jobQueue;

    public World(int _width = 100, int _height = 100)
    {
        jobQueue = new Queue<Job>();

        Width = _width;
        Height = _height;
 
        tiles = new Tile[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
                tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
            }
        }
        Debug.Log("World created with " + (_width * _height) + " tiles");
        CreateInstalledObjectPrototypes();
    }
    
    protected void CreateInstalledObjectPrototypes()
    {
        installedObjectPrototypes = new Dictionary<string, InstalledObject>();
        
        installedObjectPrototypes.Add("Wall", InstalledObject.CreatePrototype("Wall", 0, 1, 1, true));
    }

    public void RandomizeTiles()
    {
        Debug.Log("RandomizeTiles Called");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (UnityEngine.Random.Range (0, 2) == 0)
                {
                    tiles[x, y].Type = TileType.Empty;
                }
                else
                {
                    tiles[x, y].Type = TileType.Floor;
                }
            }
        }
    }

    public Tile GetTileAt(int x, int y)
    {
        if (x > Width || x < 0 || y > Height || y < 0)
        {
            Debug.LogError("Tile (" + x + "," + y + ") is out of range");
            return null;
        }
        return tiles[x, y];
    }

    public void PlaceInstalledObject(string _objectType, Tile _t)
    {
        if (installedObjectPrototypes.ContainsKey(_objectType) == false)
        {
            Debug.LogError("installedObjectPrototypes doesn't contain a prototype for the key" + _objectType);
            return;
        }

        InstalledObject obj = InstalledObject.PlaceInstance(installedObjectPrototypes[_objectType], _t);
        if (obj == null)
        {
            //Failed to place the object
            return;
        }
        if(cbInstalledObjectCreated != null)
        {
            cbInstalledObjectCreated(obj);
        }
    }

    public void RegisterInstalledObjectCreated(Action<InstalledObject> _callbackFunction)
    {
        cbInstalledObjectCreated += _callbackFunction;
    }
    public void UnRegisterInstalledObjectCreated(Action<InstalledObject> _callbackFunction)
    {
        cbInstalledObjectCreated -= _callbackFunction;
    }
    public void RegisterTileChanged(Action<Tile> _callbackFunction)
    {
        cbTileChanged += _callbackFunction;
    }
    public void UnRegisterTileChanged(Action<Tile> _callbackFunction)
    {
        cbTileChanged -= _callbackFunction;
    }
    void OnTileChanged(Tile _t)
    {
        if (cbTileChanged == null)
        {
            return;
        }

        cbTileChanged(_t);
    }
    public bool IsInstalledObjectPlacementValid(string InstalledObjectType, Tile t)
    {
        return installedObjectPrototypes[InstalledObjectType].IsValidPosition(t);
    }

}
