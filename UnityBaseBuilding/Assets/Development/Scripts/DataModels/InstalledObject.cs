using System;
using System.Collections.Generic;
using UnityEngine;

//InstalledObjects are things like walls, doors, furniture, etc.
public class InstalledObject {

    //this represents BASE tile of the object, large objects may occupy multipule tiles
    public Tile Tile { get; protected set; } 

    //This objectType will be queried by the visual system to know what sprite to render for the object
    public string ObjectType { get; protected set; }

    //this is a multiplier, so a value of 2 means you move twice as slowly (half speed)
    //Tile types and other environmental effects may be combined
    //e.g. moving through a tile with a cost of 2 with a InstalledObject with a cost of 3, that is on fire with has 3 cost
    //would have total cost of 8, so you'd move at 1/8 speed
    //SPECIAL: if movementCost = 0, then tile cannot be moved through
    float movementCost;

    //For example, a sofa may 3x2, but the graphic only covers 3x1, so there is leg room
    int width;
    int height;

    //if it connects to nearby object of the same kind, like walls or pipes
    public bool LinksToNeighbour { get; protected set; }

    Action<InstalledObject> cbOnChanged;

    protected InstalledObject()
    {

    }

    //this will be used to create the prototypical objects in the code that the real ones will be copied from
    static public InstalledObject CreatePrototype (string _objectType, float _movementCost = 1f, int _width = 1, int _height = 1, bool _linksToNeighbour = false)
    {
        InstalledObject obj = new InstalledObject();
        obj.ObjectType = _objectType;
        obj.movementCost = _movementCost;
        obj.width = _width;
        obj.height = _height;
        obj.LinksToNeighbour = _linksToNeighbour;

        return obj;
    }
    //takes the prototype and a tile and creates the actual object
    static public InstalledObject PlaceInstance (InstalledObject _proto, Tile _tile)
    {
        InstalledObject obj = new InstalledObject();

        obj.ObjectType = _proto.ObjectType;
        obj.movementCost = _proto.movementCost;
        obj.width = _proto.width;
        obj.height = _proto.height;
        obj.LinksToNeighbour = _proto.LinksToNeighbour;

        obj.Tile = _tile;

        if(obj.Tile.PlaceObject(obj) == false)
        {
            //For some reason, we weren't able to place the object on the tile, it was probaly occupied already

            //Do not return the object, it will be garbage collected
            return null;
        }

        if (obj.LinksToNeighbour)
        {
            //this type of InstalledObject links to neighbors so we need to inform its new neighbors when it is made

        }

        return obj;
    }

    public void RegisterOnChangedCallback(Action<InstalledObject> callbackFunction)
    {
        cbOnChanged += callbackFunction;
    }
    public void UnRegisterOnChangedCallback(Action<InstalledObject> callbackFunction)
    {
        cbOnChanged -= callbackFunction;
    }

}
