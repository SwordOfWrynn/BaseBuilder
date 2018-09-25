using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSpriteController : MonoBehaviour {

    InstalledObjectSpriteController installedObjectSpriteController;

	// Use this for initialization
	void Start () {
        installedObjectSpriteController = FindObjectOfType<InstalledObjectSpriteController>();

        WorldController.Instance.world.jobQueue.RegisterJobCreationCallback(OnJobCreated);

	}

    void OnJobCreated(Job _j)
    {
        Sprite theSprite = installedObjectSpriteController.GetSpriteForInstalledObject(WorldController.Instance.world.GetInstalledObjectPrototype(_j.jobObjectType));

        _j.RegisterJobCompleteCallback(OnJobEnded);
        _j.RegisterJobCancelCallback(OnJobEnded);

    }

    //For when a job is completed or cancelled
    void OnJobEnded(Job _j)
    {

    }


}
