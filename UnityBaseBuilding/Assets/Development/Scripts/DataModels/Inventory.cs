using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Inventory are things sitting on the floor/in a pile, like a bunch of metal bars
//or a non-installed version of an InstalledObject (e.g. a chair still sitting around to be installed)
public class Inventory {

    public string objectType = "Steel Plate";

    public int maxStackSize = 50;
    protected int stackSize;
    public int StackSize
    {
        get { return stackSize; }
        set { if(stackSize != value)
            {
                stackSize = value;
                if(cbInventoryChanged != null)
                {
                    cbInventoryChanged(this);
                }
            } }
    }

    Action<Inventory> cbInventoryChanged;

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
        StackSize = _other.StackSize;
    }

    public virtual Inventory Clone()
    {
        return new Inventory(this);
    }

    public void RegisterInventoryChangedCallback(Action<Inventory> callback)
    {
        cbInventoryChanged += callback;
    }
    public void UnRegisterInventoryChangedCallback(Action<Inventory> callback)
    {
        cbInventoryChanged -= callback;
    }
}
