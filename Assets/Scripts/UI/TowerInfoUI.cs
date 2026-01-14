using UnityEngine;
using TMPro;

public class TowerInfoUI : MonoBehaviour
{
    public TMP_Text TowerNameText;
    public TMP_Text TowerDescriptionText;
    public TMP_Text TowerCostText;
    public TMP_Text TowerRangeText;
    public TMP_Text TowerFireRateText;
    public TMP_Text TowerDamageText;

    public static TowerInfoUI Instance;

    public TowerInfoUI()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        Close();
    }

    public void Visualize(TowerSO tower)
    {
        this.gameObject.SetActive(true);
        TowerNameText.text = tower.TowerName;
        TowerDescriptionText.text = tower.TowerDescription;
        TowerCostText.text = "Cost: " + tower.Price.ToString();
        TowerRangeText.text = "Range: " + tower.Range.ToString("F1");
        TowerFireRateText.text = "Fire Rate: " + tower.FireTimer.ToString("F2") + " s";
        TowerDamageText.text = "Damage: " + tower.BulletDamage.ToString();
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }
    
}
