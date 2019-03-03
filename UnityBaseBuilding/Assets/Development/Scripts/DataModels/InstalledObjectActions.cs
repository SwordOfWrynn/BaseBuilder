using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InstalledObjectActions
{

    public static void Door_UpdateAction(InstalledObject _inObj, float _deltaTime)
    {
        //if (Debug.isDebugBuild)
        //    Debug.Log("Door_UpdateAction: " + _inObj.inObjParameters["openness"]);

        if (_inObj.GetParameter("is_opening") >= 1)
        {
            _inObj.ChangeParameter("openness", _deltaTime * 4);
            if (_inObj.GetParameter("openness") >= 1)
            {
                _inObj.SetParameter("is_opening", 0);
            }
        }
        else
        {
            _inObj.ChangeParameter("openness", -_deltaTime * 4);
        }

        _inObj.SetParameter("openness", Mathf.Clamp01(_inObj.GetParameter("openness")));

        if (_inObj.cbOnChanged != null)
            _inObj.cbOnChanged(_inObj);
    }

    public static ENTERABILITY Door_IsEnterable(InstalledObject _inObj)
    {
        //if(Debug.isDebugBuild)
        //    Debug.Log("Door_IsEnterable");

        _inObj.SetParameter("is_opening", 1);

        if (_inObj.GetParameter("openness") >= 1)
        {
            return ENTERABILITY.Yes;
        }
        return ENTERABILITY.Soon;
    }

    public static void JobCompleteInstalledObjectBuild(Job _theJob)
    {
        WorldController.Instance.world.PlaceInstalledObject(_theJob.jobObjectType, _theJob.tile);
        _theJob.tile.pendingInstalledObjectJob = null;
    }

    public static void Stockpile_UpdateAction(InstalledObject _inObj, float _deltaTime)
    {
        //we need to make sure that there is always a job to bring inventory to us if we aren't empty

        //if we already have a job, return
        if (_inObj.JobCount() > 0)
            return;

        if (_inObj.Tile.Inventory == null)
        {
            //We are empty, ask for anything to be brought here
            Job j = new Job(_inObj.Tile, null, Stockpile_JobComplete, 0, new Inventory[1] { new Inventory("Steel Plate", 50, 0) }); //need to be able to ask for any type

            _inObj.AddJob(j);
        }
    }

    public static void Stockpile_JobComplete(Job _j)
    {
        _j.tile.InstalledObject.RemoveJob(_j);
        foreach (Inventory inv in _j.inventoryRequirments)
        {
            if (inv.stackSize > 0)
                _j.tile.World.inventoryManager.PlaceInventory(_j.tile, inv);
            return;
        }
    }

}
