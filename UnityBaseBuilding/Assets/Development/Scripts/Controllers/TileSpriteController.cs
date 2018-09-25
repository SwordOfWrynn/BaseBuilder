using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileSpriteController : MonoBehaviour {
    
    
    public Sprite emptySprite;
    public Sprite floorSprite;

    //Keep track of Tiles and their GameObjects
    Dictionary<Tile, GameObject> tileGameObjectMap;

    World world { get { return WorldController.Instance.world; } }

    void Start() {

        //Instantiate the dictionary to track the Tiles with their GameObjects
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        //create a Gameobject for each tile to show visuals
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                Tile tile_data = world.GetTileAt(x, y);

                GameObject tile_GO = new GameObject();
                //add the Tile/GameObject pair to dictionary
                tileGameObjectMap.Add(tile_data, tile_GO);

                tile_GO.name = "Tile_" + x + "," + y;
                tile_GO.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);
                tile_GO.transform.SetParent(transform, true);

                //add a sprite renderer
                tile_GO.AddComponent<SpriteRenderer>().sprite = emptySprite;

                //register callback so our GameObject gets updated
                //this tells the Tile the function it should run (OnTileChanged) when it's Type changed

            }
        }

        world.RegisterTileChanged(OnTileChanged);

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
            tile_data.UnRegisterTileTypeChangedCallback(OnTileChanged);
            //Destroy the GameObject
            Destroy(tile_GO);
        }
    }


    //should be called whenever a tile's data is changed
    void OnTileChanged(Tile tile_data)
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
            tile_GO.GetComponent<SpriteRenderer>().sprite = emptySprite;
        }
        else
        {
            Debug.LogError("OnTileTypeChanged - Unrecognized tile type!");
        }
    }
}
