using UnityEngine;
using System.Collections;

public class AutomaticVerticalSize : MonoBehaviour
{

    public float childHeight = 35f;

    // Use this for initialization
    void Start()
    {
        AdjustSize();
    }

    public void AdjustSize()
    {
        Vector2 transformSize = GetComponent<RectTransform>().sizeDelta;
        transformSize.y = transform.childCount * childHeight;
        GetComponent<RectTransform>().sizeDelta = transformSize;
    }
}
