using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plusScript : MonoBehaviour
{
    public float intensity = 1f;
    public CaveRoom forCave;
    float shakeInterval = 0.5f;
    float shakeTimer = 1f;
    bool sign = false; // Return to original position or new position?
    public int xPos = 0;
    public int yPos = 0;
    Quaternion targetRotation = Quaternion.identity; // Lerp target.
    bool hovering = false;
    public string direction; // up, left, right, down, or empty string for initial room
    // Start is called before the first frame update
    void Start()
    {
        Color color = GetComponent<SpriteRenderer>().color;
        color.a = 0.1f;
        GetComponent<SpriteRenderer>().color = color;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (shakeTimer > 0) {
            shakeTimer -= Time.deltaTime;
        } else {
            shakeTimer = shakeInterval;
            if (sign) {
                targetRotation = Quaternion.identity;
            } else {
                targetRotation = Quaternion.Euler(Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity), Random.Range(-intensity, intensity));
            }
             // Flip the sign.
            sign = !sign;
            transform.localRotation = targetRotation;
        }
        
        
    }
    void OnMouseUp()
    {
        if(hovering){
            GridOverlord.Instance.CreateRoom(new RoomDefinition(){
                size = 0,
                contentIds = new int[]{-1, -1, -1, -1},
                xPos=xPos,
                yPos=yPos,
                type = RoomType.DefaultRoom
            }, direction);
            forCave.OpenRoom(direction);

            // add this room as a backwards movement option for just opened room
            CaveRoom newRoom = GridOverlord.Instance.roomGrid[xPos][yPos].GetComponent<CaveRoom>();
            newRoom.backwardsCoo.Add(new Coordinates(){x=forCave.defs.xPos,y=forCave.defs.yPos,});
            // add new room as forwards option for this room
            forCave.forwardCoo.Add(new Coordinates(){x=xPos,y=yPos,});
        }
    }

    void OnMouseEnter()
    {
        shakeInterval = 0.1f;
        Color color = GetComponent<SpriteRenderer>().color;
        color.a = 1f;
        GetComponent<SpriteRenderer>().color = color;
        Cursor.SetCursor(GridOverlord.Instance.gameLib.pointerCursorTexture, Vector2.zero, CursorMode.Auto);
        hovering = true;
    }

    void OnMouseExit()
    {
        shakeInterval = 0.5f;
        Color color = GetComponent<SpriteRenderer>().color;
        color.a = 0.1f;
        GetComponent<SpriteRenderer>().color = color;
        Cursor.SetCursor(GridOverlord.Instance.gameLib.defaultCursorTexture, Vector2.zero, CursorMode.Auto);
        hovering = false;
    }
}
