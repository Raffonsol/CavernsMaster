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
    public List<DivisionUsage> sections;

    // definitions
    [SerializeField]
    public RoomDefinition defs;

    // Start is called before the first frame update
    void Awake()
    {
        sections = new List<DivisionUsage>(new DivisionUsage[defs.size]);
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
            }else {
                entrance.SetActive(false);
                
                if (defs.backDoor ) {
                    if(backPlus!= null)backPlus.SetActive(false);
                }else {

                    if ((GridOverlord.Instance.roomGrid.ContainsKey(defs.xPos) && GridOverlord.Instance.roomGrid[defs.xPos].ContainsKey(defs.yPos - 1) )) {
                        GridOverlord.Instance.roomGrid[defs.xPos][defs.yPos - 1].GetComponent<CaveRoom>().DisablePlus("front");;
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
                sections[i] = divi;
                //placing
                GameObject contenter = GameObject.Instantiate(divi.gameObject);
                float[] coordinates = Util.GetPositionPerType(defs.type, i);
                Vector2 divisionPos = defs.type == RoomType.BirthRoom 
                 ? new Vector2(0,-12f) //birth room doesnt worry about divisions stuff
                 : quadrants[i].transform.position;
                // -------------------------- modifier for room type ------ quadrant pos --- modifier from this specific division ----------------- //
                contenter.transform.position = new Vector2(coordinates[0] + divisionPos.x + divi.shift.x, coordinates[1] + divisionPos.y+ divi.shift.y);
                //adding scripts to it
                bool needsCollider = false;bool hasHoverTarget = false;
                if (divi.effect.Contains("cs")){ // hoverable to show a currency
                    HoverTarget hovTar = contenter.AddComponent<HoverTarget>();
                    hovTar.uITypes = new TargetType[]{TargetType.Life, TargetType.Currency};
                    
                    hovTar.subTypeIds = new int[]{0, Int32.Parse(divi.effect.Substring(divi.effect.IndexOf("cs")+2,2))}; // second index for currency type
                    hovTar.ForcedStart();
                    needsCollider=true;hasHoverTarget=true;
                }
                if (divi.effect.Contains("tg")){ // targetable
                    Target tar = contenter.AddComponent<Target>();
                    tar.tarDef = new TargetDefinitions();
                    tar.tarDef.attacker = false;
                    tar.tarDef.moveSpeed = 0;
                    tar.tarDef.maxLife=divi.life;
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
                    hovTar.ForcedStart();
                    needsCollider=true;
                }
                Draggable drag = contenter.AddComponent<Draggable>();
                drag.fixedPos = divi.effect.Contains("fp");
                drag.inRoom = this;
                drag.inQuadrant = i;
                drag.diviShift = divi.shift;
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
