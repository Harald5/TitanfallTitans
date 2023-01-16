using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    public float minX = -60f;
    public float maxX = 60f;
 
    public float sensitivity;
    public Camera cam;
 
    float rotY = 0f;
    float rotX = 0f;

    Movement m;
    MovementWithAnimation mva;
    EnterTitan et;
    EnterVanguardTitan titanScript;
    public Transform embarkPos;
 
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        titanScript = FindObjectOfType<EnterVanguardTitan>();
        m = GetComponent<Movement>();
        mva = GetComponent<MovementWithAnimation>();
        et = FindObjectOfType<EnterTitan>();
    }
 
    void Update()
    {
        if (!et.isEmbarking)
        {
            rotY += Input.GetAxis("Mouse X") * sensitivity;
            rotX += Input.GetAxis("Mouse Y") * sensitivity;
    
            rotX = Mathf.Clamp(rotX, minX, maxX);
    
            transform.localEulerAngles = new Vector3(0, rotY, 0);
            cam.transform.localEulerAngles = new Vector3(-rotX, 0, mva.tilt);
        }
        else if (et.isEmbarking)
        {
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        }
    }
}
