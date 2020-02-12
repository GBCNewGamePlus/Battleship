using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputScript : MonoBehaviour
{
    public GridScript grid;

    void Update()
    {
        MouseInput();
    }

    void MouseInput()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            if (hit.transform.gameObject.tag == "Tile")
            {
                hit.transform.gameObject.GetComponent<TileScript>().Highlight();
            }
        }
    }
}
