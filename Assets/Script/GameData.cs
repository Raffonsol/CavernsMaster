using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameData : MonoBehaviour
{
    public int day;
    public int[] currencyAmounts;
    public int lastRoomId = 0;
    // Start is called before the first frame update
    void Start()
    {
        NewGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void NewGame() {
        GridOverlord.Instance.CreateRoom(null, "");
        GridOverlord.Instance.CreateRoom(new RoomDefinition()
        {
            size = 4,
            isMain = true,
            isFinal = false,
            leftDoor = false,
            frontDoor = false,
            rightDoor = false,
            backDoor = false,
            contentIds = new int[]{-1, 1, -1, -1},
            xPos=0,
            yPos=-1,
            type= RoomType.BirthRoom,
        }, "");
        currencyAmounts = new int[GridOverlord.Instance.gameLib.currencies.Length];
    }
}
