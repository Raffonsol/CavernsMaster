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
    public bool dragging = false;
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
    void FixedUpdate(){
        // if (Input.GetButtonUp("Fire1") && hovering && !fixedPos) {
        //     dragging = true;
        //     CameraController.noDrag = true;
        // }
        if (Input.GetMouseButtonDown(0)&& !fixedPos)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray,Mathf.Infinity);
                    
            if(hit.collider != null && hit.collider.transform == gameObject.transform)
                {
                    dragging = true;
                    CameraController.noDrag = true;
                }
        }
    }
    public void Drop()
    {
        
        float[] coordinates = Util.GetPositionPerType(inRoom.defs.type, inQuadrant);
        transform.position = new Vector2(coordinates[0] + lastQuadrant.transform.position.x + diviShift.x, coordinates[1] + lastQuadrant.transform.position.y + diviShift.y);
        fixedPosition = transform.position;
        inRoom = lastQuadrant.gameObject.transform.parent.GetComponent<CaveRoom>();
        inQuadrant = Int32.Parse(lastQuadrant.name.Substring(8));
        try {
            gameObject.GetComponent<Fighter>().inRoom = inRoom;
            inRoom.DeclareFigher(gameObject.GetComponent<Fighter>());
        } catch{}
    }

    void OnMouseDown()
    {
        if(!fixedPos){
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

    bool isCoroutineExecuting = false;
    IEnumerator ExecuteAfterTime(float time, Action task)
    {
        if (isCoroutineExecuting)
            yield break;
            
        isCoroutineExecuting = true;

        yield return new WaitForSeconds(time);

        task();

        isCoroutineExecuting = false;
    }
}
