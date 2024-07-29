using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonTailWag : MonoBehaviour
{
    // public GameObject sphere;
    // public GameObject tail;

    // Vector3 spherePosition;
    // Vector3 dragonPosition;

    // Vector3 dragonTailRotation;

    // float diffInPos;

    // float sign = -1.0f;
    // bool flip = false;

    // public float speed = 1;
    // public float RotAngleY = 15;


    private float rotation;

    public float distance;
    public float min_distance = 4.0f;
    public float max_distance = 20.0f;

    private float t;
    public float min_t = 0.05f;
    public float max_t = 10.0f;

    private float base_z;

    public GameObject tracked_object;
    public GameObject tail;    
    public float amplitude;

    void Awake() 
    {
        // spherePosition = sphere.gameObject.transform.position;
        // dragonPosition = this.gameObject.transform.position;

        // tail = tail.gameObject;

        base_z = this.transform.eulerAngles.z;
        rotation = 0.0f;
        t = 0.0f;

        //tail = this.gameObject.transform.Find("Tail.2").gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // spherePosition = sphere.gameObject.transform.position;
        // dragonPosition = this.gameObject.transform.position;

        // diffInPos = dragonPosition.x - spherePosition.x;

        // // float rY = Mathf.SmoothStep(-15,RotAngleY,Mathf.PingPong(Time.time * speed,1));
        // // tail.transform.rotation = Quaternion.Euler(0,rY,0);


        // if (tail.transform.localRotation.eulerAngles.y > 15 || tail.transform.localRotation.eulerAngles.y < -15) {
        //     flip = true;
        // } else {
        //     flip = false;
        // }
            
        // if (flip) {
        //     sign = sign * (-1.0f);
        // }

        // tail.transform.Rotate(0.0f, 0.0f, sign * 0.03f);
        // // //tail.transform.Rotate(0.0f, -0.03f, 0.0f);

        // // Debug.Log(tail.transform);


        distance = Vector3.Distance(tracked_object.transform.position, tail.transform.position);        
        t += Mathf.Lerp(min_t, max_t, (Mathf.Clamp(distance, min_distance, max_distance) - min_distance) / (max_distance - min_distance));
       
        rotation = (float) (amplitude * Mathf.Sin(t));
        tail.transform.eulerAngles = new Vector3(tail.transform.eulerAngles.x, tail.transform.eulerAngles.y, base_z + rotation);



    }
}
