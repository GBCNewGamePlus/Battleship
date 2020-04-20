using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputScript : MonoBehaviour
{
    void Update()
    {
        if(UIManager.State == 4){
            MouseOver();
            MouseClick();
        }
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
                thisTile.GetGrid().HighlightArea(thisTile.GetLocation());
                ToggleShipRotation(thisTile.GetGrid());
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
                    thisTile.GetGrid().SelectArea(thisTile.GetLocation());
                }
            }
        }
    }

    void ToggleShipRotation(GridScript g)
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            g.ToggleShipRotation();
        }
    }
}
