using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [HideInInspector]
    public GameLib lib;
    public GameObject currencyPanel;
    public GameObject costPanel;
    public GameObject warriorPanels;
    public GameObject nextButton;
    public GameObject statPanel;
    public GameObject mysteryItemPanel;

    private List<Sale> onGoingSale;

    private bool showingWarriors = false;
    private MysteryContent showingMystery;
    
    void Start()
    {
        lib = GridOverlord.Instance.gameLib;
            nextButton.transform.Find("Panel").gameObject.GetComponent<Button>().onClick.AddListener(() => {
                GridOverlord.Instance.StartNextRaid();
                nextButton.transform.Find("Panel").gameObject.SetActive(false);
        });
    }

    void Update() {
        ListenForClicks();
    }

    public void ShowCurrency(int currencyIndex)
    {
        Currency currency = lib.currencies[currencyIndex];
        currencyPanel.transform.Find("Panel").gameObject.SetActive(true);
        currencyPanel.transform.Find("Panel/Image").GetComponent<Image>().sprite = currency.icon;
        currencyPanel.transform.Find("Panel/Description").GetComponent<TextMeshProUGUI>().text = currency.name;
        currencyPanel.transform.Find("Panel/Value").GetComponent<TextMeshProUGUI>().text = GridOverlord.Instance.gameData.currencyAmounts[currencyIndex].ToString();
    } 
    public void HideCurrency()
    {
        currencyPanel.transform.Find("Panel").gameObject.SetActive(false);
    } 
    public void ShowWarriorMenu(int saleIndex)
    {
        warriorPanels.transform.Find("Panel").gameObject.SetActive(true);
        showingWarriors = true;
        onGoingSale = new List<Sale>();
        for (int i = 0; i < GridOverlord.Instance.gameLib.sales.Length; i++)
        {
            Sale sale = GridOverlord.Instance.gameLib.sales[i];
            if (sale.id == saleIndex) {
                onGoingSale.Add(sale);

                UniqueChar monster = GridOverlord.Instance.gameLib.monsters[sale.saleItemIndex];
                warriorPanels.transform.Find("Panel/"+(onGoingSale.Count-1).ToString()+"/Image").GetComponent<Image>().sprite = monster.bodyDown;
                warriorPanels.transform.Find("Panel/"+(onGoingSale.Count-1).ToString()+"/Description").GetComponent<TextMeshProUGUI>().text = monster.stats.name;
                warriorPanels.transform.Find("Panel/"+(onGoingSale.Count-1).ToString()+"/Value").GetComponent<TextMeshProUGUI>().text = "["+(onGoingSale.Count).ToString()+"] to assign";
            }
        }
        if (onGoingSale.Count == 1) {
            warriorPanels.transform.Find("Panel/1/Image").GetComponent<Image>().sprite = null;
            warriorPanels.transform.Find("Panel/1/Description").GetComponent<TextMeshProUGUI>().text = "";
            warriorPanels.transform.Find("Panel/1/Value").GetComponent<TextMeshProUGUI>().text = "";
        }
        // Button btn = warriorPanels.transform.Find("Panel/0").gameObject.GetComponent<Button>();
        // btn.onClick.RemoveAllListeners();
        // btn.onClick.AddListener(() => GridOverlord.Instance.CreateMob(0));
    } 
    public void HidewWarriorMenu()
    {
        warriorPanels.transform.Find("Panel").gameObject.SetActive(false);
        showingWarriors = false;
    } 
     public void ShowCostMenu(int purchaseableIndex)
    {
        Sale sale = lib.sales[purchaseableIndex];
        Currency currency = lib.currencies[sale.currency];
        costPanel.transform.Find("Panel").gameObject.SetActive(true);
        currencyPanel.transform.Find("Panel/Image").GetComponent<Image>().sprite = currency.icon;
        currencyPanel.transform.Find("Panel/Description").GetComponent<TextMeshProUGUI>().text = sale.name;
        currencyPanel.transform.Find("Panel/Value").GetComponent<TextMeshProUGUI>().text = sale.cost.ToString();
    } 
    public void HideCost()
    {
        costPanel.transform.Find("Panel").gameObject.SetActive(false);
    } 
     public void ShowNextRaidButton()
    {
        nextButton.transform.Find("Panel").gameObject.SetActive(true);
    } 
    public void ShowStatsMenu(CharStats stats, Sprite icon)
    {
        statPanel.transform.Find("Panel").gameObject.SetActive(true);
        statPanel.transform.Find("Panel/Image").GetComponent<Image>().sprite = icon;
        statPanel.transform.Find("Panel/Level").GetComponent<TextMeshProUGUI>().text = stats.level == 0 ? "" : "Level "+ stats.level.ToString();
        statPanel.transform.Find("Panel/Name").GetComponent<TextMeshProUGUI>().text = stats.name;
        statPanel.transform.Find("Panel/LifeValue").GetComponent<TextMeshProUGUI>().text = stats.lifeCurrent +"/"+stats.lifeMax;
    } 
    public void HideStats()
    {
        statPanel.transform.Find("Panel").gameObject.SetActive(false);
    } 
    public void ShowMysteryItem(MysteryContent mysteryContent)
    {
        showingMystery = mysteryContent;
        mysteryItemPanel.transform.Find("Panel").gameObject.SetActive(true);
        for (int i = 0; i < showingMystery.itemDefs.mysteryOptions.Length; i++)
        {
            MysteryOption mysteryOption = showingMystery.itemDefs.mysteryOptions[i];

            mysteryItemPanel.transform.Find("Panel/"+(i).ToString()+"/Image").GetComponent<Image>().sprite = mysteryOption.icon;
            mysteryItemPanel.transform.Find("Panel/"+(i).ToString()+"/Name").GetComponent<TextMeshProUGUI>().text = mysteryOption.name;
            mysteryItemPanel.transform.Find("Panel/"+(i).ToString()+"/Description").GetComponent<TextMeshProUGUI>().text = mysteryOption.description;
            mysteryItemPanel.transform.Find("Panel/"+(i).ToString()+"/Value").GetComponent<TextMeshProUGUI>().text = "["+(i+1).ToString()+"]";
            
        }
    } 
    public void HideMysteryItem()
    {
        showingMystery = null;
        mysteryItemPanel.transform.Find("Panel").gameObject.SetActive(false);
    } 

    void ListenForClicks() {
        if(showingWarriors) {
            

        
            if (Input.GetKeyUp("1") && GridOverlord.Instance.gameData.CheckIfCanAfford(onGoingSale[0].currency, onGoingSale[0].cost)) {
                GridOverlord.Instance.gameData.SpendCurrency(onGoingSale[0].currency, onGoingSale[0].cost);
                GridOverlord.Instance.CreateMob(onGoingSale[0].saleItemIndex);
            }
            if (onGoingSale.Count > 1 && Input.GetKeyUp("2") && GridOverlord.Instance.gameData.CheckIfCanAfford(onGoingSale[1].currency, onGoingSale[1].cost)) {
                GridOverlord.Instance.gameData.SpendCurrency(onGoingSale[1].currency, onGoingSale[1].cost);
                GridOverlord.Instance.CreateMob(onGoingSale[1].saleItemIndex);
            }

        } else if (showingMystery != null) {

            if (Input.GetKeyUp("1") ) {
                showingMystery.BecomeItem(0);
            }
            if (Input.GetKeyUp("2") ) {
                showingMystery.BecomeItem(1);
            }

        }

    }
}
