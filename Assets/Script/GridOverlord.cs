using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class GridOverlord : MonoBehaviour
{
    #region  Singleton
    public static GridOverlord Instance { get; private set; }
    // Singleton stuff
    private void Awake() 
    { 
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }
        DontDestroyOnLoad(gameObject);
        SecondAwake();
    }
    #endregion

    public GameData gameData;
    public GameLib gameLib;
    public UIManager uIManager;
    public Dictionary<int, Dictionary<int, GameObject>> roomGrid;

    public List<Fighter> defenders = new List<Fighter>();
    public List<Fighter> attackers = new List<Fighter>();

    private bool raidOngoing = false;
    private Raid ongoingRaid;
    private float raidTimer = 0f;
    private List<int> waveIndexes;


    // Start is called before the first frame update
    void SecondAwake()
    {
        Cursor.SetCursor(GridOverlord.Instance.gameLib.defaultCursorTexture, Vector2.zero, CursorMode.Auto);
        roomGrid = new Dictionary<int,  Dictionary<int, GameObject>>();
    }

    // Update is called once per frame
    void Update()
    {
        if (raidOngoing) {
            raidTimer+=Time.deltaTime;

            for (int i = 0; i < ongoingRaid.waves.Length; i++)
            {
                if (raidTimer >= ongoingRaid.waves[i].timeOffset && !waveIndexes.Contains(i)){
                    waveIndexes.Add(i);
                    for (int j = 0; j < ongoingRaid.waves[i].attackers.Length; j++)
                    {
                        CreateAttacker(ongoingRaid.waves[i].attackers[j]);
                    }
                }
            }
        }
    }

    #nullable enable
    public void CreateRoom(RoomDefinition? defParam, string direction, int? take = 0)
    #nullable disable
    {
        RoomDefinition defs;
        RandomRoom randomRoom = new RandomRoom();
        GameObject roomObj;

        if (defParam == null){
            defs = new RoomDefinition()
            // main room
            {
                size = 4,
                isMain = true,
                isFinal = false,
                leftDoor = false,
                frontDoor = false,
                rightDoor = false,
                backDoor = false,
                contentIds = new int[]{0, -1, -1, -1},
                xPos=0,
                yPos=0,
                type= RoomType.DefaultRoom,
            };
            roomObj = GameObject.Instantiate( gameLib.roomPrefab[0]);
        }
         else{
            defs = defParam;
            if (take>5) {
                // too many random tries, must not be birth room, and let's just use 1x1 cause nothing else will work
                roomObj = GameObject.Instantiate( gameLib.roomPrefab[0]);
                take = 0;
            }
            else if (defs.type != RoomType.BirthRoom) {
                // Picking a random room
                List<RandomRoom> options = gameLib.randomRooms.OfType<RandomRoom>().ToList();
                options.Shuffle();
                int i = 0;
                randomRoom = options[i];
                // Does it work?
                while(!DoesRoomWork(randomRoom, direction)) {
                    i++;
                    randomRoom = options[i];
                }
                defs.type = randomRoom.type;

                // set sprite
                roomObj = GameObject.Instantiate( gameLib.roomPrefab[(int)randomRoom.type]);
                roomObj.GetComponent<SpriteRenderer>().sprite = randomRoom.sprite;
            } else {
                roomObj = GameObject.Instantiate( gameLib.roomPrefab[1]);
            }
            
        }
        //defs
        defs.id = gameData.lastRoomId;
        gameData.lastRoomId++;

        // instantiation | positioning | grid setup | door connections setup
        if (defs.type == RoomType.DefaultRoom || defs.type == RoomType.BirthRoom){
            if (!defs.isMain) {
                // make connection
                defs.frontDoor = direction == "down";
                defs.leftDoor = direction == "right";
                defs.rightDoor = direction == "left";
                defs.backDoor = direction == "up";
            }
            // instantiation
            roomObj.GetComponent<CaveRoom>().defs = defs;
            // roomObj = GameObject.Instantiate(roomObj);
            roomObj.transform.localPosition = new Vector3(defs.xPos * 9.6f,defs.yPos* 9.6f);
        
            // grid works
            if (!roomGrid.ContainsKey(defs.xPos)) {
                roomGrid[defs.xPos] = new Dictionary<int, GameObject>();
            }
            roomGrid[defs.xPos][defs.yPos] = roomObj;
        
            roomObj.GetComponent<CaveRoom>().CheckStatus();
        } else  { // for not standard rooms
            DoorType doorNeeded = direction == "down" ? DoorType.North
                   : doorNeeded = direction == "up" ? DoorType.South
                   : doorNeeded = direction == "left" ? DoorType.East : DoorType.West;
            List<Door> doorOptions = randomRoom.doors.OfType<Door>().ToList();
            doorOptions.Shuffle();
            Door doorToUse = new Door();

            // find door to use
            doorOptions.Any(opt => {
                if(opt.type == doorNeeded) {
                    doorToUse = opt;
                    return true;
                }
                return false;
            });

            List<CaveRoom> quarters = new List<CaveRoom>();
            for (int i = 0; i < randomRoom.size/4; i++) // for each quarter in the room
            {
                // set defs on quarters
                CaveRoom quarter = roomObj.transform.Find("quarter"+(i+1)).GetComponent<CaveRoom>();
                RoomDefinition quarterDefs = defs.Clone();
                quarter.defs = quarterDefs;
                quarters.Add(quarter);
                
                bool isConnectingQuarter = quarter.quarterDefinition.xShift == doorToUse.xSqaure && quarter.quarterDefinition.yShift == doorToUse.ySqaure;
                quarter.quarterDefinition.isFirstQuarter = isConnectingQuarter;
                quarter.quarterDefinition.availableDoors = doorOptions.Where(door => door.xSqaure == quarter.quarterDefinition.xShift && door.ySqaure == quarter.quarterDefinition.yShift).Select(door=>door.type).ToList();

                // define connections
                quarterDefs.frontDoor = direction == "down" && isConnectingQuarter;
                quarterDefs.leftDoor = direction == "right" && isConnectingQuarter;
                quarterDefs.rightDoor = direction == "left" && isConnectingQuarter;
                quarterDefs.backDoor = direction == "up" && isConnectingQuarter ;

                // roomObj = GameObject.Instantiate(roomObj);
                int newX = defs.xPos-doorToUse.xSqaure+quarter.quarterDefinition.xShift; quarter.defs.xPos = newX;
                int newY = defs.yPos-doorToUse.ySqaure+quarter.quarterDefinition.yShift; quarter.defs.yPos = newY;

                // make sure dictionary is ready for check first (always be ready for 2)
                if (!roomGrid.ContainsKey(newX)) {
                    roomGrid[newX] = new Dictionary<int, GameObject>();
                }
                if (!roomGrid.ContainsKey(newX+1)) {
                    roomGrid[newX+1] = new Dictionary<int, GameObject>();
                }
                if(i==0){
                    // only do this for the fist quarter of a longer room
                    roomObj.transform.localPosition = new Vector3(newX * 9.6f,newY* 9.6f);

                    // check if there will be trouble with this or other quarters
                    if (
                        roomGrid[newX].ContainsKey(newY) 
                        || ((randomRoom.type == RoomType.twoXtwo || randomRoom.type == RoomType.TwoXone ) && roomGrid[newX+1].ContainsKey(newY))
                        || ((randomRoom.type == RoomType.twoXtwo || randomRoom.type == RoomType.oneXtwo ) && roomGrid[newX].ContainsKey(newY+1))
                        || ((randomRoom.type == RoomType.twoXtwo ) && roomGrid[newX+1].ContainsKey(newY+1))
                    ) {
                        if (take < 10) {
                            Destroy(roomObj);
                            CreateRoom(defParam, direction, take+1);
                        } else {
                            Debug.LogError("No room was working.");
                        }
                        return;
                    }
                }
                
                // grid works
                roomGrid[newX][newY] = quarter.gameObject;
            }
            for (int i = 0; i < quarters.Count; i++)
            {
                quarters[i].CheckStatus();

               
            }
            bool firstQuarterFound = false;
            for (int i = 0; i < randomRoom.path.Length; i++)
            {
                 // pathing
                int quarterIndex = quarters.FindIndex(0,quarters.Count, (CaveRoom q) => randomRoom.path[i].x == q.quarterDefinition.xShift && randomRoom.path[i].y == q.quarterDefinition.yShift);
                // I don't even want to get into this. Basically it creates paths by giving each room a forward array of all the possible rooms to go forward to, and a backwards array with how to return from it
                if (i-1 >= 0) {
                    if (firstQuarterFound){
                        quarters[quarterIndex].backwardsCoo.Add(new Coordinates(){x = randomRoom.path[i-1].x+quarters[quarterIndex].defs.xPos - quarters[quarterIndex].quarterDefinition.xShift, y=randomRoom.path[i-1].y+quarters[quarterIndex].defs.yPos - quarters[quarterIndex].quarterDefinition.yShift,});
                    }
                    else{
                        quarters[quarterIndex].forwardCoo.Add(new Coordinates(){x = randomRoom.path[i-1].x+quarters[quarterIndex].defs.xPos - quarters[quarterIndex].quarterDefinition.xShift, y=randomRoom.path[i-1].y+quarters[quarterIndex].defs.yPos - quarters[quarterIndex].quarterDefinition.yShift,});
                    }
                }
                if (quarters[quarterIndex].quarterDefinition.isFirstQuarter)
                        firstQuarterFound = true;
                        
                
                if (i+1 < randomRoom.path.Length){
                    if (firstQuarterFound){
                        quarters[quarterIndex].forwardCoo.Add(new Coordinates(){x = randomRoom.path[i+1].x+quarters[quarterIndex].defs.xPos - quarters[quarterIndex].quarterDefinition.xShift, y=randomRoom.path[i+1].y+quarters[quarterIndex].defs.yPos - quarters[quarterIndex].quarterDefinition.yShift,});
                    }
                    else{
                        quarters[quarterIndex].backwardsCoo.Add(new Coordinates(){x = randomRoom.path[i+1].x+quarters[quarterIndex].defs.xPos - quarters[quarterIndex].quarterDefinition.xShift, y=randomRoom.path[i+1].y+quarters[quarterIndex].defs.yPos - quarters[quarterIndex].quarterDefinition.yShift,});
                    }
                }
            }
        }
    }
    // public void FindNearestQuadrant(GameObject quadrant) {
    //     Debug.Log(position);
    //     //Debug.Log(roomGrid[1][1].GetComponent<CaveRoom>().defs.xPos);
    // }

    public void CreateMob(int mobIndex) {
        GameObject instance = GameObject.Instantiate(gameLib.mobPrefab);
        instance.transform.position = roomGrid[0][-1].transform.position;
        Fighter fighter = instance.GetComponent<Fighter>();
        UniqueChar mobDef = gameLib.monsters[mobIndex];
        fighter.adjustments = mobDef.adjustments;
        fighter.stats = mobDef.stats;

        fighter.foot1 = mobDef.foot1;
        fighter.foot2 = mobDef.foot2;
        fighter.bodyUp = mobDef.bodyUp;
        fighter.bodyDown = mobDef.bodyDown;
        fighter.attackDown = mobDef.attackDown;
        fighter.attackUp = mobDef.attackUp;

        // instance.AddComponent<PolygonCollider2D>();
        Draggable drag = instance.AddComponent<Draggable>();
        drag.fixedPos = false;
        drag.inRoom = roomGrid[0][-1].GetComponent<CaveRoom>();
        drag.inQuadrant = 0;

        fighter.CheckStatus();
    }
    public void CreateAttacker(int goodGuysIndex) {
        GameObject instance = GameObject.Instantiate(gameLib.mobPrefab);
        instance.transform.position = GameObject.FindGameObjectsWithTag("Respawn")[0].transform.position + new Vector3(UnityEngine.Random.Range(-2f,2f),UnityEngine.Random.Range(-2f,2f),0);
        Fighter fighter = instance.GetComponent<Fighter>();
        UniqueChar attackerDef = gameLib.evilGoodGuys[goodGuysIndex];
        fighter.adjustments = attackerDef.adjustments;
        fighter.stats = attackerDef.stats;

        fighter.currentMovementType = MovementType.Advancing;

        fighter.foot1 = attackerDef.foot1;
        fighter.foot2 = attackerDef.foot2;
        fighter.bodyUp = attackerDef.bodyUp;
        fighter.bodyDown = attackerDef.bodyDown;
        fighter.attackDown = attackerDef.attackDown;
        fighter.attackUp = attackerDef.attackUp;

        fighter.inRoom = roomGrid[0][0].GetComponent<CaveRoom>();

        fighter.CheckStatus();
    }

    public void StartNextRaid() {
        // CreateAttacker(gameLib.raids[0].waves[0].attackers[0]);
        int raidIndex = gameData.level;
        ongoingRaid = gameLib.raids[raidIndex];
        waveIndexes = new List<int>();
        raidTimer = 0f;
        raidOngoing = true;
    }

    public void StopRaid() {
       raidOngoing = false;
    }

    private bool DoesRoomWork(RandomRoom random, string direction) {
        if ( // door avialability check
            (direction.Equals("up") && !random.doors.Any(d => d.type == DoorType.South))
         || (direction.Equals("down") && !random.doors.Any(d => d.type == DoorType.North))
         || (direction.Equals("left") && !random.doors.Any(d => d.type == DoorType.East))
         || (direction.Equals("right") && !random.doors.Any(d => d.type == DoorType.West))
        ) {
            return false;
        }
        // if ( // positioning fits check
        //         random.type == RoomType

        // ) {
        //     return false
        // }
        return true;
    }
}
