using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureTracker : MonoBehaviour
{

    public bool pinching;
    public bool pinchDown;
    public bool palmUp;
    public bool palmDown;
    public Vector3 palmCenterPoint;
    public Vector3 indexTip;


    private GameObject ghostSphere;
    public Material selectionGhost;

    void Awake()
    {
        pinchDown = false;
        pinching = false;
        indexTip = gameObject.GetComponent<OVRSkeleton>().Bones[20].Transform.position;

        ghostSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ghostSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        ghostSphere.GetComponent<Collider>().enabled = false;
        ghostSphere.GetComponent<Renderer>().material = selectionGhost;
        ghostSphere.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        pinchDown = gameObject.GetComponent<OVRHand>().GetFingerIsPinching(OVRHand.HandFinger.Index) && !pinching;
        pinching = gameObject.GetComponent<OVRHand>().GetFingerIsPinching(OVRHand.HandFinger.Index);
        palmUp = gameObject.transform.up.y < -0.75;
        palmDown = gameObject.transform.up.y > 0.75;
        palmCenterPoint = gameObject.transform.position - gameObject.transform.right*0.07f*gameObject.transform.localScale.x;

        ghostSphere.transform.position = indexTip;
    }
}
