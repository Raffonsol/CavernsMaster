using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryContent : MonoBehaviour
{
    public MysteryItem itemDefs;
    public CaveRoom inRoom;
    public int inQuadrant;

    private UIManager uIManager;
    // Start is called before the first frame update
    public void ForcedStart()
    {
        uIManager = GridOverlord.Instance.uIManager;
        gameObject.GetComponent<SpriteRenderer>().sprite = itemDefs.sprite;
    }

    public void BecomeItem(int optionIndex) {
        MysteryOption becomeThis = itemDefs.mysteryOptions[optionIndex];

        if (becomeThis.type == MysteryOptionType.Division) {
            inRoom.defs.contentIds[inQuadrant] = becomeThis.optionItemIndex;
            inRoom.sections[inQuadrant] = null;
            inRoom.CheckStatus();
            uIManager.HideMysteryItem();
            Destroy(gameObject); // TODO: move this outside the if statement once traps are setup
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseOver()
    {
        
        uIManager.ShowMysteryItem(this);
         
    }
 
   void OnMouseExit()
   {

        uIManager.HideMysteryItem();

   }
}
