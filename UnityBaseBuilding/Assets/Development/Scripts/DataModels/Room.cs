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
        if (tiles.Contains(_t)) //if the tile is already in the room
            return;

        if(_t.room != null) //if it belongs to some other room, remove it from that room
        {
            _t.room.tiles.Remove(_t);
        }

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

        //Try to build new rooms starting for each direction
        foreach(Tile t in _sourceInObj.Tile.GetNeighbours())
        {
            ActualFloodFill(t, oldRoom);
        }

        //the source tile has a wall or something, which is not part of the room
        _sourceInObj.Tile.room = null;
        oldRoom.tiles.Remove(_sourceInObj.Tile);

        //If the Object was installed in a room (which should always be true if we consider outside a big room), delete that room and assign all tiles to outside,
        //then we will fill and create a new room (or rooms) to put the tiles back in

        if (/*_sourceInObj.Tile.room*/oldRoom != world.GetOutsideRoom())
        {
            //at this point oldRoom should no longer have tiles left in it, so in practice this Deletion should only need to remove the room form the world's list
            
            if(oldRoom.tiles.Count > 0)
            {
                Debug.LogError("Room -- DoRoomFloodFill: 'oldRoom' still has tiles assinged to it. This is wrong!");
            }

            world.DeleteRoom(oldRoom);
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
        if (_tile.InstalledObject != null && _tile.InstalledObject.roomEnclosure)
        {
            //The tile has something like a wall, so we cant make a room from there
            return;
        }

        if (_tile.Type == TileType.Empty)
        {
            //the tile is empty space and must remain part of the outside
            return;
        }


        Room newRoom = new Room();
        Queue<Tile> tilesToCheck = new Queue<Tile>();
        //The current tile belongs to the new room

        tilesToCheck.Enqueue(_tile);
        
        while(tilesToCheck.Count > 0)
        {
            Tile t = tilesToCheck.Dequeue();

            if (t.room == _oldRoom)
            {
                newRoom.AssignTile(t);

                Tile[] neighbours = t.GetNeighbours();

                foreach (Tile t2 in neighbours)
                {
                    if(t2 == null || t2.Type == TileType.Empty)
                    {
                        //We have hit open space, either at edge of map or being an empty tile, so the room we're building is part of the outside so 
                        //end the flood fill and delete the new room and reassign all tiles to the outside
                        newRoom.UnAssignAllTiles();
                        return;
                    }
                    //we know t2 is not null or empty, so make sure it hasn't already be prosssed an isn't a wall type tile
                    if (t2.room == _oldRoom && (t2.InstalledObject == null || t2.InstalledObject.roomEnclosure == false))
                        tilesToCheck.Enqueue(t2);
                }

            }
        }
        //Tell world new room has benn formed
        _tile.World.AddRoom(newRoom);

    }

}
