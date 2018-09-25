using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour {

    public GameObject circleCursorPrefab;

    bool buildModeIsObjects = false;

    TileType buildModeTile = TileType.Floor;
    string buildModeObjectType;

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
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        //handle left mouse
        //start drag
        //for mobile have sense tapping, tap start tap end
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
            SimplePool.Despawn(go);
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
                        GameObject go = SimplePool.Spawn(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        go.transform.SetParent(transform, true);
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
                        if (buildModeIsObjects)
                        {
                            //We are installing objects
                            //Create the Installed object instantly and assign it to the tile
                            //WorldController.Instance.World.PlaceInstalledObject(buildModeObjectType, t);

                            //Can we build it at the tile (there isn't already something on it, or going to be build on it)
                            string InstalledObjectType = buildModeObjectType;
                             
                            if (WorldController.Instance.World.IsInstalledObjectPlacementValid(InstalledObjectType, t) && t.pendingInstalledObjectJob == null)
                            {

                                //this uses a lambda(t, (theJob) => { WorldController.Instance.World.PlaceInstalledObject(buildModeObjectType, theJob.Tile);})
                                // theJob is a mini function used for the Job callback (because the callback wants an Action<Job>), and it call the PlaceInstalledObject function

                                Job j = new Job(t, (theJob) =>
                                {
                                    WorldController.Instance.World.PlaceInstalledObject(InstalledObjectType, theJob.Tile);
                                    t.pendingInstalledObjectJob = null;
                                }
                                );

                                t.pendingInstalledObjectJob = j;
                                j.RegisterJobCancelCallback((theJob) => { theJob.Tile.pendingInstalledObjectJob = null; });
                                //Queue up the job
                                WorldController.Instance.World.jobQueue.Enqueue(j);
                                Debug.Log("Job Queue Size: " + WorldController.Instance.World.jobQueue.Count);
                            }
                        }
                        else
                        {
                            //We are changing tile types
                            t.Type = buildModeTile;
                        }
                    }
                }
            }
        }
    }

    void OnInstalledObjectJobComplete(string InstalledObjectType, Tile t)
    {
        WorldController.Instance.World.PlaceInstalledObject(InstalledObjectType, t);
    }


    void UpdateCameraMovement()
    {
        //handle screen movement
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1)) //middle or right mouse buttons
        {
            Vector3 diff = lastFramePos - currentFramePos;
            Camera.main.transform.Translate(diff);
        }
        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 25f );
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
