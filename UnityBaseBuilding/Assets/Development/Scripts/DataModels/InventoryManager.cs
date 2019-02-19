using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager {

    //this is a list of all live inventories
    public Dictionary<string, List<Inventory>> inventoryStacks;

    public InventoryManager()
    {
        inventoryStacks = new Dictionary<string, List<Inventory>>();
    }

    /// <summary>
    /// Checks the inventory, and cleans it up if it is equal to zero
    /// </summary>
    /// <param name="_inv"></param>
    void CleanupInventory(Inventory _inv)
    {
        if(_inv.StackSize == 0)
        {
            if (inventoryStacks.ContainsKey(_inv.objectType))
            {
                inventoryStacks[_inv.objectType].Remove(_inv);
            }
            if(_inv.tile != null)
            {
                _inv.tile.Inventory = null;
                _inv.tile = null;
            }
            if(_inv.character != null)
            {
                _inv.character.inventory = null;
                _inv.character = null;
            }
        }
    }

    public bool PlaceInventory(Tile _tile, Inventory _sourceInv)
    {
        bool tileWasEmpty = _tile.Inventory == null;

        if(_tile.PlaceInventory(_sourceInv) == false)
        {
            //The tile did not accept the inventory for some reason, therefore stop
            return false;
        }
        //At this point _inv might be empty if it was merged to another stack, CleanupInventory will check, and clean it up if it needs to
        CleanupInventory(_sourceInv);

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
    public bool PlaceInventory(Job _job, Inventory _sourceInv)
    {
        if(_job.inventoryRequirments.ContainsKey(_sourceInv.objectType) == false)
        {
            Debug.LogError("Trying to add inventory to a job that doesn't want it");
            return false;
        }

        _job.inventoryRequirments[_sourceInv.objectType].StackSize += _sourceInv.StackSize;

        if(_job.inventoryRequirments[_sourceInv.objectType].StackSize > _job.inventoryRequirments[_sourceInv.objectType].maxStackSize)
        {
            _sourceInv.StackSize = _job.inventoryRequirments[_sourceInv.objectType].StackSize - _job.inventoryRequirments[_sourceInv.objectType].maxStackSize;
            _job.inventoryRequirments[_sourceInv.objectType].StackSize = _job.inventoryRequirments[_sourceInv.objectType].maxStackSize;
        }
        else
        {
            _sourceInv.StackSize = 0;
        }

        //At this point _inv might be empty if it was all taken by the job. CleanupInventory will check, and clean it up if it needs to
        CleanupInventory(_sourceInv);

        return true;
    }
    public bool PlaceInventory(Character _character, Inventory _sourceInv, int _amount = -1)
    {
        if (_amount < 0)
            _amount = _sourceInv.StackSize;
        else
        {
            //Amount will equal whatever is smaller, so if we have less than what we're asking for, it wont grab more than possible
            _amount = Mathf.Min(_amount, _sourceInv.StackSize);
        }

        if(_character.inventory == null)
        {
            _character.inventory = _sourceInv.Clone();
            _character.inventory.StackSize = 0;

            inventoryStacks[_character.inventory.objectType].Add(_character.inventory);
        }
        else if (_character.inventory.objectType != _sourceInv.objectType)
        {
            Debug.LogError("InventoryManager -- PlaceInventory: Character is trying to pick up mismatched inventory object type");
            return false;
        }

        _character.inventory.StackSize += _sourceInv.StackSize;

        if (_character.inventory.StackSize > _character.inventory.maxStackSize)
        {
            _sourceInv.StackSize = _character.inventory.StackSize - _character.inventory.maxStackSize;
            _character.inventory.StackSize = _character.inventory.maxStackSize;
        }
        else
        {
            _sourceInv.StackSize -= _amount;
        }

        //At this point _inv might be empty if it was all taken by the character. CleanupInventory will check, and clean it up if it needs to
        CleanupInventory(_sourceInv);

        return true;
    }


    /// <summary>
    /// Get the closet inventory of a type
    /// </summary>
    /// <param name="_objectType"></param>
    /// <param name="_t"></param>
    /// <param name="desiredAmount">Desired Amount. If no stack has enough, it returns the largest</param>
    /// <returns></returns>
    public Inventory GetClosestInventoryOfType(string _objectType, Tile _t, int desiredAmount)
    {
        // ! This does NOT return the closest inventory right now, it only returns the first valid inventory it finds!

        if (inventoryStacks.ContainsKey(_objectType) == false)
        {
            Debug.Log("InventoryManager -- GetClosestInventoryOfType: No items of desired type");
            return null;
        }

        foreach (Inventory inv in inventoryStacks[_objectType])
        {
            if(inv.tile != null)
            {
                return inv;
            }
        }

        return null;
    }

}
