using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Fighter : MonoBehaviour
{
    public int fighterId;

    // game objects -- parts
    public GameObject body;
    public GameObject leg1;
    public GameObject leg2;

    // animations
    public Sprite foot1;
    public Sprite foot2;
    public Sprite bodyDown;
    public Sprite bodyUp;
    public Sprite attackDown;
    public Sprite attackUp;

    // definitions
    [SerializeField]
    public CharAdjustments adjustments;
    public CharStats stats;
    public CaveRoom inRoom;
    public MovementType currentMovementType;

    // controls
    private Vector2 targetPosition;
    private bool still = true;
    private bool facingUp = false;
    private GameObject lastQuadrant;

    // pathing
    private List<Coordinates> walked = new List<Coordinates>();

    // combat
    private Fighter inFight;

    // timers
    private float stepTimer = 0.5f;
    private bool onStep1 = false;
    private float stepDefaultY = 0;

    private float heaveTimer = 0.7f;
    private bool heavingUp = false;

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position;
        if (stats.faction == 1 ){
            walked.Add(new Coordinates(){x=0, y=0,});
        }
    }

    public void CheckStatus() {
        // ONLY CALL THIS ONCE!!!
        if (stats.faction == 0) {
            gameObject.transform.tag = ("Defender");
            GridOverlord.Instance.defenders.Add(this);
        } else {
            gameObject.transform.tag = ("Attacker");
            GridOverlord.Instance.attackers.Add(this);
        }
        FixLooks();
        stepDefaultY += leg1.transform.localPosition.y;
        fighterId = GridOverlord.Instance.gameData.lastCharId;
        GridOverlord.Instance.gameData.lastCharId++;
    }

    public void SetMoveTarget(Vector2 target) {
        still = false;
        targetPosition = target;
    }

    public void Die() {
        SafelyRemoveChar();
        // Todo: add reward money
    }
    public void Retreat() {
        SafelyRemoveChar();
        // Todo: remove money they stole
    }
    
    
    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
        RunAi();
    }

    void Move() {
        if (still) {
            Heave();
            return;
        }
        if (Vector3.Distance(transform.position, targetPosition) < 1f) {
            still = true;
            StopStepping();
            return;
        }
        still = false;
        Step();

        Vector2 currentPosition = transform.position;
        transform.position = ( Vector3.Lerp (currentPosition, targetPosition, stats.moveSpeed * Time.deltaTime*0.2f));

        //flip
        if (!facingUp) {
            if (targetPosition.y > transform.position.y) {
                facingUp = true;
                body.GetComponent<SpriteRenderer>().sprite = bodyUp;
            }
        } else {
            if (targetPosition.y < transform.position.y) {
                facingUp = false;
                body.GetComponent<SpriteRenderer>().sprite = bodyDown;
            }
        }
    }
    void Step() {
        if (stepTimer > 0) {
            stepTimer -= Time.deltaTime;
            
            Transform legTransform = onStep1 ? leg1.transform : leg2.transform;
            Vector2 pos = legTransform.localPosition;
            pos.y += 1f*Time.deltaTime*(facingUp ? 1 : -1);
            legTransform.localPosition = pos;

        } else {
            Transform legTransform = onStep1 ? leg1.transform : leg2.transform;
            Vector2 pos = legTransform.localPosition;
            pos.y = stepDefaultY;
            legTransform.localPosition = pos;

            stepTimer = 0.5f;
            onStep1 = !onStep1;
        }

    }
    void StopStepping() {
        Vector2 pos1 = leg1.transform.localPosition;
        Vector2 pos2 = leg2.transform.localPosition;
        pos1.y = stepDefaultY;
        pos2.y = stepDefaultY;
        leg1.transform.localPosition = pos1;
        leg2.transform.localPosition = pos2;
    }
    void Heave() {
        if (heaveTimer > 0) {
            heaveTimer -= Time.deltaTime;

            Vector2 pos = body.transform.localPosition;
            pos.y += 0.01f*(heavingUp ? 1 : -1)*adjustments.heaveFactor;
            body.transform.localPosition = pos;

        } else {
            heaveTimer = 0.7f;
            heavingUp = !heavingUp;
        }
    }
    void FixLooks() {
        leg1.transform.localPosition = new Vector2(adjustments.feetX*-1, adjustments.feetY);
        leg2.transform.localPosition = new Vector2(adjustments.feetX, adjustments.feetY);
        
        leg1.GetComponent<SpriteRenderer>().sprite = foot1;
        leg2.GetComponent<SpriteRenderer>().sprite = foot2;
        body.GetComponent<SpriteRenderer>().sprite = bodyDown;
    }
    void RunAi() {
        if (stats.faction == 0 ){
            return; // for now defenders just don't do anything
        }

        if (still) {
            // just standing, so let's find what to do next.
            Coordinates nextRoom = new Coordinates(){x=inRoom.defs.xPos, y=inRoom.defs.yPos};
            
            if (currentMovementType == MovementType.Advancing){
                inRoom.forwardCoo.Shuffle();

                if (inRoom.forwardCoo.Count == 0){
                    currentMovementType = MovementType.Retreating;
                    return;
                }
                nextRoom = inRoom.forwardCoo[0];
            } else if (currentMovementType == MovementType.Retreating) {
                if (inRoom.backwardsCoo.Count == 0){
                    Retreat();
                    return;
                }
                nextRoom = inRoom.backwardsCoo[0];
            }

            SetMoveTarget(GridOverlord.Instance.roomGrid[nextRoom.x][nextRoom.y].gameObject.transform.position+ new Vector3(UnityEngine.Random.Range(-2f,2f),UnityEngine.Random.Range(-2f,2f),0));
                
        }

    }
    void SafelyRemoveChar() {
        if (stats.faction == 0) {
            GridOverlord.Instance.defenders.Remove(this);
        } else {
            GridOverlord.Instance.attackers.Remove(this);
        }
        // TODO: Death animation
        Destroy(gameObject);
    }
    void OnTriggerEnter2D(Collider2D col)
    { 
        try {
            if(gameObject.GetComponent<Draggable>().dragging) {
                return;
            }
        } catch {}
        if (col.gameObject.tag == "Quadrant")
        {
            lastQuadrant = col.gameObject;
            inRoom = lastQuadrant.transform.parent.GetComponent<CaveRoom>();
            inRoom.DeclareFigher(this);
        }
    }

}
