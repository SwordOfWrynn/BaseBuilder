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

    void OnJobCreated(Job _j)
    {

        if (jobGameObjectMap.ContainsKey(_j))
        {
            Debug.LogError("JobSpriteController -- OnJobCreated: Called for a job gameobject that already exists. Most likely from a job being re-queued, not one being created");
            return;
        }

        GameObject job_GO = new GameObject();

        //add the Object/GameObject pair to dictionary
        jobGameObjectMap.Add(_j, job_GO);

        job_GO.name = "JOB_" + _j.jobObjectType + "_" + _j.Tile.X + "," + _j.Tile.Y;
        job_GO.transform.position = new Vector3(_j.Tile.X, _j.Tile.Y, 0);
        job_GO.transform.SetParent(transform, true);

        //add a sprite renderer
        SpriteRenderer sr = job_GO.AddComponent<SpriteRenderer>();
        sr.sprite = installedObjectSpriteController.GetSpriteForInstalledObject(_j.jobObjectType);
        sr.sortingLayerName = "Jobs";
        //keep the green the same, but reduce the other colors and change the alpha to 25% to add transparency,
        sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);

        _j.RegisterJobCompleteCallback(OnJobEnded);
        _j.RegisterJobCancelCallback(OnJobEnded);

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
