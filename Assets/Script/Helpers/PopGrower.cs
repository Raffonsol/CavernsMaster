using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopGrower : MonoBehaviour
{
    public int id;
    public int popCurrencyIndex;
    public int popGrowthType;

    private int popCount;

    // runs when a raid is finished
    public void RunGrowth() {
        
        if (popGrowthType == 2) return; // it's on death so nothing happens here

        popCount = GridOverlord.Instance.gameData.currencyAmounts[popCurrencyIndex];

        int add = 1;
        if (popGrowthType == 0) { // add based on existing population
            add = (int)popCount/3; if (add ==0 && popCount >1) add=1;
            
        } 
        //else if (popGrowthType == 1) { // this just makes it one so....

        
        popCount+=add;
        GridOverlord.Instance.gameData.currencyAmounts[popCurrencyIndex] = popCount;
    }

    // Start is called before the first frame update
    void Start()
    {
        id = GridOverlord.Instance.gameData.lastPopGId;
        GridOverlord.Instance.gameData.lastPopGId++;
        GridOverlord.Instance.popGs.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
