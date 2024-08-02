using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject currencyPanel;
    public GameObject costPanel;
    public GameLib lib;
    public GameObject warriorPanels;
    public GameObject nextButton;

    private bool showingWarriors = false;
    
    void Start()
    {
        lib = GridOverlord.Instance.gameLib;
            nextButton.transform.Find("Panel").gameObject.GetComponent<Button>().onClick.AddListener(() => {
                GridOverlord.Instance.StartNextRaid();
                nextButton.transform.Find("Panel").gameObject.SetActive(false);
        });
    }

    void Update() {
        if (Input.GetKeyUp("1") && showingWarriors && GridOverlord.Instance.gameData.CheckIfCanAfford(1, 1)) {
            GridOverlord.Instance.gameData.SpendCurrency(1, 1);
            GridOverlord.Instance.CreateMob(0);
        }
        if (Input.GetKeyUp("2") && showingWarriors && GridOverlord.Instance.gameData.CheckIfCanAfford(1, 1)) {
            GridOverlord.Instance.gameData.SpendCurrency(1, 1);
            GridOverlord.Instance.CreateMob(1);
        }
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
    public void ShowWarriorMenu(int warriorMenuIndex)
    {
        warriorPanels.transform.Find("Panel").gameObject.SetActive(true);
        showingWarriors = true;
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
}
