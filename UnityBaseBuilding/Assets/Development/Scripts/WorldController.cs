using UnityEngine;
using System;

public class WorldController : MonoBehaviour {
    
    public static WorldController Instance { get; protected set; }

    public Sprite floorSprite;


    public World World { get; protected set; }

    void Start() {
        if(Instance != null)
        {
            Debug.LogError("There is more than one WorldController!");
        }
        Instance = this;

        //create a world with empty tiles
        World = new World();

        //create a Gameobject for each tile to show visuals
        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                Tile tile_data = World.GetTileAt(x, y);

                GameObject tile_GO = new GameObject();
                tile_GO.name = "Tile_" + x + "," + y;
                tile_GO.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);
                tile_GO.transform.SetParent(this.transform, true);

                //add a sprite renderer, but keep it empty for now
                tile_GO.AddComponent<SpriteRenderer>();

                tile_data.RegisterTileTypeChangedCallback((tile) => { OnTileTypeChanged(tile, tile_GO); } );
            }
        }
        World.RandomizeTiles();
    }

    void OnTileTypeChanged(Tile tile_data, GameObject tile_GO)
    {
        if (tile_data.Type == Tile.TileType.Floor)
        {
            tile_GO.GetComponent<SpriteRenderer>().sprite = floorSprite;
        }
        else if (tile_data.Type == Tile.TileType.Empty)
        {
            tile_GO.GetComponent<SpriteRenderer>().sprite = null;
        }
        else
        {
            Debug.LogError("OnTileTypeChanged - Unrecognized tile type!");
        }
    }

}
