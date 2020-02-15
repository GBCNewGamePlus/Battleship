using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    private Color defaultColor, highlightedColor, occupiedColor;
    private Vector2Int location;
    private bool isOccupied;
    private GridScript grid;

    private void Start()
    {
        grid = transform.parent.parent.GetComponent<GridScript>();
        defaultColor = grid.defaultColor;
        highlightedColor = grid.highlightedColor;
        occupiedColor = grid.occupiedColor;
        isOccupied = false;
    }

    public void SetLocation(Vector2Int loc)
    {
        location = loc;
    }

    public Vector2Int GetLocation()
    {
        return location;
    }

    public void MouseOver()
    {
        if (!isOccupied)
        {
            SetColor(highlightedColor, false);
            CancelInvoke("ResetHighlight");
            Invoke("ResetHighlight", 0.05f);
        }
    }

    private void ResetHighlight()
    {
        if (!isOccupied)
            SetColor(defaultColor, false);
    }

    public void SetColor(Color c, bool o)
    {
        isOccupied = o;
        GetComponent<MeshRenderer>().material.SetColor("_Color", c);
    }

    public Color GetColor()
    {
        return GetComponent<MeshRenderer>().material.GetColor("_Color");
    }

    public bool GetOccupied()
    {
        return isOccupied;
    }

    public GridScript GetGrid()
    {
        return grid;
    }
}
