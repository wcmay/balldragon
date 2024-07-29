using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTracking : MonoBehaviour
{
    public Transform target;
    public Transform eyesTarget;
    public Transform headBone;

    public float headTrackingSpeed;

    public Transform leftEyeBone;
    public Transform rightEyeBone;
    


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate() 
    {
        HeadTrackingUpdate();
        EyeTrackingUpdate();
    }

    void HeadTrackingUpdate()
    {
        // Store the current head rotation since we will be resetting it
        Quaternion currentLocalRotation = headBone.localRotation;
        // Reset the head rotation so our world to local space transformation will use the head's zero rotation. 
        // Note: Quaternion.Identity is the quaternion equivalent of "zero"
        headBone.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = target.position - headBone.position;
        Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir);

        // Apply angle limit
        targetLocalLookDir = Vector3.RotateTowards(
            Vector3.forward,
            targetLocalLookDir,
            Mathf.Deg2Rad * 360, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
            0 // We don't care about the length here, so we leave it at zero
        );

        // Get the local rotation by using LookRotation on a local directional vector
        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);

        // Apply smoothing
        headBone.localRotation = Quaternion.Slerp(
            currentLocalRotation,
            targetLocalRotation, 
            1 - Mathf.Exp(-headTrackingSpeed * Time.deltaTime)
        );
    }

    void EyeTrackingUpdate()
    {

        Vector3 targetLookDir = Vector3.RotateTowards(
            headBone.forward,
            eyesTarget.position - headBone.position,
            Mathf.Deg2Rad * 40, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
            0 // We don't care about the length here, so we leave it at zero
        );

        Quaternion targetRotation = Quaternion.LookRotation(targetLookDir, Vector3.up);

        leftEyeBone.rotation = targetRotation;
        rightEyeBone.rotation = targetRotation;
        
    }
}
