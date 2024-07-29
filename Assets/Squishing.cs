using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Squishing : MonoBehaviour
{
    //[Range(0.7f, 1.3f)] public float xYSquishValue = 1f;
    [Range(0.7f, 1.3f)] public float singleAxisSquishValue = 1f;

    [Range(0.7f, 1.3f)] public float xSquishValue = 1f;
    [Range(0.7f, 1.3f)] public float ySquishValue = 1f;
    [Range(0.7f, 1.3f)] public float zSquishValue = 1f;

    public SquashStretchAxis axisToAffect = SquashStretchAxis.Y;
    public Transform transformToAffect;

    Vector3 originalScale;
    Vector3 modifiedScale;

    public bool useSeparateSquishValues = false;

    private bool affectX => (axisToAffect & SquashStretchAxis.X) != 0;
    private bool affectY => (axisToAffect & SquashStretchAxis.Y) != 0;
    private bool affectZ => (axisToAffect & SquashStretchAxis.Z) != 0;

    //IEnumerator squishCoroutine;

    [System.Flags]
    public enum SquashStretchAxis
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
    }

    // Start is called before the first frame update
    void Start()
    {
        //squishCoroutine = squishCOR();
        originalScale = transformToAffect.localScale;
    }

    // IEnumerator squishCOR()
    // {
    //     while (transformToAffect.localScale )
    //     {
    //         float effect = squishByValue * Time.deltaTime;
    //         transformToAffect.localScale = new Vector3(2f - effect, effect, 1f);
    //         //Wait for the next frame
    //         yield return null;
    //     }
    //     StopCoroutine(squishCoroutine);
    // }

    // Update is called once per frame
    void Update()
    {
        if(!useSeparateSquishValues)
        {
            if (affectX)
                modifiedScale.x = originalScale.x * singleAxisSquishValue;
            else
                modifiedScale.x = originalScale.x;

            if (affectY)
                modifiedScale.y = originalScale.y * singleAxisSquishValue;
            else
                modifiedScale.y = originalScale.y;

            if (affectZ)
                modifiedScale.z = originalScale.z * singleAxisSquishValue;
            else
                modifiedScale.z = originalScale.z;
        } else 
        {
            modifiedScale.x = originalScale.x * xSquishValue;
            modifiedScale.y = originalScale.y * ySquishValue;
            modifiedScale.z = originalScale.z * zSquishValue;
        }

        

        transformToAffect.localScale = modifiedScale;
    }

    // best results with values from 0.7 to 1.3
    void XYSquish(float squishBy)
    {
        //transformToAffect.localScale = new Vector3(2f - squishBy, squishBy, 1f);
    }

    void OnMouseDown()
    {
        Debug.Log("down");
        //StartCoroutine(squishCoroutine);
    }
}
