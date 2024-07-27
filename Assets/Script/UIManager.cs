using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject currencyPanel;
    public GameLib lib;
    public GameObject warriorPanels;
    public GameObject nextButton;
    // Start is called before the first frame update
    void Start()
    {
        lib = GridOverlord.Instance.gameLib;
            nextButton.transform.Find("Panel").gameObject.GetComponent<Button>().onClick.AddListener(() => {
            GridOverlord.Instance.StartNextRaid();
            nextButton.transform.Find("Panel").gameObject.SetActive(false);
        });
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
        Button btn = warriorPanels.transform.Find("Panel/0").gameObject.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => GridOverlord.Instance.CreateMob(0));
        // warriorPanels.transform.Find("Panel/Image").GetComponent<Image>().sprite = currency.icon;
        // warriorPanels.transform.Find("Panel/Description").GetComponent<TextMeshProUGUI>().text = currency.name;
        // warriorPanels.transform.Find("Panel/Value").GetComponent<TextMeshProUGUI>().text = GridOverlord.Instance.gameData.currencyAmounts[currencyIndex].ToString();
    } 
    public void HidewWarriorMenu()
    {
        warriorPanels.transform.Find("Panel").gameObject.SetActive(false);
    } 



    // Update is called once per frame
    void Update()
    {
        
    }
}
