using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportation : MonoBehaviour
    {
    //bool isVisible = false;
//    bool sizeFlip = true;

    Vector3 walkableBounds;
    public GameObject area;
    public int areaSize;
    private float grav;

    public GameObject debugSphere;
    public GameObject pointer;

    public Camera cam;

    public OVRInput.Button RTrigger;
    public OVRInput.Button LTrigger;

    CharacterController CC;

    public float personHeight = 2.5f;

    private float height;
    private Quaternion areaRotation;

    // Start is called before the first frame update
    void Start()
    {
        CC = GetComponent<CharacterController>();
        areaRotation = area.transform.rotation;
        GameObject walkableArea = GameObject.FindGameObjectWithTag("Walkable");
        Bounds b = walkableArea.GetComponent<MeshRenderer>().bounds;
        Vector3 size = b.max -b.min;

        height = b.max.y;
        Debug.Log("Height: " + height.ToString());
        //height = walkableArea.transform.position.y;
        /*walkableBounds = new Vector3(
            (walkableArea.transform.lossyScale.x * 5) - (area.transform.lossyScale.x * 5),
            0,
            (walkableArea.transform.lossyScale.z * 5) - (area.transform.lossyScale.z * 5)
        );*/
    /*
        b = area.GetComponent<MeshRenderer>().bounds;
        Vector3 size2 = b.max -b.min;
        walkableBounds = new Vector3(size.x - size2.x, 0, size.z - size2.z);*/
    }

    // Update is called once per frame
    void Update()
    {  
        if(!CC.isGrounded)
        {
            grav += Physics.gravity.y * Time.deltaTime;
        }
        CC.Move(new Vector3(0,Physics.gravity.y, 0));

        //transform.Rotate(0, -cam.transform.rotation.y, 0);
/*
        if (transform.localScale.x > 5 || transform.localScale.x < 1)
        {
            sizeFlip = !sizeFlip;
        }

        if (sizeFlip) {
            transform.localScale = new Vector3(transform.localScale.x + 0.2f, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x - 0.2f, transform.localScale.y, transform.localScale.z);
        }
*/      OVRInput.Update();


        if (OVRInput.Get(RTrigger)) // RMB
        {
            RaycastHit hit;
            Vector3 rPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch );
            Quaternion rRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch );
            if (Physics.Raycast(transform.position, rRot * Vector3.forward, out hit) && hit.transform.tag == "Walkable")
            {
                //Debug.DrawRay(pointer.transform.position, pointer.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow, 10);
                //pointer.transform.TransformDirection(Vector3.forward) * hit.distance;
                /*Debug.Log( "x: " + hit.point.x
                        + " y: " + hit.point.y
                        + " z: " + hit.point.z
                        + "\nx: " + walkableBounds.x
                        + " y: " + height
                        + " z: " + walkableBounds.z
                        );
                */
                area.transform.position = new Vector3(
                    hit.point.x,
                    height + 0.01f,
                    hit.point.z
                );
                //area.transform.rotation = Vector3.zero;
                //Debug.Log("Obj - distance: " + hit.distance);

                if (!area.GetComponent<Renderer>().enabled) 
                    area.GetComponent<Renderer>().enabled = !area.GetComponent<Renderer>().enabled;
            }
            else {
                area.transform.position = new Vector3(transform.position.x, height + 0.01f, transform.position.z);
            }

            if (OVRInput.GetDown(LTrigger)) // LMB
            {
                transform.position = new Vector3(area.transform.position.x, area.transform.position.y + personHeight, area.transform.position.z);
            }
        }
        else if (area.GetComponent<Renderer>().enabled)
        {
            area.GetComponent<Renderer>().enabled = false;
        }
    }

    void LateUpdate()
    {
        area.transform.rotation = areaRotation;
    }
}
