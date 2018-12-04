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

        //If the Object was installed in a room (which should always be true if we consider outside a big room), delete that room and assign all tiles to outside,
        //then we will fill and create a new room (or rooms) to put the tiles back in

        if (_sourceInObj.Tile.room != world.GetOutsideRoom())
            world.DeleteRoom(_sourceInObj.Tile.room);

    }

}
