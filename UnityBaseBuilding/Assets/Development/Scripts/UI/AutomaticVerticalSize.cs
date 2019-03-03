using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AutomaticVerticalSize : MonoBehaviour
{

    public float childHeight = 35f;

    // Use this for initialization
    void Start()
    {
        AdjustSize();
    }

    private void Update()
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
