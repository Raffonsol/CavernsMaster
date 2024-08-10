using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TargetDefinitions
{
    public float moveSpeed;//0 for static
    public float satisfaction;//100 makes attackers go back
    public bool attacker;
}
public class Target : MonoBehaviour
{
    [SerializeField]
    public TargetDefinitions tarDef;
    public DivisionUsage divi;

    public CaveRoom inRoom;
    public int inQuadrant;

    private float life;
    
    // Start is called before the first frame update
    public void ForcedStart()
    {
        life = divi.life;
    }

    public void TakeDamage(float damage) {
        life-=damage;
        if (life <= 0) {
            SafelyDie();
        }
    }

    void SafelyDie() {
        inRoom.defs.contentIds[inQuadrant] = -1;
        inRoom.sections[inQuadrant] = null;
        if (divi.effect.Contains("gp")) {
            GridOverlord.Instance.popGs.RemoveAll(x => x.id == gameObject.GetComponent<PopGrower>().id);
        }
        if (divi.effect.Contains("ff")) {
            // GAMEOVER!!!
            Application.Quit();
        }
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
