using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WorldController : MonoBehaviour {
    
    public static WorldController Instance { get; protected set; }

    public Sprite emptySprite;
    public Sprite floorSprite;

    //Keep track of Tiles and their GameObjects
    Dictionary<Tile, GameObject> tileGameObjectMap;
    //Keep track of InstalledObjcts and their GameObjects
    Dictionary<InstalledObject, GameObject> installedObjectGameObjectMap;

    Dictionary<string, Sprite> installedObjectSprites;

    public World World { get; protected set; }

    void Start() {
        if(Instance != null)
        {
            Debug.LogError("There is more than one WorldController!");
        }
        Instance = this;

        LoadSpritesFromResources();

        //create a world with empty tiles
        World = new World();
        World.RegisterInstalledObjectCreated(OnInstalledObjectCreated);

        //Instantiate the dictionary to track the Tiles with their GameObjects
        tileGameObjectMap = new Dictionary<Tile, GameObject>();
        installedObjectGameObjectMap = new Dictionary<InstalledObject, GameObject>();
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

                //add a sprite renderer
                tile_GO.AddComponent<SpriteRenderer>().sprite = emptySprite;

                //register callback so our GameObject gets updated
                //this tells the Tile the function it should run (OnTileTypeChanged) when it's Type changed
                tile_data.RegisterTileTypeChangedCallback(OnTileTypeChanged);
            }
        }
        //Center the camera
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, -10);


        //World.RandomizeTiles();
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

    void LoadSpritesFromResources()
    {
        installedObjectSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Art/InstalledObjects/");

        foreach (Sprite s in sprites)
        {
            Debug.Log(s);
            installedObjectSprites[s.name] = s;
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

    public void OnInstalledObjectCreated(InstalledObject _obj)
    {
        //create a visual GameObject linked to this data
        GameObject obj_GO = new GameObject();
        //add the Object/GameObject pair to dictionary
        installedObjectGameObjectMap.Add(_obj, obj_GO);

        obj_GO.name = _obj.ObjectType + "_" + _obj.Tile.X + "," + _obj.Tile.Y;
        obj_GO.transform.position = new Vector3(_obj.Tile.X, _obj.Tile.Y, 0);
        obj_GO.transform.SetParent(transform, true);

        //add a sprite renderer
        obj_GO.AddComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(_obj);
        obj_GO.GetComponent<SpriteRenderer>().sortingLayerName = "InstalledObjects";
        //register callback so our GameObject gets updated
        _obj.RegisterOnChangedCallback(OnInstalledObjectChanged);
    }

    void OnInstalledObjectChanged(InstalledObject _obj)
    {

        if(installedObjectGameObjectMap.ContainsKey(_obj) == false)
        {
            Debug.LogError("OnInstalledObjectChanged -- Trying to change a InstalledObject that is not in the map");
            return;
        }

        GameObject obj_GO = installedObjectGameObjectMap[_obj];
        obj_GO.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(_obj);
    }

    Sprite GetSpriteForInstalledObject(InstalledObject _obj)
    {
        if (_obj.LinksToNeighbour == false)
        {
            return installedObjectSprites[_obj.ObjectType];
        }
        string spriteName = _obj.ObjectType + "_";

        //This region checks for neighbors and remanes the sprite accorrdingly
        #region NeighbourCheck
        //check for neighbours North, East, South, West
        int x = _obj.Tile.X;
        int y = _obj.Tile.Y;
        Tile t;

        //This code will see if it has neighbours next to it, and if so add a letter to the name
        //For example, a wall with wall above and below it will be first be named Wall_N then Wall_NS
        t = World.GetTileAt(x, y + 1);
        //if there is a tile above us, it has an object on it, and that object matches ours
        if(t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == _obj.ObjectType)
        {
            spriteName += "N";
        }
        
        t = World.GetTileAt(x + 1, y);
        //if there is a tile to the right, it has an object on it, and that object matches ours
        if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == _obj.ObjectType)
        {
            spriteName += "E";
        }

        t = World.GetTileAt(x, y - 1);
        //if there is a tile below us, it has an object on it, and that object matches ours
        if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == _obj.ObjectType)
        {
            spriteName += "S";
        }

        t = World.GetTileAt(x - 1, y);
        //if there is a tile to the left, it has an object on it, and that object matches ours
        if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == _obj.ObjectType)
        {
            spriteName += "W";
        }

        //return name that matches the sprite name
        if (installedObjectSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("GetSpriteForInstalledObject -- The Sprite for the" + spriteName + "object does not exist!");
            return null;
        }
        return installedObjectSprites[spriteName];
        #endregion
    }



}
