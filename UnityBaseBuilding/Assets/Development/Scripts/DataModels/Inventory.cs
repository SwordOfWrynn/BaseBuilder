﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Inventory are things sitting on the floor/in a pile, like a bunch of metal bars
//or a non-installed version of an InstalledObject (e.g. a door still sitting around to be installed)
public class Inventory {

    public string objectType = "Steel Plate";

    public int maxStackSize = 50;
    public int stackSize;

}
