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
    public Sprite rangedAttackSprite;

    // sound
    public AudioClip attackSound;
    public AudioClip deathSound;

    // definitions
    [SerializeField]
    public CharAdjustments adjustments;
    public CharStats stats;
    public CaveRoom inRoom;
    public MovementType currentMovementType;
    public AudioSource audioSource;

    // controls
    private Vector2 movePosition;
    private Vector2 targetPosition;
    private bool still = true;
    private bool facingUp = false;
    private bool facingRight = true;
    private GameObject lastQuadrant;

    // pathing
    private List<Coordinates> walked = new List<Coordinates>();

    // combat
    private Fighter inFight;
    private bool combatMode = false;

    // timers
    private float animationTime = 0f;
    private float stepTimer = 0.5f;
    private bool onStep1 = false;
    private float stepDefaultY = 0;

    private float heaveTimer = 0.7f;
    private bool heavingUp = false;

    private float attackCooldown= 0f;
    private float attackingTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        targetPosition = transform.position;
        movePosition = transform.position;
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
        targetPosition = target;
        movePosition = transform.position;
        animationTime=0;
        still = false;
    }

    public void Die() {
        if (stats.faction == 1) {
            int goldReward = UnityEngine.Random.Range(stats.goldDroppedMin,stats.goldDroppedMax);
            GridOverlord.Instance.gameData.AddCurrency(0, goldReward);
        }
        SafelyRemoveChar();
    }
    public void Retreat() {
        // Todo: remove money they stole
        SafelyRemoveChar();
    }


    public void InitiateCombat(Fighter enemy) {
        if (combatMode) return;
        inFight = enemy;
        combatMode = true;
        attackCooldown = stats.attackCooldown;
        movePosition = transform.position;
        animationTime=0;
        enemy.InitiateCombat(this);
    }
    public void TakeDamage(float damage) {
        float sufferedDamage = damage - UnityEngine.Random.Range(0, stats.armor);
        stats.lifeCurrent -= sufferedDamage;
        if (stats.lifeCurrent <= 0) {
            GameObject death = Instantiate(GridOverlord.Instance.gameLib.deathPrefab, transform.position, transform.rotation);
            death.transform.position = transform.position;
            death.transform.parent = null;
            death.GetComponent<AudioSource>().clip = deathSound;
            death.GetComponent<AudioSource>().Play();
            Die();
        } else {
            GameObject aoe = Instantiate(GridOverlord.Instance.gameLib.hitPrefab, transform.position, transform.rotation);
            AnimationP anim = aoe.GetComponent<AnimationP>();
            if (anim != null) {
                anim.stickTarget = gameObject;
                anim.sticky = true;
            }
        }
    }
    
    
    // Update is called once per frame
    void FixedUpdate()
    {
        FixSortingOrder();
        if (combatMode) {
            Fight();
            return;
        }
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
        animationTime += stats.moveSpeed * Time.deltaTime*0.1f;
        transform.position = ( Vector3.Lerp (movePosition, targetPosition, animationTime));

        Flip(targetPosition);
    }
    void Step() {
        if (stepTimer > 0) {
            stepTimer -= Time.deltaTime;
            
            if (adjustments.bodyChangeOnMove) {
                body.GetComponent<SpriteRenderer>().sprite = facingUp ? bodyUp : bodyDown;
            } else {
                Transform legTransform = onStep1 ? leg1.transform : leg2.transform;
                Vector2 pos = legTransform.localPosition;
                pos.y += adjustments.feetAnimFactor*Time.deltaTime*(facingUp ? 1 : -1);
                legTransform.localPosition = pos;
            }
        } else {
            if (adjustments.bodyChangeOnMove) {
                body.GetComponent<SpriteRenderer>().sprite = facingUp ? foot1 : foot2;
            } else {
                Transform legTransform = onStep1 ? leg1.transform : leg2.transform;
                Vector2 pos = legTransform.localPosition;
                pos.y = stepDefaultY;
                legTransform.localPosition = pos;
            }
            stepTimer = adjustments.stepDuration;
            onStep1 = !onStep1;
        }

    }
    void StopStepping() {
        if (adjustments.bodyChangeOnMove) {
            body.GetComponent<SpriteRenderer>().sprite = facingUp ? bodyUp : bodyDown;
        } else {
            Vector2 pos1 = leg1.transform.localPosition;
            Vector2 pos2 = leg2.transform.localPosition;
            pos1.y = stepDefaultY;
            pos2.y = stepDefaultY;
            leg1.transform.localPosition = pos1;
            leg2.transform.localPosition = pos2;

        }
    }
    void Heave() {
        if (adjustments.noHeave) return;
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
    void Fight() {
        if (inFight == null || inFight.gameObject == null) {
            combatMode = false;
            inRoom.DeclareFigher(this);
            FixLooks();
            return;
        }
        if (Vector3.Distance(transform.position, inFight.gameObject.transform.position) < stats.range) {
            
            Heave();
            StopStepping();

            if (attackingTimer > 0) { // restore normal state after attackingTimer is down to 0
                attackingTimer-=Time.deltaTime;
                if (attackingTimer <=0){
                    body.GetComponent<SpriteRenderer>().sprite = facingUp? bodyUp: bodyDown;
                    inFight.TakeDamage(stats.attackDamage);
                }
            }
            else if (attackCooldown > 0) {
                attackCooldown -= Time.deltaTime;
            } else {
                attackCooldown = stats.attackCooldown;
                attackingTimer = stats.attackDamageDelay;
                
                // anims and sounds
                body.GetComponent<SpriteRenderer>().sprite = facingUp? attackUp: attackDown;
                audioSource.clip = attackSound;
                audioSource.Play();

                // ranged created arrow
                if (rangedAttackSprite != null) {
                    GameObject arrow = Instantiate(GridOverlord.Instance.gameLib.projectilePrefab,transform.position, transform.rotation);
                    arrow.GetComponent<SpriteRenderer>().sprite = rangedAttackSprite;
                    arrow.GetComponent<Projectile>().target = inFight.gameObject.transform.position;
                    arrow.GetComponent<Projectile>().Go();
                }
            }

        } else {
            Step();

            Vector2 currentPosition = transform.position;
            animationTime += stats.moveSpeed * Time.deltaTime*0.15f;
            transform.position = ( Vector3.Lerp (movePosition, inFight.gameObject.transform.position, animationTime));

            Flip(inFight.gameObject.transform.position);
        }
        
    }
    void Flip(Vector2 targetPosition) {
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
            if (!facingRight) {
                if (targetPosition.x > transform.position.x) {
                    facingRight = true;
                    body.GetComponent<SpriteRenderer>().flipX = false;
                }
            } else {
                if (targetPosition.x < transform.position.x) {
                    facingRight = false;
                    body.GetComponent<SpriteRenderer>().flipX = true;
                }
            }
    }
    void SafelyRemoveChar() {
        if (stats.faction == 0) {
            GridOverlord.Instance.defenders.RemoveAll(x => x.fighterId == fighterId);
        } else {
            GridOverlord.Instance.attackers.RemoveAll(x => x.fighterId == fighterId);
        }
        // TODO: Death animation
        Destroy(gameObject);
    }
    void FixSortingOrder() {

        body.GetComponent<SpriteRenderer>().sortingOrder = (int)(-10*transform.position.y);
        leg1.GetComponent<SpriteRenderer>().sortingOrder = (int)(-10*transform.position.y);
        leg2.GetComponent<SpriteRenderer>().sortingOrder = (int)(-10*transform.position.y);
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
