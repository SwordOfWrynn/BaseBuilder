using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InstalledObjectActions {

	public static void Door_UpdateAction(InstalledObject _inObj, float _deltaTime)
    {
        //if (Debug.isDebugBuild)
        //    Debug.Log("Door_UpdateAction: " + _inObj.inObjParameters["openness"]);

        if (_inObj.inObjParameters["is_opening"] >= 1){
            _inObj.inObjParameters["openness"] += _deltaTime * 4;
            if(_inObj.inObjParameters["openness"] >= 1)
            {
                _inObj.inObjParameters["is_opening"] = 0;
            }
        }
        else
        {
            _inObj.inObjParameters["openness"] -= _deltaTime * 4;
        }

        _inObj.inObjParameters["openness"] = Mathf.Clamp01(_inObj.inObjParameters["openness"]);

        if(_inObj.cbOnChanged != null)
            _inObj.cbOnChanged(_inObj);
    }

    public static ENTERABILITY Door_IsEnterable(InstalledObject _inObj)
    {
        //if(Debug.isDebugBuild)
        //    Debug.Log("Door_IsEnterable");

        _inObj.inObjParameters["is_opening"] = 1;

        if(_inObj.inObjParameters["openness"] >= 1)
        {
            return ENTERABILITY.Yes;
        }
        return ENTERABILITY.Soon;
    }

}
