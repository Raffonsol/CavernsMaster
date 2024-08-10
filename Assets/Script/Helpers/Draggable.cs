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
    public GameObject lastQuadrant;

    public int thingIndex = -1;
    
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
        if (lastQuadrant == null) {
            GridOverlord.Instance.ShowError("Cannot be dropped here!", transform.position);
            return;
        }
        CaveRoom targetRoom = lastQuadrant.gameObject.transform.parent.GetComponent<CaveRoom>();
        if (thingIndex != -1 && inQuadrant != Int32.Parse(lastQuadrant.name.Substring(8)) && targetRoom.defs.contentIds[Int32.Parse(lastQuadrant.name.Substring(8))] != -1) {
            GridOverlord.Instance.ShowError("There is already something here!", transform.position);
            return;
        }

        if (inRoom != null && thingIndex!= -1) {
            inRoom.defs.contentIds[inQuadrant] = -1;
            inRoom.sections[inQuadrant] = null;
        }

        dragging = false;
        CameraController.noDrag = false;

        float[] coordinates = Util.GetPositionPerType(inRoom.defs.type, inQuadrant);
        transform.position = new Vector2(coordinates[0] + lastQuadrant.transform.position.x + diviShift.x, coordinates[1] + lastQuadrant.transform.position.y + diviShift.y);
        fixedPosition = transform.position;
        inRoom = targetRoom;
        inQuadrant = Int32.Parse(lastQuadrant.name.Substring(8));
        Target tar = gameObject.GetComponent<Target>();
        if (tar!= null) { // for targetable thing sbeing dragged
            tar.inRoom = targetRoom;
            tar.inQuadrant = Int32.Parse(lastQuadrant.name.Substring(8));
        }
        if (thingIndex != -1){ // for things being dragged
            inRoom.defs.contentIds[inQuadrant] = thingIndex;
            inRoom.sections[inQuadrant] = gameObject;
        }
        try {
            gameObject.GetComponent<Fighter>().inRoom = inRoom;
            inRoom.DeclareFigher(gameObject.GetComponent<Fighter>());
        } catch{}
    }

    void OnMouseDown()
    {
        if(!fixedPos && GridOverlord.Instance.attackers.Count <= 0){
            dragging = true;
            CameraController.noDrag = true;
        }
    }
    void OnMouseUp()
    {
        if(dragging){
            Drop();
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
