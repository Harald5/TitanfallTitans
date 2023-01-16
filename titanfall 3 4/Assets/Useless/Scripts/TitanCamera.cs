using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanCamera : MonoBehaviour
{
    public GameObject cameraPos;
    public float minX = -60f;
    public float maxX = 60f;
 
    public float sensitivity;
    public Camera cam;
    public GameObject aim;
 
    float rotY = 0f;
    float rotX = 0f;

    EnterTitan et;
    TitanMovement tm;

    bool shouldHeadBob;

    public float runBobSpeed;
    public float walkBobSpeed;
    public float walkBobAmount;
    public float runBobAmount;
    public float defaultY;
    private float timer;
 
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        et = GetComponent<EnterTitan>();
        tm = GetComponent<TitanMovement>();
    }
 
    void Update()
    {
        if (et.inTitan && !tm.isDead)
        {
            rotY += Input.GetAxis("Mouse X") * sensitivity;
            rotX += Input.GetAxis("Mouse Y") * sensitivity;
 
            rotX = Mathf.Clamp(rotX, minX, maxX);
    
            transform.localEulerAngles = new Vector3(0, rotY, 0);
            cam.transform.localEulerAngles = new Vector3(-rotX, 0, 0);
            //cam.transform.position = cameraPos.transform.position;    

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
                aim.transform.position = raycastHit.point;   

            //Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            //aim.transform.position = mouseWorldPosition;   

            HandleHeadBob();   
        }

    }

    void HandleHeadBob()
    {
        if (tm.isMoving)
        {
            timer += Time.deltaTime * (tm.isRunning ? runBobSpeed : walkBobSpeed);
            cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, defaultY + Mathf.Sin(timer) * (tm.isRunning ? runBobAmount : walkBobAmount), cam.transform.localPosition.z);
        }
    }
}
