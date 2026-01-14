using UnityEngine;
using TMPro;

public class EconomyUI : MonoBehaviour
{
    public TMP_Text MoneyText;
    private Economy _economy;

    private int _displayAmount = 0;

    void Start()
    {
        _economy = GameStateManager.GetInstance().Economy;
    }


    // Update is called once per frame
    void Update()
    {
        int actualAmount = _economy.Money;
        if (_displayAmount != actualAmount)
        {
            if (_displayAmount > actualAmount)
                _displayAmount--;
            else
                _displayAmount++;
        }
        MoneyText.text = "Money: " + _displayAmount.ToString();
    }
}
