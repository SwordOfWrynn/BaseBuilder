using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InstalledObjectSpriteController : MonoBehaviour {
    
     //Keep track of InstalledObjcts and their GameObjects
    Dictionary<InstalledObject, GameObject> installedObjectGameObjectMap;

    Dictionary<string, Sprite> installedObjectSprites;

    World world { get { return WorldController.Instance.world; } }

    void Start() {

        LoadSpritesFromResources();
        
        installedObjectGameObjectMap = new Dictionary<InstalledObject, GameObject>();
        
        world.RegisterInstalledObjectCreated(OnInstalledObjectCreated);

        foreach(InstalledObject inObj in world.installedObjects)
        {
            OnInstalledObjectCreated(inObj);
        }
    }

    void LoadSpritesFromResources()
    {
        installedObjectSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Art/InstalledObjects/");

        foreach (Sprite s in sprites)
        {
            installedObjectSprites[s.name] = s;
        }
    }

    public void OnInstalledObjectCreated(InstalledObject _inObj)
    {
        //create a visual GameObject linked to this data
        GameObject inObj_GO = new GameObject();

        //add the Object/GameObject pair to dictionary
        installedObjectGameObjectMap.Add(_inObj, inObj_GO);

        inObj_GO.name = _inObj.ObjectType + "_" + _inObj.Tile.X + "," + _inObj.Tile.Y;
        inObj_GO.transform.position = new Vector3(_inObj.Tile.X, _inObj.Tile.Y, 0);
        inObj_GO.transform.SetParent(transform, true);

        if (_inObj.ObjectType == "Door")
        {
            //By default, door graphic is made for walls to east/west, let check to see if there are walls north/south, and if so, rotate the GO

            Tile northTile = world.GetTileAt(_inObj.Tile.X, _inObj.Tile.Y + 1);
            Tile southTile = world.GetTileAt(_inObj.Tile.X, _inObj.Tile.Y - 1);
            if (northTile != null && southTile != null && northTile.InstalledObject != null && southTile.InstalledObject != null
                && northTile.InstalledObject.ObjectType == "Wall" && southTile.InstalledObject.ObjectType == "Wall")
            {
                inObj_GO.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                inObj_GO.gameObject.transform.Translate(1f, 0f, 0f, Space.World);
            }

        }

        //add a sprite renderer
        SpriteRenderer sr = inObj_GO.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForInstalledObject(_inObj);
        sr.sortingLayerName = "InstalledObjects";

        //register callback so our GameObject gets updated
        _inObj.RegisterOnChangedCallback(OnInstalledObjectChanged);
    }

    void OnInstalledObjectChanged(InstalledObject _inObj)
    {

        if(installedObjectGameObjectMap.ContainsKey(_inObj) == false)
        {
            Debug.LogError("OnInstalledObjectChanged -- Trying to change a InstalledObject that is not in the map");
            return;
        }

        GameObject obj_GO = installedObjectGameObjectMap[_inObj];
        obj_GO.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(_inObj);

        
    }

    public Sprite GetSpriteForInstalledObject(InstalledObject _inObj)
    {
            string spriteName = _inObj.ObjectType;

        if (_inObj.LinksToNeighbour == false)
        {
            //if this is a door check openness and update sprite
            if (_inObj.ObjectType == "Door")
            {
                if (_inObj.inObjParameters["openness"] < 0.1f)
                {
                    //Door is closed
                    spriteName = "Door";
                }
                else if (_inObj.inObjParameters["openness"] < 0.5f)
                {
                    //Door is open a little bit
                    spriteName = "Door_openness_1";
                }
                else if (_inObj.inObjParameters["openness"] < 0.9f)
                {
                    //Door is almost entirely open
                    spriteName = "Door_openness_2";
                }
                else
                {
                    //Door is open
                    spriteName = "Door_openness_3";
                }
            }

            return installedObjectSprites[spriteName];
        }
        spriteName = _inObj.ObjectType + "_";

        //This region checks for neighbors and remanes the sprite accorrdingly
        #region NeighbourCheck
        //check for neighbours North, East, South, West
        int x = _inObj.Tile.X;
        int y = _inObj.Tile.Y;
        Tile t;

        //This code will see if it has neighbours next to it, and if so add a letter to the name
        //For example, a wall with wall above and below it will be first be named Wall_N then Wall_NS
        t = world.GetTileAt(x, y + 1);
        //if there is a tile above us, it has an object on it, and that object matches ours
        if(t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == _inObj.ObjectType)
        {
            spriteName += "N";
        }
        
        t = world.GetTileAt(x + 1, y);
        //if there is a tile to the right, it has an object on it, and that object matches ours
        if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == _inObj.ObjectType)
        {
            spriteName += "E";
        }

        t = world.GetTileAt(x, y - 1);
        //if there is a tile below us, it has an object on it, and that object matches ours
        if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == _inObj.ObjectType)
        {
            spriteName += "S";
        }

        t = world.GetTileAt(x - 1, y);
        //if there is a tile to the left, it has an object on it, and that object matches ours
        if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == _inObj.ObjectType)
        {
            spriteName += "W";
        }
        #endregion

        //return name that matches the sprite name
        if (installedObjectSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("GetSpriteForInstalledObject -- The Sprite for the " + spriteName + " object does not exist!");
            return null;
        }

        return installedObjectSprites[spriteName];
    }

    public Sprite GetSpriteForInstalledObject(string _objectType)
    {
        if(installedObjectSprites.ContainsKey(_objectType))
        {
            return installedObjectSprites[_objectType];
        }
        //Will look for something like the wall which has an added underscore
        if (installedObjectSprites.ContainsKey(_objectType + "_"))
        {
            return installedObjectSprites[_objectType + "_"];
        }
        Debug.LogError("GetSpriteForInstalledObject -- The Sprite for the" + _objectType + "object does not exist!");
        return null;
    }

}
