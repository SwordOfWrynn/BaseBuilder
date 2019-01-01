﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager {

    //this is a list of all live inventories
    public Dictionary<string, List<Inventory>> inventoryStacks;

    public InventoryManager()
    {
        inventoryStacks = new Dictionary<string, List<Inventory>>();
    }

    public bool PlaceInventory(Tile _tile, Inventory _inv)
    {
        bool tileWasEmpty = _tile.Inventory == null;

        if(_tile.PlaceInventory(_inv) == false)
        {
            //The tile did not accept the inventory for some reason, therefore stop
            return false;
        }
        //At this point _inv might be empty if it was merged to another stack
        if(_inv.stackSize == 0)
        {
            if(inventoryStacks.ContainsKey(_tile.Inventory.objectType)){
                inventoryStacks[_inv.objectType].Remove(_inv);
            }
        }

        //We may have created a new stack on the tile, if the tile was empty before
        if (tileWasEmpty)
        {
            if (inventoryStacks.ContainsKey(_tile.Inventory.objectType) == false){
                inventoryStacks[_tile.Inventory.objectType] = new List<Inventory>();
            }

            inventoryStacks[_tile.Inventory.objectType].Add(_tile.Inventory);
        }

        return true;
    }

}