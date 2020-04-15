using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileScript : MonoBehaviour
{
    public TextMesh attackFeedbackText;

    private Color defaultColor, highlightedColor, occupiedColor;
    private Vector2Int location;
    private bool isOccupied;
    private bool hasShip;
    private bool isAttacked;
    private GridScript grid;

    private void Start()
    {
        grid = transform.parent.parent.GetComponent<GridScript>();
        defaultColor = grid.defaultColor;
        highlightedColor = grid.highlightedColor;
        occupiedColor = grid.occupiedColor;
        isOccupied = false;
        hasShip = false;
        isAttacked = false;
        attackFeedbackText.text = "";
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

    public void SetHasShip(bool b)
    {
        hasShip = b;
    }

    public bool HasShip()
    {
        return hasShip;
    }

    public void SetIsAttacked(bool b)
    {
        isAttacked = b;
    }

    public void ShowText(string msg)
    {
        attackFeedbackText.text = msg;
    }

    public bool IsAttacked()
    {
        return isAttacked;
    }
}
