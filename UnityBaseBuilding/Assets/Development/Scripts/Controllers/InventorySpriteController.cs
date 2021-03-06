﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySpriteController : MonoBehaviour{

    public GameObject inventoryUIPrefab;

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

        if(_inv.maxStackSize > 1) //this object is stackable, so add a UI component to show the stack size
        {
            GameObject ui_GO = Instantiate(inventoryUIPrefab);
            ui_GO.transform.SetParent(inv_GO.transform);
            ui_GO.transform.localPosition = Vector3.zero;
            ui_GO.GetComponentInChildren<Text>().text = _inv.StackSize.ToString();
        }

        //register callback so our GameObject gets updated
        _inv.RegisterInventoryChangedCallback(OnInventoryChanged);
    }

    void OnInventoryChanged(Inventory _inv)
    {


        if (inventoryGameObjectMap.ContainsKey(_inv) == false)
        {
            Debug.LogError("OnInventoryChanged -- Trying to change a Inventory that is not in the map!");
            return;
        }

        GameObject inventory_GO = inventoryGameObjectMap[_inv];

        if (_inv.StackSize > 0)
        {
            inventory_GO.transform.position = new Vector3(_inv.tile.X, _inv.tile.Y, 0);

            Text t = inventory_GO.GetComponentInChildren<Text>();
            if (t != null)
                t.text = _inv.StackSize.ToString();
        }
        else
        {
            //the stack is equal to 0, so remove the sprite
            inventoryGameObjectMap.Remove(_inv);
            _inv.UnRegisterInventoryChangedCallback(OnInventoryChanged);
            Destroy(inventory_GO);
        }
    }

}
