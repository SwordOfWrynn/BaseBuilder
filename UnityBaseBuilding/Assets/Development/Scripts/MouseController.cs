using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

    public GameObject circleCursor;

    Vector3 lastFramePos;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 currentFramePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentFramePos.z = 0;

        //Update circle cursor position;
        circleCursor.transform.position = currentFramePos;

        //handle screen movement
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1)) //middle or right mouse buttons
        {
            Vector3 diff = lastFramePos - currentFramePos;
            Camera.main.transform.Translate(diff);
        }
        lastFramePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePos.z = 0;
    }
}
