using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScript : MonoBehaviour
{
    public GameObject tilePrefab;
    public Color defaultColor;
    public Color highlightedColor;
    public int gridSizeX;
    public int gridSizeY;
    public float padding;

    private GameObject[,] grid;
    private float tileWidth, tileHeight;
    
    void Start()
    {
        grid = new GameObject[gridSizeX, gridSizeY];
        tileWidth = tilePrefab.transform.lossyScale.x;
        tileHeight = tilePrefab.transform.lossyScale.y;
        CenterGridParent();
        CreateGrid();
    }
    
    void Update()
    {
        
    }

    private void CenterGridParent()
    {
        float newX = (tileWidth + padding) * gridSizeX / -2;
        float newY = (tileHeight + padding) * gridSizeY / 2;
        transform.position = new Vector3(newX, newY, 0);
    }

    private void CreateGrid()
    {
        for (int i = 0; i < gridSizeX; i++)
        {
            string rowName = "Row " + i;
            GameObject currentRow = new GameObject(rowName);
            currentRow.transform.parent = gameObject.transform;
            for (int j = 0; j < gridSizeY; j++)
            {
                float nextX = i * (tileWidth + padding);
                float nextY = -(j * (tileHeight + padding));
                Vector3 nextPosition = transform.position;
                nextPosition += new Vector3(nextX, nextY, 0);
                GameObject thisTile = Instantiate(tilePrefab, nextPosition, Quaternion.identity);
                thisTile.name = j.ToString();
                thisTile.GetComponent<MeshRenderer>().material.SetColor("_Color", defaultColor);
                thisTile.transform.parent = currentRow.transform;
                grid[i, j] = thisTile;
            }
        }
    }
}
