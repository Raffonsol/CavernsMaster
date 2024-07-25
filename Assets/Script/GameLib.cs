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




// -=-=-=-=-=-=-=-Chars-=-=-=-=-=-=-=-=-=-=- //

public enum MovementType
{
    Guarding, // for stationary dungeon mobs
    Patrolling, // for patrolling dungeon mobs
    Advancing, // for dungeon attackers
    Retreating, // attackers once life is low or they started repeating themselves
}

[Serializable]
public class CharStats { // saveable object
    public string name;
    public int faction; // 0 = dungeon | 1 = goodGuys (bad)

    public int level;
    public float range;
    public float moveSpeed;

    public float lifeCurrent;
    public float lifeMax;
    public float attackDamage;
    public float armor;

    public int goldDroppedMin;
    public int goldDroppedMax;

     public CharStats Clone()
    {
        return (CharStats)this.MemberwiseClone();
    }
}
[Serializable]
public class CharAdjustments { // saveable object
    public float feetX = 1f;
    public float feetY = -3f;
}

[Serializable]
public class UniqueChar { // game data
    public int id;

    public CharStats stats;
    public CharAdjustments adjustments;
    
    public Sprite foot1;
    public Sprite foot2;
    public Sprite bodyDown;
    public Sprite bodyUp;
    public Sprite attackDown;
    public Sprite attackUp;

    public UniqueChar Clone()
    {
        return (UniqueChar)this.MemberwiseClone();
    }
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
    [SerializeField]
    public UniqueChar[] monsters; 
    [SerializeField]
    public UniqueChar[] evilGoodGuys; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
