using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

    public GameObject circleCursorPrefab;
    Vector3 currentFramePos;
    Vector3 lastFramePos;
    Vector3 dragStartPos;
    List<GameObject> dragPreviewGameObjects;

	// Use this for initialization
	void Start () {
        dragPreviewGameObjects = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {

        currentFramePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentFramePos.z = 0;

        //UpdateCursor();
        UpdateCameraMovement();
        UpdateDragging();
        
        //save mouse position from this frame
        //dont use currentFramePos because we might have moved
        lastFramePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePos.z = 0;
    }

    /*void UpdateCursor()
    {
        //Update circle cursor position;
        Tile tileUnderMouse = WorldController.Instance.GetTileAtWorldCoord(currentFramePos);
        if (tileUnderMouse != null)
        {
            circleCursor.SetActive(true);
            Vector3 cursorPosition = new Vector3(tileUnderMouse.X, tileUnderMouse.Y, 0);
            circleCursor.transform.position = cursorPosition;
        }
        else
        {
            circleCursor.SetActive(false);
        }
    }*/

    void UpdateDragging()
    {
        //handle left mouse
        //start drag
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = currentFramePos;
        }

        int start_x = Mathf.FloorToInt(dragStartPos.x);
        int end_x = Mathf.FloorToInt(currentFramePos.x);
        if (end_x < start_x)
        {
            int tmp = end_x;
            end_x = start_x;
            start_x = tmp;
        }
        int start_y = Mathf.FloorToInt(dragStartPos.y);
        int end_y = Mathf.FloorToInt(currentFramePos.y);
        if (end_y < start_y)
        {
            int tmp = end_y;
            end_y = start_y;
            start_y = tmp;
        }
        //clean up old previews each frame
        while (dragPreviewGameObjects.Count > 0)
        {
            GameObject go = dragPreviewGameObjects[0];
            dragPreviewGameObjects.RemoveAt(0);
            Destroy(go);
        }

        if (Input.GetMouseButton(0))
        {
            //Display drag area preview
            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(x, y);
                    if (t != null)
                    {
                        //display building hint at this position
                        GameObject go = Instantiate(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        dragPreviewGameObjects.Add(go);
                    }
                }
            }
        }


        //end drag
        if (Input.GetMouseButtonUp(0))
        {
            //loop through the tiles
            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(x, y);
                    if (t != null)
                    {
                        t.Type = Tile.TileType.Floor;
                    }
                }
            }
        }
    }

    void UpdateCameraMovement()
    {
        //handle screen movement
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1)) //middle or right mouse buttons
        {
            Vector3 diff = lastFramePos - currentFramePos;
            Camera.main.transform.Translate(diff);
        }
    }

}
