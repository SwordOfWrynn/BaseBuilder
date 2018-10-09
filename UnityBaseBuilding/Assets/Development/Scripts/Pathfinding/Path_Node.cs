using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Node<T> {
    //Path_Node<Tile> -> T becomes a Tile

    public T data;

    public Path_Edge<T>[] edges; //nodes leading out, not in

}
