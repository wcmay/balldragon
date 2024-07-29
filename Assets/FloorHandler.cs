using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorHandler : MonoBehaviour
{
    //public Vector3 firstPosition = new Vector3(0, 0, 0);
    public int gap = 10;
    public float y = 1;
    public Vector2Int gridSize;

    public GameObject floorTile;
    public GameObject trackedObject;

    public float distance;

    // Start is called before the first frame update
    void Start()
    {
        for(int x=0; x < gridSize.x * gap; x += gap)
        {
            for (int z = 0; z < gridSize.y * gap; z += gap)
            {

                GameObject obj = Instantiate(floorTile, new Vector3(x, y, z), Quaternion.identity)as GameObject;
                obj.transform.parent = this.transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 position = firstPosition;

        for(int i = 0; i < this.transform.childCount; i++)
        {
           GameObject child = this.transform.GetChild(i).gameObject;

           distance = Vector3.Distance(child.transform.position, trackedObject.transform.position);

           if(distance < 25) 
           {
                var cubeRenderer = child.GetComponent<Renderer>();

                // Create a new RGBA color using the Color constructor and store it in a variable
                Color customColor = new Color(0.4f, 0.9f, 0.7f, 1.0f);

               // Call SetColor using the shader property name "_Color" and setting the color to the custom color you created
               cubeRenderer.material.SetColor("_Color", customColor);
           } else 
           {
                var cubeRenderer = child.GetComponent<Renderer>();

                // Create a new RGBA color using the Color constructor and store it in a variable
                Color customColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

               // Call SetColor using the shader property name "_Color" and setting the color to the custom color you created
               cubeRenderer.material.SetColor("_Color", customColor);
           }

           
        }
    }
}
