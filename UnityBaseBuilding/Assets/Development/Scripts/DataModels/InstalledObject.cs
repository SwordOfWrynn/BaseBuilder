using System;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

//InstalledObjects are things like walls, doors, furniture, etc.
public class InstalledObject : IXmlSerializable {

    public Dictionary<string, float> inObjParameters;
    public Action<InstalledObject, float> updateActions;


    //this represents BASE tile of the object, large objects may occupy multipule tiles
    public Tile Tile { get; protected set; } 

    //This objectType will be queried by the visual system to know what sprite to render for the object
    public string ObjectType { get; protected set; }

    //this is a multiplier, so a value of 2 means you move twice as slowly (half speed)
    //Tile types and other environmental effects may be combined
    //e.g. moving through a tile with a cost of 2 with a InstalledObject with a cost of 3, that is on fire with has 3 cost
    //would have total cost of 8, so you'd move at 1/8 speed
    //SPECIAL: if movementCost = 0, then tile cannot be moved through
    public float MovementCost
    {
        get; protected set;
    }

    //For example, a sofa may 3x2, but the graphic only covers 3x1, so there is leg room
    int width;
    int height;

    //if it connects to nearby object of the same kind, like walls or pipes
    public bool LinksToNeighbour { get; protected set; }

    Action<InstalledObject> cbOnChanged;
    //Func is like an action, but will return something, in this case a boolean
    Func<Tile, bool> funcPositionValidation;

    public InstalledObject()
    {
        //Empty constructor used for Xml serialization
        inObjParameters = new Dictionary<string, float>();
    }

    protected InstalledObject(InstalledObject _other)
    {
        ObjectType = _other.ObjectType;
        MovementCost = _other.MovementCost;
        width = _other.width;
        height = _other.height;
        LinksToNeighbour = _other.LinksToNeighbour;

        inObjParameters = new Dictionary<string, float>(_other.inObjParameters);

        if(_other.updateActions != null)
            updateActions = (Action<InstalledObject, float>)_other.updateActions.Clone();
    }

    virtual public InstalledObject Clone ()
    {
        return new InstalledObject(this);
    }

    //this will be used to create the prototypical objects in the code that the real ones will be copied from
    public InstalledObject (string _objectType, float _movementCost = 1f, int _width = 1, int _height = 1, bool _linksToNeighbour = false)
    {
        ObjectType = _objectType;
        MovementCost = _movementCost;
        width = _width;
        height = _height;
        LinksToNeighbour = _linksToNeighbour;

        funcPositionValidation = _IsValidPosition;

        inObjParameters = new Dictionary<string, float>();
    }

    public void Update(float _deltaTime)
    {
        if(updateActions != null)
        {
            updateActions(this, _deltaTime);
        }
    }

    //takes the prototype and a tile and creates the actual object
    static public InstalledObject PlaceInstance (InstalledObject _proto, Tile _tile)
    {
        if(_proto.funcPositionValidation(_tile) == false)
        {
            Debug.LogError("PlaceInstance -- Position Validity function returned FALSE.");
            return null;
        }
        //We now know that the placement is valid

        //create object
        InstalledObject inObj = _proto.Clone();

        inObj.Tile = _tile;

        if(inObj.Tile.PlaceObject(inObj) == false)
        {
            //For some reason, we weren't able to place the object on the tile, it was probaly occupied already

            //Do not return the object, it will be garbage collected
            return null;
        }

        if (inObj.LinksToNeighbour)
        {
            //this type of InstalledObject links to neighbors so we need to inform its new neighbors when it is made

            Tile t;

            int x = inObj.Tile.X;
            int y = inObj.Tile.Y;

            t = inObj.Tile.World.GetTileAt(x, y + 1);
            //if there is a tile above us, it has an object on it, and that object matches ours
            if (t != null && t.InstalledObject != null && t.InstalledObject.cbOnChanged != null && t.InstalledObject.ObjectType == inObj.ObjectType)
            {
                t.InstalledObject.cbOnChanged(t.InstalledObject);
            }

            t = inObj.Tile.World.GetTileAt(x + 1, y);
            //if there is a tile to the right, it has an object on it, and that object matches ours
            if (t != null && t.InstalledObject != null && t.InstalledObject.cbOnChanged != null && t.InstalledObject.ObjectType == inObj.ObjectType)
            {
                t.InstalledObject.cbOnChanged(t.InstalledObject);
            }

            t = inObj.Tile.World.GetTileAt(x, y - 1);
            //if there is a tile below us, it has an object on it, and that object matches ours
            if (t != null && t.InstalledObject != null && t.InstalledObject.cbOnChanged != null && t.InstalledObject.ObjectType == inObj.ObjectType)
            {
                t.InstalledObject.cbOnChanged(t.InstalledObject);
            }

            t = inObj.Tile.World.GetTileAt(x - 1, y);
            //if there is a tile to the left, it has an object on it, and that object matches ours
            if (t != null && t.InstalledObject != null && t.InstalledObject.cbOnChanged != null && t.InstalledObject.ObjectType == inObj.ObjectType)
            {
                t.InstalledObject.cbOnChanged(t.InstalledObject);
            }

        }

        return inObj;
    }

    public void RegisterOnChangedCallback(Action<InstalledObject> callbackFunction)
    {
        cbOnChanged += callbackFunction;
    }
    public void UnRegisterOnChangedCallback(Action<InstalledObject> callbackFunction)
    {
        cbOnChanged -= callbackFunction;
    }

    public bool IsValidPosition(Tile _t)
    {
        return funcPositionValidation(_t);
    }

    public bool _IsValidPosition(Tile _tile)
    {
        //make sure tile is floor
        if(_tile.Type != TileType.Floor)
        {
            return false;
        }
        //Make sure the tile doesn't already have an InstalledObject on it
        if(_tile.InstalledObject != null)
        {
            return false;
        }

        return true;
    }

    public bool _IsValidPosition_Door(Tile _tile)
    {
        if (_IsValidPosition(_tile) == false)
        {
            return false;
        }
        //make sure there is a pair off E/W walls, or N/S walls

        return true;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter _writer)
    {
        _writer.WriteAttributeString("X", Tile.X.ToString());
        _writer.WriteAttributeString("Y", Tile.Y.ToString());
        _writer.WriteAttributeString("ObjectType", ObjectType);
        //_writer.WriteAttributeString("MovementCost", MovementCost.ToString());

        foreach (string k in inObjParameters.Keys)
        {
            _writer.WriteStartElement("Param");
            _writer.WriteAttributeString("name", k);
            _writer.WriteAttributeString("value", inObjParameters[k].ToString());
            _writer.WriteEndElement();
        }
    }

    public void ReadXml(XmlReader _reader)
    {
        //X, Y, Tile and object type should have already been set in the World
        //MovementCost = int.Parse(_reader.GetAttribute("MovementCost"));

        if (_reader.ReadToDescendant("Param"))
        {
            do
            {
                string k = _reader.GetAttribute("name");
                float v = float.Parse(_reader.GetAttribute("value"));
                inObjParameters[k] = v;
            } while (_reader.ReadToNextSibling("Param"));

            
        }
    }

}
