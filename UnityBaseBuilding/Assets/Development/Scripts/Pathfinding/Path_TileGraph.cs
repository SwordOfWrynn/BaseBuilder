using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_TileGraph{

    //this class constructs a simple pathfinding compatable graph of the world
    //Each tile is a node. each walkable neighbour of a tile is linked with an edge

    public Dictionary<Tile, Path_Node<Tile>> nodes;
    
    
	public Path_TileGraph(World _world)
    {
        //loop through all world walkable floor tiles, and create a Node for each.

        Debug.Log("Path_TileGraph");

        nodes = new Dictionary<Tile, Path_Node<Tile>>();

        for (int x = 0; x < _world.Width; x++)
        {
            for (int y = 0; y < _world.Height; y++)
            {
                Tile t = _world.GetTileAt(x, y);

                //if (t.MovementCost > 0) //Tiles with cost of 0 are non-walkable
                //{
                    Path_Node<Tile> n = new Path_Node<Tile>();
                    n.data = t;
                    nodes.Add(t, n);
                //}
            }
        }
        Debug.Log("Path_TileGraph: created " + nodes.Count + " nodes");

        int edgeCount = 0;

        //Now loop through all nodes and create edges

        foreach (Tile t in nodes.Keys)
        {
            Path_Node<Tile> n = nodes[t];

            List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();

            //Get list of nieghbours for tiles
            Tile[] neighbours = t.GetNeighbours(true); //Some array spots may be null

            //create an edge if the nieghbour is walkable
            for (int i = 0; i < neighbours.Length; i++)
            {
                //See if neighbour exists and is walkable
                if (neighbours[i] != null && neighbours[i].MovementCost != 0)
                {
                    //make sure we won't clip through wall corners, or squeeze through 2 diagonal walls
                    if (IsClippingCorner(t, neighbours[i]))
                    {
                        continue; //skip to next neighbor with out building edge
                    }

                    //create an edge
                    Path_Edge<Tile> e = new Path_Edge<Tile>();
                    e.cost = neighbours[i].MovementCost;
                    e.node = nodes[neighbours[i]];

                    //add edge to temporary list
                    edges.Add(e);
                    edgeCount++;
                }
            }

            n.edges = edges.ToArray();

        }
        Debug.Log("Path_TileGraph: created " + edgeCount + " edges");

    }

    bool IsClippingCorner( Tile _currentTile, Tile _neighborTile)
    {
        //if movement from current tile to the neighbor tile is diagonal, tehan chack to make sure we wont be clipping

        //see if we are diagonal
        if(Mathf.Abs(_currentTile.X - _neighborTile.X) + Mathf.Abs(_currentTile.Y - _neighborTile.Y) == 2)
        {
            int dX = _currentTile.X - _neighborTile.X;
            int dY = _currentTile.Y - _neighborTile.Y;

            //if east or west is unwalkable, meaning this would be clipped movement
            if (_currentTile.World.GetTileAt(_currentTile.X - dX, _currentTile.Y).MovementCost == 0)
                return true;

            //if north or south is unwalkable, meaning this would be clipped movement
            if (_currentTile.World.GetTileAt(_currentTile.X, _currentTile.Y - dY).MovementCost == 0)
                return true;
        }

        return false;

    }

}
