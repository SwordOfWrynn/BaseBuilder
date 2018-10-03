using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteController : MonoBehaviour{

    //Keep track of characters and their GameObjects
    Dictionary<Character, GameObject> characterGameObjectMap;

    Dictionary<string, Sprite> characterSprites;

    World world { get { return WorldController.Instance.world; } }

    void Start(){

        LoadSpritesFromResources();

        characterGameObjectMap = new Dictionary<InstalledObject, GameObject>();

        world.RegisterInstalledObjectCreated(OnInstalledObjectCreated);

    }

    void LoadSpritesFromResources()
    {
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Art/InstalledObjects/");

        foreach (Sprite s in sprites)
        {
            characterSprites[s.name] = s;
        }
    }

    public void OnInstalledObjectCreated(InstalledObject _obj)
    {
        //create a visual GameObject linked to this data
        GameObject obj_GO = new GameObject();
        //add the Object/GameObject pair to dictionary
        characterGameObjectMap.Add(_obj, obj_GO);

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

        if (characterGameObjectMap.ContainsKey(_obj) == false)
        {
            Debug.LogError("OnInstalledObjectChanged -- Trying to change a InstalledObject that is not in the map");
            return;
        }

        GameObject obj_GO = characterGameObjectMap[_obj];
        obj_GO.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(_obj);
    }

}
