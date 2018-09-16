using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//InstalledObjects are things like walls, doors. furniture, etc.
public class InstalledObject {

    //this represents BASE tile of the object, large objects may occupy multipule tiles
    Tile tile; 


    string objectType;

    //this is a multiplier, so a value of 2 means you move twice as slowly (half speed)
    //Tile types and other environmental effects may be combined
    //e.g. moving through a tile with a cost of 2 with a InstalledObject with a cost of 3, that is on fire with has 3 cost
    //would have total cost of 8, so you'd move at 1/8 speed
    //SPECIAL: if movementCost = 0, then tile cannot be moved through
    float movementCost = 1f;

    int width = 0;
    int height = 0;

}
