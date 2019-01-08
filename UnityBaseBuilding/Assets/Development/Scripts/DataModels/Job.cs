using System;
using System.Collections.Generic;
using UnityEngine;

public class Job {

    //this class will hold info for a queued up job, which can include things like placing InstalledObjects,
    //moving inventory, working at a dest, and mabye fighting enemies.

    public Tile Tile { get; protected set; }
    float jobTime;

    public string jobObjectType { get; protected set; }

    Action<Job> cbJobComplete;
    Action<Job> cbJobCancelled;

    Dictionary<string, Inventory> inventoryRequirments; //the max stack size of inventory in this dictionary is the required amount
    
    public Job(Tile _tile, string _jobObjectType, Action<Job> _cbJobComplete, float _jobTime, Inventory[] _inventoryRequirments)
    {
        Tile = _tile;
        cbJobComplete += _cbJobComplete;
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

}
