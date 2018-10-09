using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_TileGraph{

    //this class constructs a simple pathfinding compatable graph of the world
    //Each tile is a node. each walkable neighbour of a tile is linked with an edge

    Dictionary<Tile, Path_Node<Tile>> nodes;
    
    
	public Path_TileGraph(World _world)
    {
        //loop through all world walkable floor tiles, and create a Node for each.

        nodes = new Dictionary<Tile, Path_Node<Tile>>;

        for (int x = 0; x < _world.Width; x++)
        {
            for (int y = 0; y < _world.Height; y++)
            {
                Tile t = _world.GetTileAt(x, y);

                if (t.MovementCost > 0) //Tiles with cost of 0 are non-walkable
                {
                    Path_Node<Tile> n = new Path_Node<Tile>;
                    n.data = t;
                    nodes.Add(t, n);
                }
            }
        }

        //Now loop through all nodes and create edges

        foreach(Tile n in nodes.Keys)
        {
            //Get list of nieghbours for tiles, and create an edge if the nieghbour is walkable


        }

    }

}
