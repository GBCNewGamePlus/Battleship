using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Update()
    {
        float width = transform.lossyScale.x;
        float height = transform.lossyScale.y;
        Debug.Log(width + "," + height);
    }
}
