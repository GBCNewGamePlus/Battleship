using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipRotation {Vertical, Horizontal};
public enum GridType {Defense, Attack};

public class GridScript : MonoBehaviour
{
    public GridType gridType;
    public GameObject tilePrefab;
    public GameObject shipPrefab_3;
    public Color backgroundColor;
    public Color defaultColor;
    public Color highlightedColor;
    public Color occupiedColor;
    public int gridSizeX;
    public int gridSizeY;
    public float padding;

    private GameObject[,] grid;
    private float tileWidth, tileHeight;
    private ShipRotation currentRotation;
    private int totalShipCount = -1; // -1 means n/a
    private int shipCount = -1; // -1 means n/a
    private bool setupComplete;
    
    void Awake()
    {
        grid = new GameObject[gridSizeX, gridSizeY];
        tileWidth = tilePrefab.transform.lossyScale.x;
        tileHeight = tilePrefab.transform.lossyScale.y;
        currentRotation = ShipRotation.Horizontal;
        if (gridType == GridType.Defense)
        {
            totalShipCount = 3;
            shipCount = 0;
        }
        setupComplete = false;
        //CenterGridParent();
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
                thisTile.GetComponent<TileScript>().SetLocation(new Vector2Int(i, j));
                grid[i, j] = thisTile;
            }
        }

        Vector3 firstTilePos = grid[0, 0].transform.localPosition;
        Vector3 centeringVector = new Vector3(gridSizeX * (tileWidth + padding)/2, -gridSizeY * (tileHeight + padding)/2, 0.5f);
        Vector3 finalTouches = new Vector3(-tileWidth/2 - padding/2, tileHeight/2 + padding/2, 0);
        Vector3 backgroundPos = firstTilePos + centeringVector + finalTouches;
        Vector3 backgroundScale = new Vector3(gridSizeX * (tileWidth + padding) + padding/2, gridSizeY * (tileHeight + padding) + padding / 2, 1);
        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
        background.name = "Background";
        background.transform.parent = gameObject.transform;
        background.transform.position = backgroundPos;
        background.transform.localScale = backgroundScale;
        background.GetComponent<MeshRenderer>().material.SetColor("_Color", backgroundColor);
    }

    public void ToggleShipRotation()
    {
        if (currentRotation == ShipRotation.Horizontal)
            currentRotation = ShipRotation.Vertical;
        else
            currentRotation = ShipRotation.Horizontal;
    }

    public void HighlightArea(Vector2Int index)
    {
        if (gridType == GridType.Defense && !setupComplete)
        {
            if (currentRotation == ShipRotation.Horizontal)
            {
                index.x = Mathf.Clamp(index.x, 1, gridSizeX - 2);
                if (TileAvailable(index))
                {
                    grid[index.x, index.y].GetComponent<TileScript>().MouseOver();
                    grid[index.x - 1, index.y].GetComponent<TileScript>().MouseOver();
                    grid[index.x + 1, index.y].GetComponent<TileScript>().MouseOver();
                }
            }
            else
            {
                index.y = Mathf.Clamp(index.y, 1, gridSizeY - 2);
                if (TileAvailable(index))
                {
                    grid[index.x, index.y].GetComponent<TileScript>().MouseOver();
                    grid[index.x, index.y + 1].GetComponent<TileScript>().MouseOver();
                    grid[index.x, index.y - 1].GetComponent<TileScript>().MouseOver();
                }
            }
        }
    }

    public void SelectArea(Vector2Int index)
    {
        if (gridType == GridType.Defense && !setupComplete)
        {
            if (currentRotation == ShipRotation.Horizontal)
            {
                index.x = Mathf.Clamp(index.x, 1, gridSizeX - 2);
                if (TileAvailable(index))
                {
                    grid[index.x, index.y].GetComponent<TileScript>().SetColor(occupiedColor, true);
                    grid[index.x - 1, index.y].GetComponent<TileScript>().SetColor(occupiedColor, true);
                    grid[index.x + 1, index.y].GetComponent<TileScript>().SetColor(occupiedColor, true);
                    InstantiateSprite(index);
                    shipCount++;
                }
            }
            else
            {
                index.y = Mathf.Clamp(index.y, 1, gridSizeY - 2);
                if (TileAvailable(index))
                {
                    grid[index.x, index.y].GetComponent<TileScript>().SetColor(occupiedColor, true);
                    grid[index.x, index.y + 1].GetComponent<TileScript>().SetColor(occupiedColor, true);
                    grid[index.x, index.y - 1].GetComponent<TileScript>().SetColor(occupiedColor, true);
                    InstantiateSprite(index);
                    shipCount++;
                }
            }
            if (shipCount >= totalShipCount)
            {
                setupComplete = true;
            }
        }
    }

    public bool SetupComplete()
    {
        return setupComplete;
    }

    private bool TileAvailable(Vector2Int index)
    {
        List<bool> conditions = new List<bool>();
        if (currentRotation == ShipRotation.Horizontal)
        {
            conditions.Add(grid[index.x, index.y].GetComponent<TileScript>().GetOccupied());
            conditions.Add(grid[index.x - 1, index.y].GetComponent<TileScript>().GetOccupied());
            conditions.Add(grid[index.x + 1, index.y].GetComponent<TileScript>().GetOccupied());
        }
        else
        {
            conditions.Add(grid[index.x, index.y].GetComponent<TileScript>().GetOccupied());
            conditions.Add(grid[index.x, index.y - 1].GetComponent<TileScript>().GetOccupied());
            conditions.Add(grid[index.x, index.y + 1].GetComponent<TileScript>().GetOccupied());
        }
        foreach (bool c in conditions)
        {
            if (c)
                return false;
        }
        return true;
    }

    private void InstantiateSprite(Vector2Int index)
    {
        Vector3 spritePos = grid[index.x, index.y].transform.position;
        spritePos += new Vector3(0, 0, -1);
        Quaternion spriteRot = Quaternion.identity;
        if (currentRotation == ShipRotation.Horizontal)
        {
            spriteRot.eulerAngles = new Vector3(0, 0, 90);
        }
        GameObject thisShip = Instantiate(shipPrefab_3, spritePos, spriteRot);
        thisShip.name = "Ship_3";
        thisShip.transform.parent = gameObject.transform;
    }
}
