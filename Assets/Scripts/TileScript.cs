using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public GridScript grid;
    private Color defaultColor, highlightedColor;

    private void Start()
    {
        defaultColor = grid.defaultColor;
        highlightedColor = grid.highlightedColor;
    }

    public void Highlight()
    {
        SetColor(highlightedColor);
        CancelInvoke("ResetHighlight");
        Invoke("ResetHighlight", 0.05f);
    }

    private void ResetHighlight()
    {
        SetColor(defaultColor);
    }

    private void SetColor(Color c)
    {
        GetComponent<MeshRenderer>().material.SetColor("_Color", c);
    }
}
