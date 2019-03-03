using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSpriteController : MonoBehaviour {

    InstalledObjectSpriteController installedObjectSpriteController;
    //Dictionary to connect Jobs with their GameObjects
    Dictionary<Job, GameObject> jobGameObjectMap;

	// Use this for initialization
	void Start () {
        installedObjectSpriteController = FindObjectOfType<InstalledObjectSpriteController>();
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        WorldController.Instance.world.jobQueue.RegisterJobCreationCallback(OnJobCreated);

	}

    void OnJobCreated(Job _job)
    {
        if(_job.jobObjectType == null)
        {
            //This job does not have a sprite, so dont render it
            return;
        }


        if (jobGameObjectMap.ContainsKey(_job))
        {
            Debug.LogError("JobSpriteController -- OnJobCreated: Called for a job gameobject that already exists. Most likely from a job being re-queued, not one being created");
            return;
        }

        GameObject job_GO = new GameObject();

        //add the Object/GameObject pair to dictionary
        jobGameObjectMap.Add(_job, job_GO);

        job_GO.name = "JOB_" + _job.jobObjectType + "_" + _job.tile.X + "," + _job.tile.Y;
        job_GO.transform.position = new Vector3(_job.tile.X, _job.tile.Y, 0);
        job_GO.transform.SetParent(transform, true);

        //add a sprite renderer
        SpriteRenderer sr = job_GO.AddComponent<SpriteRenderer>();
        sr.sprite = installedObjectSpriteController.GetSpriteForInstalledObject(_job.jobObjectType);
        sr.sortingLayerName = "Jobs";
        //keep the green the same, but reduce the other colors and change the alpha to 25% to add transparency,
        sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);


        if (_job.jobObjectType == "Door")
        {
            //By default, door graphic is made for walls to east/west, let check to see if there are walls north/south, and if so, rotate the GO

            Tile northTile = _job.tile.World.GetTileAt(_job.tile.X, _job.tile.Y + 1);
            Tile southTile = _job.tile.World.GetTileAt(_job.tile.X, _job.tile.Y - 1);
            if (northTile != null && southTile != null && northTile.InstalledObject != null && southTile.InstalledObject != null
                && northTile.InstalledObject.ObjectType == "Wall" && southTile.InstalledObject.ObjectType == "Wall")
            {
                job_GO.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
            }

        }


        _job.RegisterJobCompleteCallback(OnJobEnded);
        _job.RegisterJobCancelCallback(OnJobEnded);

    }

    //For when a job is completed or cancelled
    void OnJobEnded(Job _j)
    {
        GameObject job_GO = jobGameObjectMap[_j];
        _j.UnregisterJobCompleteCallback(OnJobEnded);
        _j.UnregisterJobCancelCallback(OnJobEnded);
        Destroy(job_GO);

    }


}
