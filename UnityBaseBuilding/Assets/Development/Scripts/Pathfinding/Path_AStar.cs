using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;

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

        Path_Node<Tile> start = nodes[_tileStart];
        Path_Node<Tile> goal = nodes[_tileEnd];

        List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>>();

        /*List<Path_Node<Tile>> OpenSet = new List<Path_Node<Tile>>();
        OpenSet.Add( start );*/

        //A queue where everything has a priority value. The lower the number the higher the priority. Higher priority items are first on the list
        SimplePriorityQueue<Path_Node<Tile>> OpenSet = new SimplePriorityQueue<Path_Node<Tile>>();
        OpenSet.Enqueue(start, 0);

        Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

        Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float>();
        //set the assummed movement to get to the nodes to infinity, give the real value later
        foreach(Path_Node<Tile> n in nodes.Values)
        {
            g_score[n] = Mathf.Infinity;
        }

        g_score[start] = 0;

        Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float>();
        foreach (Path_Node<Tile> n in nodes.Values)
        {
            f_score[n] = Mathf.Infinity;
        }
        f_score[start] = heuristic_cost_estimate(start, goal);

        while(OpenSet.Count > 0)
        {
            Path_Node<Tile> current = OpenSet.Dequeue();

            if(current == goal)
            {
                //we have reached our goal, we'll convert this into a sequence of tiles to walk and end the function
                ReconstructPath(Came_From, current);
                return;
            }

            ClosedSet.Add(current);

            foreach(Path_Edge<Tile> edgeNeighbor in current.edges)
            {
                Path_Node<Tile> neighbor = edgeNeighbor.node;
                if (ClosedSet.Contains(neighbor))
                    continue;

                float tentative_g_score = g_score[current] + DistBetween(current, neighbor);

                if (OpenSet.Contains(neighbor) && tentative_g_score >= g_score[neighbor])
                    continue;

                Came_From[neighbor] = current;
                g_score[neighbor] = tentative_g_score;
                f_score[neighbor] = g_score[neighbor] + heuristic_cost_estimate(neighbor, goal);

                if (OpenSet.Contains(neighbor) == false)
                    OpenSet.Enqueue(neighbor, f_score[neighbor]);
            }

        }
        //If we reached here we have gone through the entire OpenSet without reaching a point where current == goal
        //This happens if there is no path from start to goal, e.g. a wall, or a missing floor
    }

    float heuristic_cost_estimate(Path_Node<Tile> _a, Path_Node<Tile> _b)
    {
        return Mathf.Sqrt(Mathf.Pow(_a.data.X - _b.data.X, 2) + Mathf.Pow(_a.data.Y - _b.data.Y, 2));
    }

    float DistBetween(Path_Node<Tile> _a, Path_Node<Tile> _b)
    {
        //we can make assumptions because we know we are working on a grid
        //Hori/Vert neighbors have distance of 1
        if (Mathf.Abs(_a.data.X - _b.data.X) + Mathf.Abs(_a.data.Y - _b.data.Y) == 1)
            return 1;

        //diag neighbors have distance of 1.41421356237
        if (Mathf.Abs(_a.data.X - _b.data.X) == 1 && Mathf.Abs(_a.data.Y - _b.data.Y) == 1)
            return 1.41421356237f; //sqrt 2
        
        //Otherwise do the actual math

        return Mathf.Sqrt(Mathf.Pow(_a.data.X - _b.data.X, 2) + Mathf.Pow(_a.data.Y - _b.data.Y, 2));

    }

    void ReconstructPath(Dictionary<Path_Node<Tile>, Path_Node<Tile>> _Came_From, Path_Node<Tile> _current)
    {
        //At this point current is the goal, so walk backward through the came from map until we reach the end, which is the starting node
        Queue<Tile> total_path = new Queue<Tile>();
        total_path.Enqueue(_current.data); //this final step in the path is the goal

        while (_Came_From.ContainsKey(_current))
        {
            //came from is a map where the key => value relation is saying some node => we got there from this node
            _current = _Came_From[_current];
            total_path.Enqueue(_current.data);
        }
        //At this point total_path is a queue running from the end tile to the start tile, we need to reverse it

        path = new Queue<Tile>(total_path.Reverse());
    }

    public Tile Dequeue()
    {
        return path.Dequeue();
    }

    public int Length()
    {
        if (path == null)
            return 0;
        return path.Count;
    }

}
