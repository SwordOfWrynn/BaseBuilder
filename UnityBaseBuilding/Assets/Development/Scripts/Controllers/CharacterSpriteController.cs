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

        characterGameObjectMap = new Dictionary<Character, GameObject>();

        world.RegisterCharacterCreated(OnCharacterCreated);

        Character c = world.CreateCharacter(world.GetTileAt(world.Width/2, world.Height/2));
        //c.SetDestination(world.GetTileAt(world.Width / 2 + 5, world.Height / 2));
    }

    void LoadSpritesFromResources()
    {
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Art/Characters/");

        foreach (Sprite s in sprites)
        {
            characterSprites[s.name] = s;
        }
    }

    public void OnCharacterCreated(Character _character)
    {
        //create a visual GameObject linked to this data
        GameObject character_GO = new GameObject();
        //add the Object/GameObject pair to dictionary
        characterGameObjectMap.Add(_character, character_GO);

        character_GO.name = "Character";
        character_GO.transform.position = new Vector3(_character.X, _character.Y, 0);
        character_GO.transform.SetParent(transform, true);

        //add a sprite renderer
        SpriteRenderer sr = character_GO.AddComponent<SpriteRenderer>();
        sr.sprite = characterSprites["p1_front"];
        sr.sortingLayerName = "Characters";

        //register callback so our GameObject gets updated
        _character.RegisterOnChangedCallback(OnCharacterChanged);
    }

    void OnCharacterChanged(Character _character)
    {

        if (characterGameObjectMap.ContainsKey(_character) == false)
        {
            Debug.LogError("OnCharacterChanged -- Trying to change a Character that is not in the map!");
            return;
        }

        GameObject character_GO = characterGameObjectMap[_character];
       //character_GO.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(_character);

        character_GO.transform.position = new Vector3(_character.X, _character.Y, 0);
    }

}
