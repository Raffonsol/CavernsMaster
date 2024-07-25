using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Draggable : MonoBehaviour
{
    public CaveRoom inRoom;
    public int inQuadrant;
    public Vector2 diviShift;
    public bool fixedPos = false;
    bool hovering = false;
    bool dragging = false;
    Vector2 fixedPosition;
    GameObject lastQuadrant;
    
    public void ForcedStart()
    {
        fixedPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (dragging) {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector2(pos.x, pos.y);
        }
    }
    public void Drop()
    {
        
        float[] coordinates = Util.GetPositionPerType(inRoom.defs.type, inQuadrant);
        transform.position = new Vector2(coordinates[0] + lastQuadrant.transform.position.x + diviShift.x, coordinates[1] + lastQuadrant.transform.position.y + diviShift.y);
        fixedPosition = transform.position;
        inRoom = lastQuadrant.gameObject.transform.parent.GetComponent<CaveRoom>();
        inQuadrant = Int32.Parse(lastQuadrant.name.Substring(8));
    }

    void OnMouseDown()
    {
        if(hovering && !fixedPos){
            dragging = true;
            CameraController.noDrag = true;
        }
    }
    void OnMouseUp()
    {
        if(dragging){
            Drop();
            dragging = false;
            CameraController.noDrag = false;
        }
    }

    void OnMouseEnter()
    {
        hovering = true;
    }

    void OnMouseExit()
    {
        hovering = false;
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        
        if (col.gameObject.tag == "Quadrant")
        {
            lastQuadrant = col.gameObject;
        }
    }
}
