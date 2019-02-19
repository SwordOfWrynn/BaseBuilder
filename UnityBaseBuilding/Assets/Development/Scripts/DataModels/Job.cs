using System;
using System.Collections.Generic;
using UnityEngine;

public class Job {

    //this class will hold info for a queued up job, which can include things like placing InstalledObjects,
    //moving inventory, working at a dest, and mabye fighting enemies.

    public Tile tile;
    float jobTime;

    public string jobObjectType { get; protected set; }

    Action<Job> cbJobComplete;
    Action<Job> cbJobCancelled;

    public Dictionary<string, Inventory> inventoryRequirments; //the max stack size of inventory in this dictionary is the required amount
    
    public Job(Tile _tile, string _jobObjectType, Action<Job> _cbJobComplete, float _jobTime, Inventory[] _inventoryRequirments)
    {
        tile = _tile;
        cbJobComplete += _cbJobComplete;
        //cbJobCancelled += _cbJobCancelled;
        jobObjectType = _jobObjectType;
        jobTime = _jobTime;

        inventoryRequirments = new Dictionary<string, Inventory>();
        if (_inventoryRequirments != null) //if no material is needed, null will be passed in for requirments
        {
            foreach (Inventory inv in _inventoryRequirments)
            {
                inventoryRequirments[inv.objectType] = inv.Clone();
            }
        }
    }

    protected Job(Job _other)
    {
        tile = _other.tile;
        cbJobComplete = _other.cbJobComplete;
        //cbJobCancelled = _other.cbJobCancelled;
        jobObjectType = _other.jobObjectType;
        jobTime = _other.jobTime;

        inventoryRequirments = new Dictionary<string, Inventory>();
        if (_other.inventoryRequirments != null) //if no material is needed, null will be passed in for requirments
        {
            foreach (Inventory inv in _other.inventoryRequirments.Values)
            {
                inventoryRequirments[inv.objectType] = inv.Clone();
            }
        }
    }

    virtual public Job Clone()
    {
        return new Job(this);
    }

    public void DoWork(float _workTime)
    {
        jobTime -= _workTime;

        if(jobTime <= 0)
        {
            if(cbJobComplete != null)
            {
                cbJobComplete(this);
            }
        }
    }

    public void CancelJob()
    {
        if(cbJobCancelled != null)
        {
            cbJobCancelled(this);
        }
    }

    public bool HasAllMaterial()
    {
        foreach(Inventory inv in inventoryRequirments.Values)
        {
            if (inv.maxStackSize > inv.StackSize)
                return false;
        }
        return true;
    }

    public int NeedsInventoryType(Inventory inv)
    {
        if(inventoryRequirments.ContainsKey(inv.objectType) == false){
            //We don't want this
            return 0;
        }
        if(inventoryRequirments[inv.objectType].StackSize >= inventoryRequirments[inv.objectType].maxStackSize){
            //We already have enough
            return 0;
        }
        //we want it and need more
        return inventoryRequirments[inv.objectType].maxStackSize - inventoryRequirments[inv.objectType].StackSize;
    }

    public void RegisterJobCompleteCallback(Action<Job> _cb)
    {
        cbJobComplete += _cb;
    }
    public void RegisterJobCancelCallback(Action<Job> _cb)
    {
        cbJobCancelled += _cb;
    }

    public void UnregisterJobCompleteCallback(Action<Job> _cb)
    {
        cbJobComplete -= _cb;
    }
    public void UnregisterJobCancelCallback(Action<Job> _cb)
    {
        cbJobCancelled -= _cb;
    }

    public Inventory GetFirstNeededInventory()
    {
        foreach (Inventory inv in inventoryRequirments.Values)
        {
            if (inv.StackSize < inv.maxStackSize)
                return inv;
        }
        //No more inventory is needed
        return null;
    }
}
