﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private Controls m_controls;

    private void OnEnable()
    {
        // Unity input doesn't support reloads, so it may get wiped.
        if (m_controls == null)
        {
            m_controls = new Controls();
        }
        m_controls.Enable();
    }

    void Update()
    {

        if (Keyboard.current.wKey.isPressed)
        {
            cam.transform.position += cam.transform.up * Time.deltaTime * moveSpeed;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            cam.transform.position -= cam.transform.up * Time.deltaTime * moveSpeed;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            cam.transform.position -= cam.transform.right * Time.deltaTime * moveSpeed;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            cam.transform.position += cam.transform.right * Time.deltaTime * moveSpeed;
        }

        if (m_controls.UI.ScrollWheel.ReadValue<Vector2>().y > 0)
        {
            zoom -= scrollSpeed;
        }
        else if (m_controls.UI.ScrollWheel.ReadValue<Vector2>().y < 0)
        {
            zoom += scrollSpeed;
        }
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        cam.orthographicSize = zoom;
    }
}
