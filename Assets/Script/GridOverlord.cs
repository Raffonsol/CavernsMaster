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

    // Start is called before the first frame update
    void SecondAwake()
    {
        Cursor.SetCursor(GridOverlord.Instance.gameLib.defaultCursorTexture, Vector2.zero, CursorMode.Auto);
        roomGrid = new Dictionary<int,  Dictionary<int, GameObject>>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
            if (defs.type != RoomType.BirthRoom) {
            Debug.Log("Creating "+defs.type);
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
                            Debug.Log("IT HAPPENED "+take+ " ("+newX+","+newY+")");
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
        }
    }
    // public void FindNearestQuadrant(GameObject quadrant) {
    //     Debug.Log(position);
    //     //Debug.Log(roomGrid[1][1].GetComponent<CaveRoom>().defs.xPos);
    // }
    public bool DoesRoomWork(RandomRoom random, string direction) {
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
