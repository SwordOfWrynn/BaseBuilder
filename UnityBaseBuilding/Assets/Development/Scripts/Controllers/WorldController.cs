using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WorldController : MonoBehaviour {
    
    public static WorldController Instance { get; protected set; }

    public World world { get; protected set; }

    void Awake() {
        if(Instance != null)
        {
            Debug.LogError("There is more than one WorldController!");
        }
        Instance = this;

        //create a world with empty tiles
        world = new World();



        //Center the camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, -10);
        
    }

    //returns the tile at the given coordinates
    public Tile GetTileAtWorldCoord(Vector3 _coord)
    {
        int x = Mathf.FloorToInt(_coord.x);
        int y = Mathf.FloorToInt(_coord.y);

        return world.GetTileAt(x, y);
    }

}
