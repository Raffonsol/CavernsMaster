using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public class Currency 
{
    public int id;
    public string name;
    public float startingAmount;
    public Sprite icon;
}

[Serializable]
public class DivisionUsage
{
    public int id;
    public string name;
    public GameObject gameObject;
    public float life;
    public string effect;
    /**
    * Effect codes
    * *Rules
    * - each 2 characters represent a unique effect code
    * - certain codes are expected to be followed by quantity number, which ends on | when not specified by code
    * - certain codes are expected to be followed by identifier number, ending depends on code
    * *Codes
    * - fp - fixed position - no identifier or quantity char
    * - cs - currency storage - expects identifier (0 = gold, 1 = population) - 2 identifier chars
    * - tg - target (attackers will go to this) - expects satisfaction percentage quantity - 3 percentage chars
    * - rq - required - cannot be deleted, only moved - no identifier or quantity char
    * - wm - warrior menu - expects warrior unit identifier - 3 identifier chars
    */
    public Vector2 shift;
}
public enum DoorType
{
    North,
    East,
    South,
    West,
    Down,
    Up
}
[Serializable]
public class Door {
    public DoorType type; // direction
    public int xSqaure;
    public int ySqaure; // 0,1 is west facing door on bottom left | 4,1 would be the right end of a horizontal hallway
}
public enum RoomType
{
    DefaultRoom,
    BirthRoom,
    twoXtwo,
    oneXtwo,
    TwoXone,
    threeXthree
}

[Serializable]
public class RandomRoom {
    public int id;
    public int size; // how many divisions
    public Sprite sprite;
    public RoomType type;
    public Door[] doors;
}

[Serializable]
public class RoomDefinition { // saveable object
    public int id;
    public int size = 4; // how many divisions it has
    public RoomType type;
    public bool isMain = false;
    public bool isFinal = false;
    public bool leftDoor = false;
    public bool frontDoor = false;
    public bool rightDoor = false;
    public bool backDoor = false;
    public int[] contentIds; // goes clockwise starting on top-right ends at top-left TODO: DEAL WOT THUIS
    public int xPos=0;public int yPos=0; // of bottom left square

     public RoomDefinition Clone()
    {
        return (RoomDefinition)this.MemberwiseClone();
    }
}
[Serializable]
public class QuarterDefinition { 
    public bool isQuarter = false;
    public int xShift=0;public int yShift=0; 
    public bool isFirstQuarter = false;

    // [HideInInspector]
    public List<DoorType> availableDoors = new List<DoorType>();
}

public class GameLib : MonoBehaviour
{
    // default prefabs
    public GameObject[] roomPrefab;
    public GameObject plusPrefab;

    // textures
    public Texture2D pointerCursorTexture;
    public Texture2D defaultCursorTexture;

    //game content
    [SerializeField]
    public DivisionUsage[] roomDivisions;
    [SerializeField]
    public Currency[] currencies; 
    [SerializeField]
    public RandomRoom[] randomRooms; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
