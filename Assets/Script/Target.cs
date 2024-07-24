using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TargetDefinitions
{
    public float maxLife;
    public float moveSpeed;//0 for static
    public float satisfaction;//100 makes attackers go back
    public bool attacker;
}
public class Target : MonoBehaviour
{
    [SerializeField]
    public TargetDefinitions tarDef;
    // Start is called before the first frame update
    public void ForcedStart()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
