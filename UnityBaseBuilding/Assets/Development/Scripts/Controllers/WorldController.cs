﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;

public class WorldController : MonoBehaviour {
    
    public static WorldController Instance { get; protected set; }

    public World world { get; protected set; }

    static bool loadWorld = false;

    void Awake() {
        if(Instance != null)
        {
            Debug.LogError("There is more than one WorldController!");
        }
        Instance = this;

        if (loadWorld)
        {
            CreateWorldFromSaveFile();
            loadWorld = false;
        }
        else
        {
            CreateEmptyWorld();
        }
    }

    void Update()
    {
        world.Update(Time.deltaTime);
    }

    //returns the tile at the given coordinates
    public Tile GetTileAtWorldCoord(Vector3 _coord)
    {
        int x = Mathf.FloorToInt(_coord.x + 0.5f); //The mouse is half a tile off without this -0.5
        int y = Mathf.FloorToInt(_coord.y + 0.5f);

        return world.GetTileAt(x, y);
    }

    public void NewWorld()
    {
        Debug.Log("NewWorld button clicked");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void SaveWorld()
    {
        Debug.Log("SaveWorld button clicked");

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, world);
        writer.Close();

        Debug.Log(writer.ToString());

        PlayerPrefs.SetString("SaveGame00", writer.ToString());
    }
    public void LoadWorld()
    {
        Debug.Log("LoadWorld button clicked");

        loadWorld = true;
        //Reload scene to reset all data (and purge old references)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CreateEmptyWorld()
    {
        //create a world with empty tiles
        world = new World(100, 100);

        //Center the camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, -10);
    }

    void CreateWorldFromSaveFile()
    {
        Debug.Log("CreateWorldFromSaveFile");
        //create a world with from save file data

        //        PlayerPrefs.SetString("SaveGame00", writer.ToString());

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));
        Debug.Log(reader.ToString());
        world = (World)serializer.Deserialize(reader);
        reader.Close();


        //Center the camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, -10);
    }

}
