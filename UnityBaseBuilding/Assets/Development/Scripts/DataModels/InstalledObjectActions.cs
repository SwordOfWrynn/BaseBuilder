using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InstalledObjectActions {

	public static void Door_UpdateAction(InstalledObject _inObj, float _deltaTime)
    {

    }

    public static ENTERABILITY Door_IsEnterable(InstalledObject _inObj)
    {
        if(_inObj.inObjParameters["openess"] >= 1)
        {
            return ENTERABILITY.Yes;
        }
        return ENTERABILITY.Soon;
    }

}
