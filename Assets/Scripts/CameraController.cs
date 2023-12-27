
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // public Vector3 cameraSpeed = new Vector3(0.6f,0.6f,10.0f);
    // public Vector3 cameraTarget = Vector3.zero;
    // public Vector3 cameraAngle = new Vector3(-40.0f,0.0f,0.0f);
    // public float cameraDistance = 10.0f;

    // private Vector3 lastMousePosition;


    // void Start()
    // {
    //     UpdateCamera();
    // }

    // void Update()
    // {
    //     if(Input.GetMouseButtonDown(0))
    //     {
    //         Vector3 dragOffset = Input.mousePosition - lastMousePosition;
    //         cameraAngle.x = (cameraAngle.x + dragOffset.y * cameraSpeed.x) % 360.0f;
    //         cameraAngle.y = (cameraAngle.y - dragOffset.x * cameraSpeed.y) % 360.0f;
            
    //     }

    //     lastMousePosition = Input.mousePosition;

    //     UpdateCamera();
    // }

    // void UpdateCamera()
    // {
    //     transform.rotation = Quaternion.Euler(-cameraAngle);
    //     transform.position = cameraTarget + transform.rotation * Vector3.back * cameraDistance;
    // }



    // // GameObject mainCamera;
    // GameObject fieldObject;
    // public float rotateSpeed = 1.0f;
    // // private Vector3 lastMousePosition;//一つ前のMousePosを記憶
    // // private Vector3 newAngle = new Vector3(0, 0, 0);
    // void Start()
    // {
    //     // this.mainCamera = Camera.main.gameObject;
    //     this.fieldObject = GameObject.Find("FieldCenter");//フィールド
    // }
    // void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         // newAngle = transform.localEulerAngles;
    //         // lastMousePosition = Input.mousePosition;
    //     }
    //     else if (Input.GetMouseButton(0))
    //     {
    //         rotateCamera();
    //     }
    // }

    // private void rotateCamera()
    // {
    //     Vector3 angle = new Vector3(
    //             Input.GetAxis("Mouse X") * this.rotateSpeed,
    //             Input.GetAxis("Mouse Y") * this.rotateSpeed,
    //             0
    //         );
    //     this.transform.RotateAround(this.fieldObject.transform.position, Vector3.up, angle.x);

    // }

    private float maxAngle = 45;
    // private float minAngle = 0;
    private Vector3 totalAngle = Vector3.zero;
    private GameObject fieldObject;
    [SerializeField]
    private Vector2 rotationSpeed;
    [SerializeField]
    private bool reverse;


    // private Camera mainCamera;
    public Vector2 lastMousePosition;
    void Start()
    {
        this.fieldObject = GameObject.Find("FieldCenter");
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            if (!reverse)
            {
                var x = lastMousePosition.x - Input.mousePosition.x;
                var y = Input.mousePosition.y - lastMousePosition.y;

                // if (Mathf.Abs(x) < Mathf.Abs(y))
                //     x = 0;
                // else
                //     y = 0;

                var newAngle = Vector3.zero;
                newAngle.x = x * rotationSpeed.x;
                newAngle.y = y * rotationSpeed.y;


                // if (maxAngle < totalAngle.y + newAngle.y)
                //     newAngle.y = maxAngle - totalAngle.y;
                // if (totalAngle.y + newAngle.y < -maxAngle)
                //     newAngle.y = -maxAngle - totalAngle.y;

                // totalAngle.y += newAngle.y;

                this.transform.RotateAround(fieldObject.transform.position, Vector3.up, newAngle.x);
                this.transform.RotateAround(fieldObject.transform.position, transform.right, newAngle.y);
                lastMousePosition = Input.mousePosition;
            }
            else
            {
                var x = Input.mousePosition.x - lastMousePosition.x;
                var y = lastMousePosition.y - Input.mousePosition.y;

                // if (Mathf.Abs(x) < Mathf.Abs(y)) 
                //     x = 0; 
                // else 
                //     y = 0; 

                var newAngle = Vector3.zero;
                newAngle.x = x * rotationSpeed.x;
                newAngle.y = y * rotationSpeed.y;

                // if (maxAngle < totalAngle.y + newAngle.y)
                //     newAngle.y = maxAngle - totalAngle.y;
                // if (totalAngle.y + newAngle.y < -maxAngle)
                //     newAngle.y = -maxAngle - totalAngle.y;

                // totalAngle.y += newAngle.y;

                this.transform.RotateAround(fieldObject.transform.position, Vector3.up, newAngle.x);
                this.transform.RotateAround(fieldObject.transform.position, transform.right, newAngle.y);
                lastMousePosition = Input.mousePosition;
            }
        }
    }

    // public void ResetLastPosition()
    // {
    //     lastMousePosition = Vector2.zero;
    // }
}