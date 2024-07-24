using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
   public float moveSpeed;
    public float zoomSpeed;
    public float minZoomDist;
    public float maxZoomDist;
    public float dragSpeed = 2;
    public float dragBreaks;
    public static bool noDrag = false;
    
    private Vector3 dragOrigin;
    private float goingDragSpeed;
    private Camera cam;

    void Awake ()
    {
        cam = Camera.main;
    }

    
    void Update ()
    {
        Move();
        DragMove();
        Zoom();
    }
    void Move ()
    {
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        Vector3 dir = transform.up * zInput + transform.right * xInput;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }
    void DragMove()
    {
        if (noDrag) return;
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            goingDragSpeed= dragSpeed;
            return;
        }
 
        if (!Input.GetMouseButton(0)) return;
 
        if (Vector3.Distance(dragOrigin, Input.mousePosition) > 100)
        goingDragSpeed -= goingDragSpeed * dragBreaks;
        //else goingDragSpeed= dragSpeed;

        //Debug.Log(Vector3.Distance(dragOrigin, Input.mousePosition));

        Vector3 pos = cam.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(pos.x * goingDragSpeed * -1, pos.y * goingDragSpeed * -1, 0);
 
        transform.Translate(move, Space.World);  
    }
    void Zoom ()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        float dist = Vector3.Distance(transform.position, cam.transform.position);
    
        if( scrollInput > 0.0f && cam.GetComponent<Camera>().orthographicSize < minZoomDist)
            return;
        else if(scrollInput < 0.0f && cam.GetComponent<Camera>().orthographicSize > maxZoomDist)
            return;
        cam.GetComponent<Camera>().orthographicSize -= (cam.transform.forward.z * scrollInput * zoomSpeed);
    }

}
