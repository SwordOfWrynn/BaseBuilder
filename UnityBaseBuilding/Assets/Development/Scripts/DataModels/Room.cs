using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {

    public float atmosO2 = 0;
    public float atmosN = 0;
    public float atmosCO2 = 0;

    List<Tile> tiles;

    public Room()
    {
        tiles = new List<Tile>();
    }

    public void AssignTile(Tile _t)
    {
        if (tiles.Contains(_t))
            return;

        _t.room = this;
        tiles.Add(_t);
    }

    public void UnAssignAllTiles()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].room = tiles[i].World.GetOutsideRoom();
        }
        tiles = new List<Tile>();
    }

    public static void DoRoomFloodFill(InstalledObject _sourceInObj)
    {
        //_sourceInObj is the oject that may split 2 rooms or fully enclose on (e.g. a wall)
        //check NESW neighbour of the Installed Object's tile and do flood fills from them
        World world = _sourceInObj.Tile.World;

        Room oldRoom = _sourceInObj.Tile.room;

        //Try to build a new room starting from the north
        ActualFloodFill(_sourceInObj.Tile.North(), oldRoom);
        ActualFloodFill(_sourceInObj.Tile.East(), oldRoom);
        ActualFloodFill(_sourceInObj.Tile.South(), oldRoom);
        ActualFloodFill(_sourceInObj.Tile.West(), oldRoom);

        //If the Object was installed in a room (which should always be true if we consider outside a big room), delete that room and assign all tiles to outside,
        //then we will fill and create a new room (or rooms) to put the tiles back in
        oldRoom.tiles = new List<Tile>(); // All tiles should now point to a different room so we can force the old rooms tile list to be blank

        if (_sourceInObj.Tile.room != world.GetOutsideRoom())
        {
            world.DeleteRoom(oldRoom); //Will reassign tiles to the outside room
        }
    }

    protected static void ActualFloodFill(Tile _tile, Room _oldRoom)
    {
        if(_tile == null)
        {
            //We are trying to flood fill the edge of the map, so just return
            return;
        }
        if(_tile.room != _oldRoom)
        {
            //This tile was already assigned to a new room, which means the direction picked isn't isolated. So we can just return without makeing a new room.
            return;
        }
        if (_tile.InstalledObject.roomEnclosure)
        {
            //The tile has something like a wall or something, so we can make a room from there
            return;
        }

        Room newRoom = new Room();
    }

}
