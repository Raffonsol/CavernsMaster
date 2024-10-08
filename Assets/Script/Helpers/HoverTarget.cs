using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum TargetType
{
    Currency,
    Life,
    WarriorMenu,
    Stats,
    Cost
}

public class HoverTarget : MonoBehaviour
{
    UIManager uIManager;
    public TargetType[] uITypes;
    public int[] subTypeIds;

    // Start is called before the first frame update
    public void Awake()
    {
        uIManager = GridOverlord.Instance.uIManager;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnMouseOver()
    {
        for( int i = 0; i<uITypes.Length; i++)
        {
            TargetType item =uITypes[i];
            if (item == TargetType.Currency) {
                uIManager.ShowCurrency(subTypeIds[i]);
            }
            if (item == TargetType.WarriorMenu) {
                uIManager.HideStats();
                uIManager.ShowWarriorMenu(subTypeIds[i]);
            }
            if (item == TargetType.Cost) {
                uIManager.ShowCostMenu(subTypeIds[i]);
            }
            if (item == TargetType.Stats) {
                    uIManager.HidewWarriorMenu();
                Fighter me = GetComponent<Fighter>();
                uIManager.ShowStatsMenu(me.stats, me.bodyDown);
            }
        }
    }
 
   void OnMouseExit()
   {
        StartCoroutine(ExecuteAfterTime(1f, () =>
        {        
            for( int i = 0; i<uITypes.Length; i++)
            {
                TargetType item =uITypes[i];
                if (item == TargetType.Currency) {
                    uIManager.HideCurrency();
                }
                if (item == TargetType.WarriorMenu) {
                    uIManager.HidewWarriorMenu();
                }
                if (item == TargetType.Cost) {
                    uIManager.HideCost();
                }
                if (item == TargetType.Stats) {
                    uIManager.HideStats();
            }
            }
        }));
   }

    bool isCoroutineExecuting = false;
    IEnumerator ExecuteAfterTime(float time, Action task)
    {
        if (isCoroutineExecuting)
            yield break;
            
        isCoroutineExecuting = true;

        yield return new WaitForSeconds(time);

        task();

        isCoroutineExecuting = false;
    }
}
