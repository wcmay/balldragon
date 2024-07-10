using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class RaySceneInteractionManager : MonoBehaviour
{
    private OVRCameraRig cameraRig;
    private GameObject ghostSphere;
    private GameObject ghostArm;
    private LineRenderer lineRenderer;

    public EffectMesh effectMesh;
    private EffectMesh effectMeshScript;

    public OVRHand leftHand;
    public OVRHand rightHand;
    private GestureTracker leftGesture;
    private GestureTracker rightGesture;


    public Material virtualMaterial;
    public Material passthroughMaterial;
    public Material selectionGhost;

    public GameObject dragon;

    bool xDown;
    bool aDown;

    bool anchored;

    private void Awake()
    {
        cameraRig = FindObjectOfType<OVRCameraRig>();
        lineRenderer = GetComponent<LineRenderer>();
        effectMeshScript = effectMesh.GetComponent<EffectMesh>();

        anchored = false;

        ghostSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ghostSphere.transform.localScale = dragon.transform.localScale;
        ghostSphere.GetComponent<Collider>().enabled = false;
        ghostSphere.GetComponent<Renderer>().material = selectionGhost;
        ghostSphere.SetActive(false);

        dragon.SetActive(false);

        leftGesture = leftHand.GetComponent<GestureTracker>();
        rightGesture = rightHand.GetComponent<GestureTracker>();
    }

    private Ray GetControllerRay()
    {
        Vector3 rayOrigin;
        Vector3 rayDirection;
        if (OVRInput.activeControllerType == OVRInput.Controller.Touch // right controller ray
            || OVRInput.activeControllerType == OVRInput.Controller.RTouch)
        {
            rayOrigin = cameraRig.rightControllerInHandAnchor.position;
            rayDirection = cameraRig.rightControllerInHandAnchor.forward;
        }
        else // right hand ray
        {
            rayOrigin = rightHand.PointerPose.position;
            rayDirection = rightHand.PointerPose.forward;
        }

        return new Ray(rayOrigin, rayDirection);
    }

    // Update is called once per frame
    public void Update()
    {

        // Not currently using  pinch input

        xDown = OVRInput.GetDown(OVRInput.RawButton.X) && !leftGesture.pinchDown;
        aDown = OVRInput.GetDown(OVRInput.RawButton.A) && !rightGesture.pinchDown;

        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick)) { // reset button
            anchored = false;
            dragon.SetActive(false);
        }

        if (dragon.activeSelf == false)
        {
            if (anchored == false)
            {
                lineRenderer.enabled = false;
                var ray = GetControllerRay();
                ghostSphere.SetActive(true);
                ghostSphere.transform.position = ray.origin + ray.direction*0.1f;
                if (xDown)
                {
                    anchored = true;
                }
            }
            else // anchored == true
            {
                Vector2 LThumb = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                float ytrans = Mathf.Abs(LThumb.y) > 0.6f ? LThumb.y - 0.6f : 0.0f;
                ytrans *= 0.005f;
                float rotate = Mathf.Abs(LThumb.x) > 0.4f ? LThumb.x - 0.4f : 0.0f;
                rotate *= 1.5f;

                ghostSphere.transform.Translate(new Vector3(0, ytrans, 0));
                ghostSphere.transform.Rotate(0, rotate, 0);

                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, ghostSphere.transform.position);
                lineRenderer.SetPosition(1, ghostSphere.transform.position - 0.2f*ghostSphere.transform.forward);

                if (xDown)
                {
                    dragon.transform.position = ghostSphere.transform.position;
                    dragon.transform.rotation = ghostSphere.transform.rotation;
                    dragon.SetActive(true);
                    lineRenderer.enabled = false;
                    ghostSphere.SetActive(false);
                }
            }
        }
        else //dragon.activeSelf == true
        {

        }

        if (OVRInput.GetDown(OVRInput.RawButton.Y)) {
            OVRPassthroughLayer passthrough = cameraRig.GetComponent<OVRPassthroughLayer>();
            passthrough.enabled = !passthrough.enabled;
            Material newMaterial = passthrough.enabled ? passthroughMaterial : virtualMaterial;
            effectMeshScript.OverrideEffectMaterial(newMaterial);
        }

    }
}
