using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public class dragon : MonoBehaviour
{

    public OVRHand leftHand;
    public OVRHand rightHand;
    private OVRHand[] hands = new OVRHand[2];

    Thread thread;
    public int connectionPort = 25001;
    TcpListener server;
    TcpClient client;
    bool running;
    Vector3 pivotPoint; // the point around which the ball is rotating
    Vector3 zeroPoint; // the point where the ball rests on the table when theta = 0
    Vector3 rotationPlaneNormal; //normal vector to the plane that the ball is in at all points of its rotation
    float theta;
    private LineRenderer lineRenderer;
    float[] dataToSend = {-2.0f, 0.2f, 0.0f, 0.2f};
    bool pivotPointSet = false;
    bool isApplicationQuitting = false;
    bool weighty = false;

    private void Awake()
    {
        // Receive on a separate thread so Unity doesn't freeze waiting for data
        ThreadStart ts = new ThreadStart(GetData);
        thread = new Thread(ts);
        thread.Start();
        lineRenderer = GetComponent<LineRenderer>();

        hands[0] = leftHand;
        hands[1] = rightHand;
    }

    private void OnEnable()
    {
        running = true;
        theta = -1.0f;
        lineRenderer.enabled = false;
        if (!pivotPointSet) {
            zeroPoint = transform.position;
            pivotPoint = transform.position - 0.2f * transform.forward + 0.005f * Vector3.down;
            Vector3 u = new Vector3(zeroPoint.x-pivotPoint.x, 0, zeroPoint.z-pivotPoint.z);
            u = u.normalized;
            rotationPlaneNormal = Vector3.Normalize(Vector3.Cross(u, Vector3.up));
            pivotPointSet = true;
        }
    }

    private void OnDisable()
    {
        if (isApplicationQuitting) return;

        dataToSend[0] = 0.0f;
    }

    private void OnApplicationQuit () {

        isApplicationQuitting = true;

        dataToSend[0] = -1.0f;
    }

    private void GetData()
    {
        server = new TcpListener(IPAddress.Any, connectionPort);
        server.Start();
        client = server.AcceptTcpClient();

        server.Start();

        NetworkStream nwStream = client.GetStream();
        byte[] buffer;

        // Start listening
        while (running)
        {
            // Read data from the network stream
            buffer = new byte[client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

            // Decode the bytes into an array of floats
            float[] dataReceived = new float[bytesRead / 4];
            Buffer.BlockCopy(buffer, 0, dataReceived, 0, bytesRead);

            if (dataReceived != null && dataReceived.Length > 0)
            {
                
                theta = dataReceived[0];

                buffer = new byte[dataToSend.Length * 4];

                Buffer.BlockCopy(dataToSend, 0, buffer, 0, buffer.Length);

                // Send stuff back to Python:
                nwStream.Write(buffer, 0, buffer.Length);
            }
        }
        server.Stop();
    }


    // The amount of feedforward torque needed to compensate for gravity when the arm is parallel to the ground
    float torqueConstant = 0.25f;

    // theta is in radians
    private float CalculateFeedforwardTorque(float theta) {
        return Mathf.Cos(theta) * torqueConstant;
    }

    // Update is called once per frame
    void Update()
    {   

        if (theta > -1.0f)
        {

            Vector3 u = new Vector3(zeroPoint.x-pivotPoint.x, 0, zeroPoint.z-pivotPoint.z);
            u = u.normalized;
            Vector3 v = Vector3.up;

            float thetaBiasRadians = (theta*6.2832f)+0.025f;
            transform.position = pivotPoint + (0.2f*Mathf.Cos(thetaBiasRadians))*u + (0.2f*Mathf.Sin(thetaBiasRadians))*v;

            lineRenderer.enabled = true;
            List<Vector3> lineVerts = new List<Vector3>();

            lineVerts.Add(transform.position);
            lineVerts.Add(pivotPoint);

            Vector3 finalTarget = transform.position;
            Vector3 finalGesturePoint = finalTarget;
            float finalTargetRanking = 9999f;

            foreach (OVRHand hand in hands)
            {
                GestureTracker gesture = hand.GetComponent<GestureTracker>();
                if (gesture.pinching || gesture.palmUp || (weighty && !gesture.palmDown))
                {
                    Vector3 target = gesture.pinching ? gesture.indexTip : gesture.palmCenterPoint;
                    Vector3 targetProjected = target - Vector3.Dot(target - pivotPoint, rotationPlaneNormal) * rotationPlaneNormal;
                    float d = (targetProjected-pivotPoint).magnitude;
                    Vector3 pathPoint = pivotPoint + (targetProjected-pivotPoint).normalized * 0.2f;

                    if (gesture.pinching && d < finalTargetRanking && d > 0.125 && d < 0.4
                            && targetProjected.y > pivotPoint.y-0.045)
                    {
                        finalTarget = targetProjected;
                        finalGesturePoint = target;
                        finalTargetRanking = d;
                    }
                    else if (!gesture.pinching && d+10 < finalTargetRanking && (pathPoint-target).magnitude < 0.08
                                && targetProjected.y < pivotPoint.y+0.1
                                && targetProjected.y > pivotPoint.y-0.06)
                    {
                        finalTarget = targetProjected - Vector3.up*0.025f;
                        finalGesturePoint = target;
                        finalTargetRanking = d + 10;
                        weighty = true;
                    }
                }
            }


            if (finalTargetRanking < 9000)
            {
                Vector3 a = finalTarget - pivotPoint;
                float unclampedAngleTurns = (Vector3.Angle(u, a)/360f - 0.025f/6.2832f);
                dataToSend[0] = 1.0f;
                dataToSend[1] = Mathf.Clamp(unclampedAngleTurns, 0.025f, 0.475f - 0.025f/6.2832f);
                if (weighty) {
                    float b = Vector3.Distance(finalTarget, transform.position) < 0.25 ? 0.0f : CalculateFeedforwardTorque(thetaBiasRadians);
                    dataToSend[2] = Mathf.Lerp(dataToSend[2], b, 0.005f);
                    dataToSend[3] = 0.25f;
                }
                else
                {
                    dataToSend[2] = CalculateFeedforwardTorque(thetaBiasRadians);
                    dataToSend[3] = 0.75f;
                    weighty = false;
                }

                lineVerts.Add(finalTarget);
                lineVerts.Add(finalGesturePoint);
            }
            else // just stay in place
            {
                dataToSend[0] = 1.0f;
                if (weighty) theta = Mathf.Lerp(theta, 0.25f - 0.025f/6.2832f, 0.5f);
                weighty = false;
                dataToSend[1] = theta;
                dataToSend[2] = CalculateFeedforwardTorque(thetaBiasRadians);
                dataToSend[3] = 0.25f;
            }

            lineRenderer.positionCount = lineVerts.Count;
            for (int i = 0; i < lineVerts.Count; i++)
            {
                lineRenderer.SetPosition(i, lineVerts[i]);
            }

            Debug.Log("Sent: [" + dataToSend[0] + ", " + dataToSend[1] + ", " + dataToSend[2] + ", " + dataToSend[3] + "]; Received: " + theta);
        }
    }
}
