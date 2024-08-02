using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameData : MonoBehaviour
{
    public int level = 0;
    public int[] currencyAmounts;
    public int lastRoomId = 0;
    public int lastCharId = 0;



    public void AddCurrency(int currencyIndex, int value) {
        currencyAmounts[currencyIndex] += value;
    }
    public bool CheckIfCanAfford(int currencyIndex, int value) {
        return currencyAmounts[currencyIndex] >= value;
    }
    public void SpendCurrency(int currencyIndex, int value) {
        currencyAmounts[currencyIndex] -= value;
    }


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
        level = 0;
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
        for (int i = 0; i < GridOverlord.Instance.gameLib.currencies.Length; i++)
        {
            currencyAmounts[i] = GridOverlord.Instance.gameLib.currencies[i].startingAmount;
        }
    }
}
