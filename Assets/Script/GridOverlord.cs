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

    public List<PopGrower> popGs = new List<PopGrower>();

    private bool raidOngoing = false;
    private bool lastWaveSent = false;
    private Raid ongoingRaid;
    private float raidTimer = 0f;
    private float raidCountdown = 0f;
    private List<int> waveIndexes;

    private int roomCount = 0;


    // Start is called before the first frame update
    void SecondAwake()
    {
        Cursor.SetCursor(GridOverlord.Instance.gameLib.defaultCursorTexture, Vector2.zero, CursorMode.Auto);
        roomGrid = new Dictionary<int,  Dictionary<int, GameObject>>();
    }

    // Update is called once per frame
    void Update()
    {
        ControlMusic();
        RunRaid();
        
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
                contentIds = new int[]{0, 2, -1, -1},
                xPos=0,
                yPos=0,
                type= RoomType.DefaultRoom,
            };
            roomObj = GameObject.Instantiate( gameLib.roomPrefab[0]);
        }
         else{
            defs = defParam.Clone();
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
            roomObj.GetComponent<CaveRoom>().defs = defs.Clone();
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
                quarter.defs = defs.Clone();
                quarters.Add(quarter);
                
                bool isConnectingQuarter = quarter.quarterDefinition.xShift == doorToUse.xSqaure && quarter.quarterDefinition.yShift == doorToUse.ySqaure;
                quarter.quarterDefinition.isFirstQuarter = isConnectingQuarter;
                quarter.quarterDefinition.availableDoors = doorOptions.Where(door => door.xSqaure == quarter.quarterDefinition.xShift && door.ySqaure == quarter.quarterDefinition.yShift).Select(door=>door.type).ToList();

                // define connections
                quarter.defs.frontDoor = direction == "down" && isConnectingQuarter;
                quarter.defs.leftDoor = direction == "right" && isConnectingQuarter;
                quarter.defs.rightDoor = direction == "left" && isConnectingQuarter;
                quarter.defs.backDoor = direction == "up" && isConnectingQuarter ;

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
            bool firstQuarterFound = false;
            for (int i = 0; i < randomRoom.path.Length; i++)
            {
                int quarterIndex = quarters.FindIndex(0,quarters.Count, (CaveRoom q) => randomRoom.path[i].x == q.quarterDefinition.xShift && randomRoom.path[i].y == q.quarterDefinition.yShift);
                // random content
                quarters[quarterIndex].defs.contentIds = new int[]{UnityEngine.Random.Range(0,15) > 1 ? -1 : 3, UnityEngine.Random.Range(0,15) > 1 ? -1 : 3, UnityEngine.Random.Range(0,15) > 1 ? -1 : 3, UnityEngine.Random.Range(0,15) > 1 ? -1 : 3};
                //enable quarters for dragging things into
                for (int j = 0; j < 4; j++)
                {
                    quarters[quarterIndex].quadrants[j].SetActive(true);   
                }
                
                 // pathing
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
            for (int i = 0; i < quarters.Count; i++)
            {
                quarters[i].CheckStatus();

               
            }
        }
        
        
        // if it's third room, game can start
        roomCount+=1;
        if (roomCount == 3) {
            uIManager.ShowNextRaidButton();
        }
    }
    // public void FindNearestQuadrant(GameObject quadrant) {
    //     Debug.Log(position);
    //     //Debug.Log(roomGrid[1][1].GetComponent<CaveRoom>().defs.xPos);
    // }

    public void CreateMob(int mobIndex) {
        GameObject instance = GameObject.Instantiate(gameLib.mobPrefab);
        instance.transform.position = roomGrid[0][-1].transform.position + new Vector3(UnityEngine.Random.Range(-1.3f,1.3f),UnityEngine.Random.Range(-0.3f,1.3f), 0);
        Fighter fighter = instance.GetComponent<Fighter>();
        defenders.Add(fighter);
        UniqueChar mobDef = gameLib.monsters[mobIndex];
        fighter.adjustments = mobDef.adjustments;
        fighter.stats = mobDef.stats.Clone();
        instance.transform.localScale = instance.transform.localScale*fighter.adjustments.scale;

        fighter.foot1 = mobDef.foot1;
        fighter.foot2 = mobDef.foot2;
        fighter.bodyUp = mobDef.bodyUp;
        fighter.bodyDown = mobDef.bodyDown;
        fighter.attackDown = mobDef.attackDown;
        fighter.attackUp = mobDef.attackUp;
        fighter.rangedAttackSprite = mobDef.rangedAttackSprite;

        fighter.attackSound = mobDef.attackSound;
        fighter.deathSound = mobDef.deathSound;

        HoverTarget ht = instance.AddComponent<HoverTarget>();
        ht.uITypes = new TargetType[]{TargetType.Stats};
        ht.subTypeIds = new int[]{0};
        Draggable drag = instance.AddComponent<Draggable>();
        drag.fixedPos = false;
        drag.inRoom = roomGrid[0][-1].GetComponent<CaveRoom>();
        drag.inQuadrant = 0;
        drag.diviShift = new Vector2(UnityEngine.Random.Range(-1.2f,1.2f),UnityEngine.Random.Range(-1.2f,1.2f));
        drag.dragging = true;

        fighter.CheckStatus();
    }
    public void CreateAttacker(int goodGuysIndex) {
        GameObject instance = GameObject.Instantiate(gameLib.mobPrefab);
        instance.transform.position = GameObject.FindGameObjectsWithTag("Respawn")[0].transform.position + new Vector3(UnityEngine.Random.Range(-1.5f,1.5f),UnityEngine.Random.Range(-1.5f,1.5f),0);
        Fighter fighter = instance.GetComponent<Fighter>();
        attackers.Add(fighter);
        UniqueChar attackerDef = gameLib.evilGoodGuys[goodGuysIndex];
        fighter.adjustments = attackerDef.adjustments;
        fighter.stats = attackerDef.stats.Clone();

        fighter.currentMovementType = MovementType.Advancing;

        fighter.foot1 = attackerDef.foot1;
        fighter.foot2 = attackerDef.foot2;
        fighter.bodyUp = attackerDef.bodyUp;
        fighter.bodyDown = attackerDef.bodyDown;
        fighter.attackDown = attackerDef.attackDown;
        fighter.attackUp = attackerDef.attackUp;
        fighter.rangedAttackSprite = attackerDef.rangedAttackSprite;
        
        fighter.attackSound = attackerDef.attackSound;
        fighter.deathSound = attackerDef.deathSound;

        fighter.inRoom = roomGrid[0][0].GetComponent<CaveRoom>();

        fighter.CheckStatus();
    }

    public void StartNextRaid() {
        // CreateAttacker(2);
        // CreateAttacker(2);
        Camera.main.GetComponent<CameraController>().PlayMusic(gameLib.combatMusic[UnityEngine.Random.Range(0, gameLib.combatMusic.Length)]);
        int raidIndex = gameData.level;
        if (raidIndex >= gameLib.raids.Length) raidIndex = gameLib.raids.Length-1;
        ongoingRaid = gameLib.raids[raidIndex];
        waveIndexes = new List<int>();
        raidTimer = 0f;
        lastWaveSent = false;
        raidOngoing = true;
    }

    public void StopRaid() {
       raidOngoing = false;
    }

    public void ShowError(string errorMessage, Vector2 position) {
        GameObject floaty = GameObject.Instantiate(gameLib.floatTextPrefab);
        floaty.GetComponent<FloatingText>().textToDisplay = errorMessage;
        floaty.transform.position = position;
        Camera.main.GetComponent<CameraController>().PlaySound(gameLib.errorSound);
    }

    private void RunRaid() {
        if (!raidOngoing)  return;
        
        raidTimer+=Time.deltaTime;

        if (lastWaveSent) {
            raidCountdown-=Time.deltaTime;
            if (raidCountdown < 0) {
                // RAID OVER
                gameData.level++;
                raidOngoing = false;
                gameData.AddCurrency(0,ongoingRaid.reward);
                for (int i = 0; i < popGs.Count; i++)
                {
                    popGs[i].RunGrowth();
                }
                uIManager.ShowNextRaidButton();
            }
        } else
        for (int i = 0; i < ongoingRaid.waves.Length; i++)
        {
            if (raidTimer >= ongoingRaid.waves[i].timeOffset && !waveIndexes.Contains(i)){
                waveIndexes.Add(i);
                for (int j = 0; j < ongoingRaid.waves[i].attackers.Length; j++)
                {
                    CreateAttacker(ongoingRaid.waves[i].attackers[j]);
                }
                if (waveIndexes.Count == ongoingRaid.waves.Length) {
                    lastWaveSent = true;
                    raidCountdown = ongoingRaid.timeAfterLastWaveToEnd;
                }
            }
        }
        
    }
    private void ControlMusic() {
        if (!raidOngoing && attackers.Count == 0)
            Camera.main.GetComponent<AudioSource>().Stop();
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
