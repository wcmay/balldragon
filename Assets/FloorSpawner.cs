using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorSpawner : MonoBehaviour
{
    public List<Transform> objects;
    public GameObject tilePrefab;

    public Vector2Int gridSize;
    public float renderDistance;
    public float groundLevel;
    public float heightToDrawBelow;
    public int gap;

    Vector3 tilePosition;

    int xRadius;
    int zRadius;

    bool drawTiles;
    
    // Start is called before the first frame update
    void Start()
    {
        tilePosition = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        xRadius = gridSize.x / 2;
        zRadius = gridSize.y / 2;

        drawTiles = false;

        foreach(Transform point in objects)
        {
            if(point.position.y < (groundLevel + heightToDrawBelow))
            {
                drawTiles = true;
            }
        }

        if (drawTiles)
        {
            DestroyChildren();
            DrawTiles(tilePrefab);
        } else
        {
            DestroyChildren();
        }
        
    }

    void DrawTiles(GameObject tilePrefab) 
    {
        foreach (Transform point in objects) 
        {
            tilePosition.x = Mathf.Floor(point.transform.position.x / gap) * gap + (xRadius * gap);
            tilePosition.y = groundLevel;
            tilePosition.z = Mathf.Floor(point.transform.position.z / gap) * gap + (zRadius * gap);

            for (int x = 0; x < gridSize.x; x++)
            {
                tilePosition.z = Mathf.Floor(point.transform.position.z / gap) * gap + (zRadius * gap);
                for (int z = 0; z < gridSize.y; z++)
                {
                    float distance = Vector3.Distance(tilePosition, point.transform.position);
                    if (distance < renderDistance)
                    {
                        GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                        tile.transform.parent = this.transform;
                    }
                    tilePosition.z -= gap;
                }
                tilePosition.x -= gap;
            } 
        }
    }

    void DestroyChildren() 
    {
        for(int i = 0; i < this.transform.childCount; i++)
        {
           GameObject child = this.transform.GetChild(i).gameObject;

           Destroy(child);
        }
    }
}
