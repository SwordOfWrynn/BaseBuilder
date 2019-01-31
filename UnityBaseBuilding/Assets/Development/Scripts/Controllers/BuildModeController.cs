using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildModeController : MonoBehaviour {

    bool buildModeIsObjects = false;

    TileType buildModeTile = TileType.Floor;
    string buildModeObjectType;

	// Use this for initialization
	void Start () {
        
	}

    void OnInstalledObjectJobComplete(string InstalledObjectType, Tile t)
    {
        WorldController.Instance.world.PlaceInstalledObject(InstalledObjectType, t);
    }

    public void DoPathfindingTest()
    {
        WorldController.Instance.world.SetupPathfindingExample();
    }

    public void DoBuild (Tile _t)
    {
        if (buildModeIsObjects)
        {
            //We are installing objects

            //This will create the Installed object instantly and assign it to the tile
            //WorldController.Instance.World.PlaceInstalledObject(buildModeObjectType, t);

            //Can we build it at the tile (there isn't already something on it, or going to be build on it)
            string installedObjectType = buildModeObjectType;

            if (WorldController.Instance.world.IsInstalledObjectPlacementValid(installedObjectType, _t) && _t.pendingInstalledObjectJob == null)
            {

                //this uses a lambda(_t, installedObjectType, "(theJob) => { WorldController.Instance.World.PlaceInstalledObject(buildModeObjectType, theJob.Tile);})"
                //it is a mini function used for the Job callback (because the callback wants an Action<Job>), and it call the PlaceInstalledObject function

                Job j;

                if (WorldController.Instance.world.installedObjectJobPrototypes.ContainsKey(installedObjectType))
                {
                    //Make a clone of the job prototype, and assign the correct tile
                    j = WorldController.Instance.world.installedObjectJobPrototypes[installedObjectType].Clone();
                    j.tile = _t;
                }
                else
                {
                    Debug.LogErrorFormat("There is no InstalledObject job prototype for {0}. Using the testing default", installedObjectType);
                    j = new Job(_t, installedObjectType, InstalledObjectActions.JobCompleteInstalledObjectBuild, 0.1f, null);
                }

                _t.pendingInstalledObjectJob = j;
                j.RegisterJobCancelCallback((theJob) => { theJob.tile.pendingInstalledObjectJob = null; });

                //Queue up the job
                WorldController.Instance.world.jobQueue.Enqueue(j);
            }
        }
        else
        {
            //We are changing tile types
            _t.Type = buildModeTile;
        }
    }

    public void SetMode_BuildFloor()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Floor;
    }

    public void SetMode_Destroy()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Empty;
    }
    //Wall is not a Tile, it is an InstalledObject
    public void SetMode_BuildInstalledObject(string _objectType)
    {
        buildModeIsObjects = true;
        buildModeObjectType = _objectType;
    }

}
