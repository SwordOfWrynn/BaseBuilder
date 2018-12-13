using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseOverInstalledObjectText : MonoBehaviour {

    //Everyframe, the script checks to see which tile is under the mouse, then updates the GetCompnent<Text>().text parameter

    Text myText;
    MouseController mouseController;

	// Use this for initialization
	void Start () {
        myText = GetComponent<Text>();

        if (myText == null)
        {
            Debug.LogError("MouseOverTileTypeText -- Start: No 'Text' Ui component on this object.");
            this.enabled = false;
            return;
        }

        mouseController = GameObject.FindObjectOfType<MouseController>();
        if(mouseController == null)
        {
            Debug.LogError("MouseOverTileTypeText -- Start: There is no MouseController in the scene!.");
            this.enabled = false;
            return;
        }
	}
	
	// Update is called once per frame
	void Update () {
        Tile t = mouseController.MouseOverTile();

        string s = "None";
        if (t.InstalledObject != null)
        {
            s = t.InstalledObject.ObjectType;
        }

        myText.text = "Installed Object: " + s;
    }
}
