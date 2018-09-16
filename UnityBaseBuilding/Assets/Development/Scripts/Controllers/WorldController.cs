using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WorldController : MonoBehaviour {
    
    public static WorldController Instance { get; protected set; }

    public Sprite floorSprite;
    //Keep track of Tiles and their GameObjects
    Dictionary<Tile, GameObject> tileGameObjectMap;

    public World World { get; protected set; }

    void Start() {
        if(Instance != null)
        {
            Debug.LogError("There is more than one WorldController!");
        }
        Instance = this;

        //create a world with empty tiles
        World = new World();
        //Instantiate the dictionary to track the Tiles with their GameObjects
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        //create a Gameobject for each tile to show visuals
        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                Tile tile_data = World.GetTileAt(x, y);

                GameObject tile_GO = new GameObject();
                //add the Tile/GameObject pair to dictionary
                tileGameObjectMap.Add(tile_data, tile_GO);

                tile_GO.name = "Tile_" + x + "," + y;
                tile_GO.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);
                tile_GO.transform.SetParent(transform, true);

                //add a sprite renderer, but keep it empty for now
                tile_GO.AddComponent<SpriteRenderer>();

                //register callback so our GameObject gets updated
                //this tells the Tile the function it should run (OnTileTypeChanged) when it's Type changed
                tile_data.RegisterTileTypeChangedCallback(OnTileTypeChanged);
            }
        }
        World.RandomizeTiles();
    }
    //Example
    void DestroyAllTileGameObjects()
    {
        //destroy the gameobjects but not the data
        while(tileGameObjectMap.Count > 0)
        {
            Tile tile_data = tileGameObjectMap.Keys.First();
            GameObject tile_GO = tileGameObjectMap[tile_data];
            //remove the pait from dictionary
            tileGameObjectMap.Remove(tile_data);
            //unregister the callback
            tile_data.UnRegisterTileTypeChangedCallback(OnTileTypeChanged);
            //Destroy the GameObject
            Destroy(tile_GO);
        }
    }

    void OnTileTypeChanged(Tile tile_data)
    {

        if(!tileGameObjectMap.ContainsKey(tile_data))
        {
            Debug.LogError("tileGameObjectMap doesn't contain the tile_data. Make sure it is added to the dictionary and the callback is unregistered");
            return;
        }

        GameObject tile_GO = tileGameObjectMap[tile_data];

        if(tile_GO == null)
        {
            Debug.LogError("tileGameObjectMap's returned GameObject is null. Make sure it is added to the dictionary and the callback is unregistered");
            return;
        }

        if (tile_data.Type == TileType.Floor)
        {
            tile_GO.GetComponent<SpriteRenderer>().sprite = floorSprite;
        }
        else if (tile_data.Type == TileType.Empty)
        {
            tile_GO.GetComponent<SpriteRenderer>().sprite = null;
        }
        else
        {
            Debug.LogError("OnTileTypeChanged - Unrecognized tile type!");
        }
    }

    //returns the tile at the given coordinates
    public Tile GetTileAtWorldCoord(Vector3 _coord)
    {
        int x = Mathf.FloorToInt(_coord.x);
        int y = Mathf.FloorToInt(_coord.y);

        return World.GetTileAt(x, y);
    }
}
