using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputScript : MonoBehaviour
{
    public GridScript grid;

    void Update()
    {
        MouseOver();
        MouseClick();
        ToggleShipRotation();
    }

    void MouseOver()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            if (hit.transform.gameObject.tag == "Tile")
            {
                TileScript thisTile = hit.transform.gameObject.GetComponent<TileScript>();
                grid.HighlightArea(thisTile.GetLocation());
            }
        }
    }

    void MouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                if (hit.transform.gameObject.tag == "Tile")
                {
                    TileScript thisTile = hit.transform.gameObject.GetComponent<TileScript>();
                    grid.SelectArea(thisTile.GetLocation());
                }
            }
        }
    }

    void ToggleShipRotation()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            grid.ToggleShipRotation();
        }
    }
}
