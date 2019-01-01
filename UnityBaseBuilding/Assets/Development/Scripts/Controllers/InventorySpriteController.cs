using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySpriteController : MonoBehaviour{

    //Keep track of characters and their GameObjects
    Dictionary<Inventory, GameObject> inventoryGameObjectMap;

    Dictionary<string, Sprite> inventorySprites;

    World world { get { return WorldController.Instance.world; } }

    void Start(){

        LoadSpritesFromResources();

        inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();

        world.RegisterInventoryCreated(OnInventoryCreated);

        //check for pre-existing characters, which won't do the callback
        foreach(string objectType in world.inventoryManager.inventoryStacks.Keys)
        {
            foreach (Inventory inv in world.inventoryManager.inventoryStacks[objectType])
            {
                OnInventoryCreated(inv);
            }
        }
        
    }

    void LoadSpritesFromResources()
    {
        inventorySprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Art/");

        foreach (Sprite s in sprites)
        {
            inventorySprites[s.name] = s;
        }
    }

    public void OnInventoryCreated(Inventory _inv)
    {
        //create a visual GameObject linked to this data
        GameObject inv_GO = new GameObject();
        //add the Object/GameObject pair to dictionary
        inventoryGameObjectMap.Add(_inv, inv_GO);

        inv_GO.name = _inv.objectType;
        inv_GO.transform.position = new Vector3(_inv.tile.X, _inv.tile.Y, 0);
        inv_GO.transform.SetParent(transform, true);

        //add a sprite renderer
        SpriteRenderer sr = inv_GO.AddComponent<SpriteRenderer>();
        sr.sprite = inventorySprites[ _inv.objectType ];
        sr.sortingLayerName = "Inventory";

        //register callback so our GameObject gets updated
        //_inv.RegisterOnChangedCallback(OnInventoryChanged);
    }

    void OnInventoryChanged(Inventory _inv)
    {

        if (inventoryGameObjectMap.ContainsKey(_inv) == false)
        {
            Debug.LogError("OnInventoryChanged -- Trying to change a Inventory that is not in the map!");
            return;
        }

        GameObject inventory_GO = inventoryGameObjectMap[_inv];
       //inventory_GO.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(_character);

        inventory_GO.transform.position = new Vector3(_inv.tile.X, _inv.tile.Y, 0);
    }

}
