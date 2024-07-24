using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject currencyPanel;
    public GameLib lib;
    // Start is called before the first frame update
    void Start()
    {
        lib = GridOverlord.Instance.gameLib;
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
