using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Scripting.Python;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public class dragon : MonoBehaviour
{

    public OVRHand leftHand;
    public OVRHand rightHand;
    private GestureTracker leftGesture;
    private GestureTracker rightGesture;

    Thread thread;
    public int connectionPort = 25001;
    TcpListener server;
    TcpClient client;
    bool running;
    Vector3 pivotPoint; // the point around which the ball is rotating
    Vector3 zeroPoint; // the point where the ball rests on the table when theta = 0
    float theta;
    private LineRenderer lineRenderer;
    float[] dataToSend = {-2.0f, -2.0f};
    bool pivotPointSet = false;
    bool isApplicationQuitting = false;

    private void Awake()
    {
        // Receive on a separate thread so Unity doesn't freeze waiting for data
        ThreadStart ts = new ThreadStart(GetData);
        thread = new Thread(ts);
        thread.Start();
        lineRenderer = GetComponent<LineRenderer>();

        leftGesture = leftHand.GetComponent<GestureTracker>();
        rightGesture = rightHand.GetComponent<GestureTracker>();        
    }

    private void OnEnable()
    {
        running = true;
        theta = -1.0f;
        lineRenderer.enabled = false;
        if (!pivotPointSet) {
            zeroPoint = transform.position;
            pivotPoint = transform.position - 0.2f * transform.forward + 0.005f * Vector3.down;
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
                //Debug.Log(dataReceived[0]);
                
                theta = dataReceived[0];

                buffer = new byte[dataToSend.Length * 4];

                Buffer.BlockCopy(dataToSend, 0, buffer, 0, buffer.Length);

                // Send stuff back to Python:
                nwStream.Write(buffer, 0, buffer.Length);
            }
        }
        server.Stop();
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
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, pivotPoint);

            dataToSend[0] = 1.0f;
        }
    }
}
