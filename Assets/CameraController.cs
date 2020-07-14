using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{

    [Header("Camera")]
    public Camera cam;
    public float moveSpeed = 32.0f;
    public float scrollSpeed = 20.0f;
    [Header("Zoom")]
    public float minZoom = 5.0f;
    public float maxZoom = 250.0f;

    float zoom;

    void Update()
    {

        if (Input.GetKey(KeyCode.W))
        {
            cam.transform.position += cam.transform.up * Time.deltaTime * moveSpeed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            cam.transform.position -= cam.transform.up * Time.deltaTime * moveSpeed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            cam.transform.position -= cam.transform.right * Time.deltaTime * moveSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            cam.transform.position += cam.transform.right * Time.deltaTime * moveSpeed;
        }

        if (Input.mouseScrollDelta.y > 0)
        {
            zoom -= moveSpeed * Time.deltaTime * scrollSpeed;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            zoom += moveSpeed * Time.deltaTime * scrollSpeed;
        }
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        cam.orthographicSize = zoom;
    }
}
