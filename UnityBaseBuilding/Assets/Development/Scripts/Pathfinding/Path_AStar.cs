using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_AStar {

    Queue<Tile> path;

    public Path_AStar(World _world, Tile _tileStart, Tile _tileEnd)
    {

        //if the world has no tileGraph, tell it to make one
        if(_world.tileGraph == null)
        {
            _world.tileGraph = new Path_TileGraph(_world);
        }

        //copy of the world's dictionary of all walkable nodes
        Dictionary<Tile, Path_Node<Tile>> nodes = _world.tileGraph.nodes;

        //make sure the start/end tiles are in the nodes list
        if (nodes.ContainsKey(_tileStart) == false)
        {
            Debug.LogError("Path_AStar -- Path_AStar: The starting tile is not in the list of nodes.");
            return;
        }
        if (nodes.ContainsKey(_tileStart) == false)
        {
            Debug.LogError("Path_AStar -- Path_AStar: The ending tile is not in the list of nodes.");
            return;
        }

        List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>>();

        List<Path_Node<Tile>> OpenSet = new List<Path_Node<Tile>>();

        OpenSet.Add( nodes[_tileStart] );

        Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

        Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float>();
        //set the assummed movement to get to the nodes to infinity, give the real value later
        foreach(Path_Node<Tile> n in nodes.Values)
        {
            g_score[n] = Mathf.Infinity;
        }

        g_score[nodes[_tileStart]] = 0;

        Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float>();
        foreach (Path_Node<Tile> n in nodes.Values)
        {
            f_score[n] = Mathf.Infinity;
        }
    }

    public Tile GetNextTile()
    {
        return path.Dequeue();
    }

}
