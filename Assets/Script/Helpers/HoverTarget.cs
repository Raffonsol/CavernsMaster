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
}

public class HoverTarget : MonoBehaviour
{
    UIManager uIManager;
    public TargetType[] uITypes;
    public int[] subTypeIds;

    // Start is called before the first frame update
    public void ForcedStart()
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
                uIManager.ShowWarriorMenu(subTypeIds[i]);
            }
        }
    }
 
   void OnMouseExit()
   {
        StartCoroutine(ExecuteAfterTime(4f, () =>
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
