using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;



// ^v^v^v^v^v^v^v^Support^v^v^v^v^v^v^v^v^v^v^ //


[Serializable]
public class Currency 
{
    public int id;
    public string name;
    public int startingAmount;
    public Sprite icon;
}
[Serializable]
public class Coordinates 
{
    public int x;
    public int y;
}





// *.*.*.*.*.*.*.*Items*.*.*.*.*.*.*.*.*.*.* //


public enum MysteryOptionType
{
    Division,
    Mob,
    Trap,
    Bonus
}
[Serializable]
public class MysteryOption
{
    public string name;
    public string description;

    public MysteryOptionType type;
    public int optionItemIndex;

    public Sprite icon;
}
[Serializable]
public class MysteryItem
{
    public int id;
    public string name;
    public int minLevel;
    public MysteryOption[] mysteryOptions;
    public Sprite sprite;
    public float scale;
}
[Serializable]
public class DivisionUsage
{
    public int id;
    public string name;
    public Sprite sprite;
    public float life;
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
    * - wm - warrior menu - expects warrior menu identifier - 3 identifier chars
    * - ff - lose condition - if this is destroyed, lose game - no identifier or anything
    * - gp - grow population - some method of growing population - 0 - every turn, grow by half - 1 - gain 1 per turn - 2 - gain 1 when enemy dies nearby
    */
    public string effect;
    public Vector2 shift;


    // adjustments
    public float scale;
    public bool hasShadow = true;
}



// -=-=-=-=-=-=-=-Map-=-=-=-=-=-=-=-=-=-=- //


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
    public Coordinates[] path;
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




// -o-o-o-o-o-o-o-Chars-o-o-o-o-o-o-o-o-o-o- //

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
    public float attackCooldown;
    public float attackDamageDelay = 0.5f;

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
public class CharAdjustments { 
    public float feetX = 1f;
    public float feetY = -3f;
    public float heaveFactor = 1f;
    public float feetAnimFactor = 1f;
    public float stepDuration = 0.5f;
    public float scale = 1f;

    public bool noHeave=false;
    public bool bodyChangeOnMove=false;
}

[Serializable]
public class UniqueChar { // game data
    public int id;

    public CharStats stats;
    public CharAdjustments adjustments;

    public AudioClip attackSound;
    public AudioClip deathSound;
    
    public Sprite foot1;
    public Sprite foot2;
    public Sprite bodyDown;
    public Sprite bodyUp;
    public Sprite attackDown;
    public Sprite attackUp;

    public Sprite rangedAttackSprite;

    public UniqueChar Clone()
    {
        return (UniqueChar)this.MemberwiseClone();
    }
}



// ~=~=~=~=~=~=~=~Waveees~=~=~=~=~=~=~=~=~=~=~ //


[Serializable]
public class Wave {
    public float timeOffset;
    public int[] attackers;
}
[Serializable]
public class Raid { // saveable object
    public int id;
    public int level;
    public Wave[] waves; 

    public int reward;
    
    public float timeAfterLastWaveToEnd = 10f;
}




// |=|-|=|-|=|-|=|Market|=|-|=|-|=|-|=|-|=|-| //


public enum SaleType
{
    NewRoom,
    Defender,
    Asset,
}

[Serializable]
public class Sale 
{
    public int id;
    public string name;
    public int cost;
    public int currency;
    public SaleType saleType;
    public int saleItemIndex;
}






public class GameLib : MonoBehaviour
{
    // default prefabs
    public GameObject[] roomPrefab;
    public GameObject plusPrefab;
    public GameObject mobPrefab;
    public GameObject hitPrefab;
    public GameObject deathPrefab;
    public GameObject projectilePrefab;
    public GameObject floatTextPrefab;
    public GameObject itemPrefab;

    // music
    public AudioClip[] homeMusic;
    public AudioClip[] combatMusic;
    public AudioClip newRoom;
    public AudioClip errorSound;

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
    [SerializeField]
    public Raid[] raids; 
    [SerializeField]
    public Sale[] sales; 
    [SerializeField]
    public MysteryItem[] mysteryItems;

    
}
