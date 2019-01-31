using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Inventory are things sitting on the floor/in a pile, like a bunch of metal bars
//or a non-installed version of an InstalledObject (e.g. a chair still sitting around to be installed)
public class Inventory {

    public string objectType = "Steel Plate";

    public int maxStackSize = 50;
    public int stackSize;

    public Tile tile;
    public Character character;

    public Inventory()
    {

    }
    public Inventory(string _objectType, int _maxStackSize, int stacksize)
    {
        objectType = _objectType;
        maxStackSize = _maxStackSize;
        stacksize = _maxStackSize;
    }

    protected Inventory(Inventory _other)
    {
        objectType = _other.objectType;
        maxStackSize = _other.maxStackSize;
        stackSize = _other.stackSize;
    }

    public virtual Inventory Clone()
    {
        return new Inventory(this);
    }

}
