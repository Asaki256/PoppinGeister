
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float maxAngle = 45;
    private Vector3 totalAngle = Vector3.zero;
    private GameObject fieldObject;
    [SerializeField]
    private Vector2 rotationSpeed;
    [SerializeField]
    private bool reverse;

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

                var newAngle = Vector3.zero;
                newAngle.x = x * rotationSpeed.x;
                newAngle.y = y * rotationSpeed.y;

                this.transform.RotateAround(fieldObject.transform.position, Vector3.up, newAngle.x);
                this.transform.RotateAround(fieldObject.transform.position, transform.right, newAngle.y);
                lastMousePosition = Input.mousePosition;
            }
            else
            {
                var x = Input.mousePosition.x - lastMousePosition.x;
                var y = lastMousePosition.y - Input.mousePosition.y;

                var newAngle = Vector3.zero;
                newAngle.x = x * rotationSpeed.x;
                newAngle.y = y * rotationSpeed.y;

                this.transform.RotateAround(fieldObject.transform.position, Vector3.up, newAngle.x);
                this.transform.RotateAround(fieldObject.transform.position, transform.right, newAngle.y);
                lastMousePosition = Input.mousePosition;
            }
        }
    }
}