using System;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class World : IXmlSerializable
{
    Tile[,] tiles;

    List<Character> characters;
    public List<InstalledObject> installedObjects;

    //the pathfinding graph used to navigate world
    public Path_TileGraph tileGraph;

    //when we pass this a string it will give the corresponding InstalledObject, or visa-versa
    Dictionary<string, InstalledObject> installedObjectPrototypes;

    public int Width { get; protected set; }

    public int Height { get; protected set; }
    //a list of functions to be called on InstalledObject creation
    Action<InstalledObject> cbInstalledObjectCreated;
    Action<Character> cbCharacterCreated;
    Action<Tile> cbTileChanged;

    public JobQueue jobQueue;

    public World(int _width, int _height)
    {
        SetUpWorld(_width, _height);
    }

    void SetUpWorld(int _width, int _height)
    {
        jobQueue = new JobQueue();

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

        characters = new List<Character>();
        installedObjects = new List<InstalledObject>();
    }

    public void Update(float _deltaTime)
    {
        foreach(Character c in characters)
        {
            c.Update(_deltaTime);
        }
    }
    
    public Character CreateCharacter(Tile _tile)
    {
        Character c = new Character(_tile);

        characters.Add(c);

        if(cbCharacterCreated != null)
            cbCharacterCreated(c);

        return c;
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
        if (x >= Width || x < 0 || y >= Height || y < 0)
        {
            //Debug.LogError("Tile (" + x + "," + y + ") is out of range");
            return null;
        }
        return tiles[x, y];
    }

    public InstalledObject PlaceInstalledObject(string _objectType, Tile _t)
    {
        if (installedObjectPrototypes.ContainsKey(_objectType) == false)
        {
            Debug.LogError("installedObjectPrototypes doesn't contain a prototype for the key" + _objectType);
            return null;
        }

        InstalledObject inObj = InstalledObject.PlaceInstance(installedObjectPrototypes[_objectType], _t);
        if (inObj == null)
        {
            //Failed to place the object
            return null;
        }

        installedObjects.Add(inObj);

        if(cbInstalledObjectCreated != null)
        {
            cbInstalledObjectCreated(inObj);
            InvalidateTileGraph();
        }

        return inObj;
    }

    public void RegisterInstalledObjectCreated(Action<InstalledObject> _callbackFunction)
    {
        cbInstalledObjectCreated += _callbackFunction;
    }
    public void UnRegisterInstalledObjectCreated(Action<InstalledObject> _callbackFunction)
    {
        cbInstalledObjectCreated -= _callbackFunction;
    }
    public void RegisterCharacterCreated(Action<Character> _callbackFunction)
    {
        cbCharacterCreated += _callbackFunction;
    }
    public void UnRegisterCharacterCreated(Action<Character> _callbackFunction)
    {
        cbCharacterCreated -= _callbackFunction;
    }
    public void RegisterTileChanged(Action<Tile> _callbackFunction)
    {
        cbTileChanged += _callbackFunction;
    }
    public void UnRegisterTileChanged(Action<Tile> _callbackFunction)
    {
        cbTileChanged -= _callbackFunction;
    }

    //Called whenever any tile changes
    void OnTileChanged(Tile _t)
    {
        if (cbTileChanged == null)
        {
            return;
        }

        cbTileChanged(_t);

        InvalidateTileGraph();
    }
    //should be called whenever a change to the world means the old pathfinding info is invalid
    public void InvalidateTileGraph()
    {
        tileGraph = null;
    }

    public bool IsInstalledObjectPlacementValid(string InstalledObjectType, Tile t)
    {
        return installedObjectPrototypes[InstalledObjectType].IsValidPosition(t);
    }

    public InstalledObject GetInstalledObjectPrototype(string _objectType)
    {
        if(installedObjectPrototypes.ContainsKey(_objectType) == false)
        {
            Debug.LogError("No InstalledObject of type " + _objectType + " is in the InstalledObjectPrototypes dictionary!");
            return null;
        }

        return installedObjectPrototypes[_objectType];
    }

    public void SetupPathfindingExample()
    {
        Debug.Log("SetupPathfindingExample");

        // Make a set of floors/walls to test pathfinding with.

        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

        for (int x = l - 5; x < l + 15; x++)
        {
            for (int y = b - 5; y < b + 15; y++)
            {
                tiles[x, y].Type = TileType.Floor;


                if (x == l || x == (l + 9) || y == b || y == (b + 9))
                {
                    if (x != (l + 9) && y != (b + 4))
                    {
                        PlaceInstalledObject("Wall", tiles[x, y]);
                    }
                }



            }
        }

    }


    //Saving and Loading

    public World()
    {

    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter _writer)
    {
        //save info here

        _writer.WriteAttributeString("Width", Width.ToString());
        _writer.WriteAttributeString("Height", Height.ToString());

        _writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _writer.WriteStartElement("Tile");
                tiles[x, y].WriteXml(_writer);
                _writer.WriteEndElement();
            }
        }
        _writer.WriteEndElement();

        _writer.WriteStartElement("InstalledObjects");
        foreach (InstalledObject inObj in installedObjects)
        {
            _writer.WriteStartElement("InstalledObject");
            inObj.WriteXml(_writer);
            _writer.WriteEndElement();
        }
        _writer.WriteEndElement();

        _writer.WriteStartElement("Characters");
        foreach (Character c in characters)
        {
            _writer.WriteStartElement("Character");
            c.WriteXml(_writer);
            _writer.WriteEndElement();
        }
        _writer.WriteEndElement();
    }

    public void ReadXml(XmlReader _reader)
    {
        Debug.Log("World -- ReadXml");
        //load info here

        //Read the Width and Height attributes of the World element
        Width = int.Parse(_reader.GetAttribute("Width"));
        Height = int.Parse(_reader.GetAttribute("Height"));

        SetUpWorld(Width, Height);

        //go through and read every line
        while (_reader.Read())
        {
            switch (_reader.Name)
            {
                //if the element we're on has a name of Tiles
                case "Tiles":
                    ReadXml_Tiles(_reader);
                    break;
                case "InstalledObjects":
                    ReadXml_InstalledObjects(_reader);
                    break;
                case "Characters":
                    ReadXml_Characters(_reader);
                    break;
            }
        }
    }

    void ReadXml_Tiles(XmlReader _reader)
    {
        //We are in the Tiles element, read it's descendant elements named Tile until we run out
        while (_reader.Read())
        {
            if (_reader.Name != "Tile")
                return; //we are out of tiles

            //Read X and Y attributes of the Tile element
            int x = int.Parse(_reader.GetAttribute("X"));
            int y = int.Parse(_reader.GetAttribute("Y"));
            tiles[x, y].ReadXml(_reader);
        }
    }

    void ReadXml_InstalledObjects(XmlReader _reader)
    {
        //We are in the InstalledObjects element, read it's descendant elements named InstalledObject until we run out
        while (_reader.Read())
        {
            if (_reader.Name != "InstalledObject")
                return; //we are out of installedObjects

            //Read X and Y attributes of the Tile element
            int x = int.Parse(_reader.GetAttribute("X"));
            int y = int.Parse(_reader.GetAttribute("Y"));

            InstalledObject inObj = PlaceInstalledObject(_reader.GetAttribute("ObjectType"), tiles[x,y]);
            //Things like movementCost or health set in the inObj's ReadXml
            inObj.ReadXml(_reader);
        }
    }
    void ReadXml_Characters(XmlReader _reader)
    {
        //We are in the Characters element, read it's descendant elements named Character until we run out
        while (_reader.Read())
        {
            if (_reader.Name != "Character")
                return; //we are out of characters

            //Read X and Y attributes of the Character element
            int x = int.Parse(_reader.GetAttribute("X"));
            int y = int.Parse(_reader.GetAttribute("Y"));

            Character c = new Character(tiles[x, y]);
            //Things like health set in c's ReadXml
            c.ReadXml(_reader);
        }
    }
}
