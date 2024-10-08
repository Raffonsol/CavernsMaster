using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CaveRoom : MonoBehaviour
{
    // game objects -- parts
    public GameObject entrance;
    public GameObject leftDoorway;
    public GameObject rightDoorway;
    public GameObject frontDoorway;

    public GameObject backPlus;
    public GameObject leftPlus;
    public GameObject rightPlus;
    public GameObject frontPlus;
    public GameObject[] quadrants;
    public QuarterDefinition quarterDefinition;

    // game objects -- content
    public List<GameObject> sections;

    // pathing
    public List<Coordinates> forwardCoo = new List<Coordinates>();
    public List<Coordinates> backwardsCoo = new List<Coordinates>();

    // definitions
    [SerializeField]
    public RoomDefinition defs;

    // Start is called before the first frame update
    void Awake()
    {
        sections = new List<GameObject>(new GameObject[4]);
        Camera.main.GetComponent<CameraController>().PlaySound(GridOverlord.Instance.gameLib.newRoom);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckStatus() {
        if (defs.type != RoomType.BirthRoom){
            // doors and plusses
            if (defs.isMain) {
                entrance.SetActive(true);
            }else if (defs.backDoor) {
                entrance.SetActive(true);
                entrance.GetComponent<SpriteRenderer>().sprite = frontDoorway.GetComponent<SpriteRenderer>().sprite;
                entrance.GetComponent<SpriteRenderer>().flipY = true;
                entrance.transform.position = new Vector2(entrance.transform.position.x, entrance.transform.position.y-0.06f);
            }else {
                entrance.SetActive(false);
                
                if (defs.backDoor ) {
                    if(backPlus!= null)backPlus.SetActive(false);
                }else {

                    if ((GridOverlord.Instance.roomGrid.ContainsKey(defs.xPos) && GridOverlord.Instance.roomGrid[defs.xPos].ContainsKey(defs.yPos - 1) )) {
                        GridOverlord.Instance.roomGrid[defs.xPos][defs.yPos - 1].GetComponent<CaveRoom>().DisablePlus("front");
                    } else if (quarterDefinition.availableDoors.Contains(DoorType.South) || defs.type == RoomType.DefaultRoom) {
                        if(backPlus!= null)backPlus.SetActive(true);else backPlus = InstantiatePlus(0f, -5f, 0,-1, "down");
                    }
                }
            }

            if (defs.leftDoor ) {
                leftDoorway.SetActive(true);
                if(leftPlus!= null)leftPlus.SetActive(false);
            }else {
                leftDoorway.SetActive(false);

                if ((GridOverlord.Instance.roomGrid.ContainsKey(defs.xPos - 1) && GridOverlord.Instance.roomGrid[defs.xPos - 1].ContainsKey(defs.yPos)) ) {
                    GridOverlord.Instance.roomGrid[defs.xPos - 1][defs.yPos].GetComponent<CaveRoom>().DisablePlus("right");
                } else if (quarterDefinition.availableDoors.Contains(DoorType.West) || defs.type == RoomType.DefaultRoom) {
                    if(leftPlus!= null)leftPlus.SetActive(true);else leftPlus = InstantiatePlus(-5f, 0f, -1,0, "left");
                }
            }

            if (defs.rightDoor ) {
                rightDoorway.SetActive(true);
                if(rightPlus!= null)rightPlus.SetActive(false);
            }else {
                rightDoorway.SetActive(false);
                if ((GridOverlord.Instance.roomGrid.ContainsKey(defs.xPos + 1) && GridOverlord.Instance.roomGrid[defs.xPos + 1].ContainsKey(defs.yPos)) ) {
                    GridOverlord.Instance.roomGrid[defs.xPos + 1][defs.yPos].GetComponent<CaveRoom>().DisablePlus("left");
                } else if (quarterDefinition.availableDoors.Contains(DoorType.East) || defs.type == RoomType.DefaultRoom) {
                    if(rightPlus!= null)rightPlus.SetActive(true);else rightPlus = InstantiatePlus(4.7f, 0f, 1, 0, "right");
                }
            }

            if (defs.frontDoor ) {
                frontDoorway.SetActive(true);
                if(frontPlus!= null)frontPlus.SetActive(false);
            }else {
                frontDoorway.SetActive(false);

                if ((GridOverlord.Instance.roomGrid.ContainsKey(defs.xPos) && GridOverlord.Instance.roomGrid[defs.xPos].ContainsKey(defs.yPos + 1)) ) {
                    GridOverlord.Instance.roomGrid[defs.xPos][defs.yPos + 1].GetComponent<CaveRoom>().DisablePlus("back");
                } else if (quarterDefinition.availableDoors.Contains(DoorType.North) || defs.type == RoomType.DefaultRoom) {
                    if(frontPlus!= null)frontPlus.SetActive(true);else frontPlus = InstantiatePlus(0f, 5f, 0,1, "up");
                }
            }
        }
        else{

        }
        // sections
        for( int i = 0; i < defs.contentIds.Length; i++) {
            if (defs.contentIds[i] > -1 && sections[i] == null) {
                DivisionUsage divi = GridOverlord.Instance.gameLib.roomDivisions[defs.contentIds[i]];
                //placing
                GameObject contenter = GameObject.Instantiate(GridOverlord.Instance.gameLib.itemPrefab);
                sections[i] = contenter;
                contenter.GetComponent<SpriteRenderer>().sprite = divi.sprite;
                contenter.transform.localScale = new Vector2(divi.scale, divi.scale);
                float[] coordinates = Util.GetPositionPerType(defs.type, i);
                Vector2 divisionPos = defs.type == RoomType.BirthRoom 
                 ? new Vector2(0,-12f) //birth room doesnt worry about divisions stuff
                 : quadrants[i].transform.position;
                 if (!divi.hasShadow) contenter.transform.Find("shadow").gameObject.SetActive(false);
                // -------------------------- modifier for room type ------ quadrant pos --- modifier from this specific division ----------------- //
                contenter.transform.position = new Vector2(coordinates[0] + divisionPos.x + divi.shift.x, coordinates[1] + divisionPos.y+ divi.shift.y);
                bool needsCollider = false;bool hasHoverTarget = false;

                if (divi.id == 3){ // hardcoded id for mystery item
                    GridOverlord.Instance.gameLib.mysteryItems.Shuffle();
                    MysteryItem item = GridOverlord.Instance.gameLib.mysteryItems[0];
                    MysteryContent mc = contenter.AddComponent<MysteryContent>();
                    mc.itemDefs = item;
                    mc.ForcedStart();
                    needsCollider=true;hasHoverTarget=true;
                    mc.inRoom = this; mc.inQuadrant = i;
                }
                //adding scripts to it
                if (divi.effect.Contains("cs")){ // hoverable to show a currency
                    HoverTarget hovTar = contenter.AddComponent<HoverTarget>();
                    hovTar.uITypes = new TargetType[]{TargetType.Life, TargetType.Currency};
                    hovTar.subTypeIds = new int[]{0, Int32.Parse(divi.effect.Substring(divi.effect.IndexOf("cs")+2,2))}; // second index for currency type
                    // hovTar.ForcedStart();
                    needsCollider=true;hasHoverTarget=true;
                    
                    if (divi.effect.Contains("gp")){ // grow population
                        PopGrower popG = contenter.AddComponent<PopGrower>();
                        popG.popCurrencyIndex = Int32.Parse(divi.effect.Substring(divi.effect.IndexOf("cs")+2,2));
                        popG.popGrowthType = Int32.Parse(divi.effect.Substring(divi.effect.IndexOf("gp")+2,1));
                    }
                }
                if (divi.effect.Contains("tg")){ // targetable
                    Target tar = contenter.AddComponent<Target>();
                    tar.tarDef = new TargetDefinitions();
                    tar.tarDef.attacker = false;
                    tar.tarDef.moveSpeed = 0;
                    tar.divi=divi;
                    tar.inRoom = this;
                    tar.inQuadrant = i;
                    // TODO: add satisfaction percentage [SASTIF]
                    tar.ForcedStart();
                    needsCollider = true;
                }
                if (divi.effect.Contains("wm")){ // hoverable to show a warrior menu
                    HoverTarget hovTar;
                    if (hasHoverTarget) {
                        hovTar = contenter.GetComponent<HoverTarget>();
                        List<TargetType> targets = new List<TargetType>(hovTar.uITypes);
                        targets.Add(TargetType.WarriorMenu);
                        hovTar.uITypes = targets.ToArray();
                        // --- subtypes ----
                        List<int> subTypes = new List<int>(hovTar.subTypeIds);
                        subTypes.Add(Int32.Parse(divi.effect.Substring(divi.effect.IndexOf("wm")+2,3)));
                        hovTar.subTypeIds = subTypes.ToArray();
                    } else {
                        hovTar = contenter.AddComponent<HoverTarget>();
                        hovTar.uITypes = new TargetType[]{TargetType.WarriorMenu};
                        hasHoverTarget=true;
                        hovTar.subTypeIds = new int[]{Int32.Parse(divi.effect.Substring(divi.effect.IndexOf("wm")+2,3))}; // second index for currency type
                    }
                    // hovTar.ForcedStart();
                    needsCollider=true;
                }
                Draggable drag = contenter.AddComponent<Draggable>();
                if (divi.effect.Contains("fp") || divi.id == 3){
                    // drag.gameObject.layer = 2;
                    drag.fixedPos = true;
                }
                drag.inRoom = this;
                drag.inQuadrant = i;
                drag.diviShift = divi.shift;
                drag.thingIndex = defs.contentIds[i];
                if (needsCollider) {
                    BoxCollider2D coll = contenter.AddComponent<BoxCollider2D>();
                    coll.isTrigger = true;
                }
                
            }
        }
        
    }
    public void DisablePlus(string kind) {
        if (kind == "back" && backPlus!= null)backPlus.SetActive(false);
        if (kind == "front" && frontPlus!= null)frontPlus.SetActive(false);
        if (kind == "left" && leftPlus!= null)leftPlus.SetActive(false);
        if (kind == "right" && rightPlus!= null)rightPlus.SetActive(false);
            
        
    }
    public void OpenRoom(string direction) {
        if (direction == "up") defs.frontDoor = true;
        if (direction == "left") defs.leftDoor = true;
        if (direction == "right") defs.rightDoor = true;
        if (direction == "down") defs.backDoor = true;
        quarterDefinition.isFirstQuarter = true;
        CheckStatus();
    }
    public void DeclareFigher(Fighter fighter) {
        List<Fighter> enemies = fighter.stats.faction == 0 ? GridOverlord.Instance.attackers : GridOverlord.Instance.defenders;
        enemies.Shuffle();
        
        // look for fight
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].inRoom == this) {
                fighter.InitiateCombat(enemies[i]);
                enemies[i].InitiateCombat(fighter);
                return;
            }            
        }

        // no fight, let's break stuff
        if (fighter.stats.faction ==0) return; // unless we're defending ofc

        for (int i = 0; i < defs.contentIds.Length; i++)
        {
            if (defs.contentIds[i] != -1) {
                DivisionUsage thing = GridOverlord.Instance.gameLib.roomDivisions[defs.contentIds[i]];
                if (thing.effect.Contains("tg")) {
                    fighter.InitiateBreakSomething(sections[i]);
                    return;
                }
            }            
        }
    }
    public bool CanGo(string direction) {
        //                            (  x exists                                                       y exists                                                 ) AND   (either there is a door or the caveRooms have the same parent, meaning they are actually parts of one bigger room)       
        if (direction == "up") return  GridOverlord.Instance.roomGrid.ContainsKey(defs.xPos) && GridOverlord.Instance.roomGrid[defs.xPos].ContainsKey(defs.yPos+1)&& (defs.frontDoor || GridOverlord.Instance.roomGrid[defs.xPos][defs.yPos+1].transform.parent == transform.parent);
        if (direction == "left") return GridOverlord.Instance.roomGrid.ContainsKey(defs.xPos-1) && GridOverlord.Instance.roomGrid[defs.xPos-1].ContainsKey(defs.yPos) && (defs.leftDoor || GridOverlord.Instance.roomGrid[defs.xPos-1][defs.yPos].transform.parent == transform.parent);
        if (direction == "right") return  GridOverlord.Instance.roomGrid.ContainsKey(defs.xPos+1) && GridOverlord.Instance.roomGrid[defs.xPos+1].ContainsKey(defs.yPos)&& (defs.rightDoor || GridOverlord.Instance.roomGrid[defs.xPos+1][defs.yPos].transform.parent == transform.parent);
        if (direction == "down") return GridOverlord.Instance.roomGrid.ContainsKey(defs.xPos) && GridOverlord.Instance.roomGrid[defs.xPos].ContainsKey(defs.yPos-1) && (defs.backDoor || GridOverlord.Instance.roomGrid[defs.xPos][defs.yPos-1].transform.parent == transform.parent);

        Debug.LogError("Direction should up, left, right, or down. You sent "+direction+". \nYou silly goose");
        return false;
    }

    GameObject InstantiatePlus(float x, float y, int createsX, int createsY, string direction) {
        GameObject plus = GameObject.Instantiate(GridOverlord.Instance.gameLib.plusPrefab);
        plus.transform.SetParent(transform);
        plus.transform.localPosition = new Vector2(x, y);
        plus.GetComponent<plusScript>().forCave = this;
        plus.GetComponent<plusScript>().xPos = defs.xPos + createsX;
        plus.GetComponent<plusScript>().yPos = defs.yPos + createsY;
        plus.GetComponent<plusScript>().direction = direction;
        return plus;
    }
}
