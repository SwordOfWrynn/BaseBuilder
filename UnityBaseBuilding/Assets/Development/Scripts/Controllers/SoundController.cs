using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {

    float soundCooldown = 0f;

	// Use this for initialization
	void Start () {
        WorldController.Instance.World.RegisterInstalledObjectCreated(OnInstalledObjectCreated);
        WorldController.Instance.World.RegisterTileChanged(OnTileChanged);
    }

    void Update()
    {
        if (soundCooldown <= 0)
        {
            return;
        }
        soundCooldown -= Time.deltaTime;
    }

    void OnTileChanged(Tile _tileData)
    {
        if (soundCooldown > 0)
        {
            return;
        }

        AudioClip ac = Resources.Load<AudioClip>("Sound/Floor_OnCreated");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = 0.1f;
    }

    void OnInstalledObjectCreated(InstalledObject obj)
    {
        if (soundCooldown > 0)
        {
            return;
        }

        AudioClip ac = Resources.Load<AudioClip>("Sound/" + obj.ObjectType + "_OnCreated");
        if (ac == null)
        {
            //no specific sound found, play a default sound
            ac = Resources.Load<AudioClip>("Sound/Wall_OnCreated");
        }
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = 0.1f;
    }
}
