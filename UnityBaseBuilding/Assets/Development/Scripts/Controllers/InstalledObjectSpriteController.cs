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

    public Sprite GetSpriteForInstalledObject(InstalledObject _obj)
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
        t = world.GetTileAt(x, y + 1);
        //if there is a tile above us, it has an object on it, and that object matches ours
        if(t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == _obj.ObjectType)
        {
            spriteName += "N";
        }
        
        t = world.GetTileAt(x + 1, y);
        //if there is a tile to the right, it has an object on it, and that object matches ours
        if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == _obj.ObjectType)
        {
            spriteName += "E";
        }

        t = world.GetTileAt(x, y - 1);
        //if there is a tile below us, it has an object on it, and that object matches ours
        if (t != null && t.InstalledObject != null && t.InstalledObject.ObjectType == _obj.ObjectType)
        {
            spriteName += "S";
        }

        t = world.GetTileAt(x - 1, y);
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
