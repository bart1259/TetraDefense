using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerSelectionUIManager : MonoBehaviour
{
    public GameObject towerButtonPrefab;
    public float spacingY = 50.0f;

    private List<TowerButtonVisuals> _visualButtons = new List<TowerButtonVisuals>();

    void Start()
    {
        List<TowerSO> towers = new List<TowerSO>(Resources.LoadAll<TowerSO>(""));
        towers.Sort((a, b) => a.Price.CompareTo(b.Price));

        for (int i = 0; i < towers.Count; i ++)
        {
            AddTowerButton(towers[i], i);
        }

        EventBus.Instance.Register<CancelPlaceEvent>(OnPiecePlaceCancelHandler);
        EventBus.Instance.Register<TowerPlaceEvent>(OnTowerPlaceHandler);
        EventBus.Instance.Register<TowerButtonHoverEvent>(OnTowerButtonHoverHandler);
    }

    void OnTowerButtonHoverHandler(TowerButtonHoverEvent evnt)
    {
        if(evnt.IsHovering)
        {
            TowerInfoUI.Instance.Visualize(evnt.Tower);
        }
        else
        {
            TowerInfoUI.Instance.Close();
        }
    }

    void OnPiecePlaceCancelHandler(CancelPlaceEvent evnt)
    {
        if(evnt.PlacingTower)
        {
            foreach (TowerButtonVisuals buttonVisual in _visualButtons)
            {
                buttonVisual.Unselect();
            }
        }
    }

    void OnTowerClickHandler(TowerButtonVisuals button)
    {
        foreach (TowerButtonVisuals buttonVisual in _visualButtons)
        {
            buttonVisual.Unselect();
        }
        button.Select();
        EventBus.Instance.Publish<TowerSelectEvent>(new TowerSelectEvent(button.Tower));
    }

    void OnTowerPlaceHandler(TowerPlaceEvent evnt)
    {
        // Idk, if we ever want the player to be able to place multiple towers at once, we can delete this
        foreach (TowerButtonVisuals buttonVisual in _visualButtons)
        {
            if(buttonVisual.Tower == evnt.Tower)
            {
                buttonVisual.Unselect();
                return;
            }
        }
    }
    
    void AddTowerButton(TowerSO tower, int index)
    {
        GameObject towerButton = GameObject.Instantiate(towerButtonPrefab);
        towerButton.transform.SetParent(transform);
        towerButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(
            0,
            -spacingY * index,
            0
        );

        TowerButtonVisuals towerButtonVisuals = towerButton.GetComponent<TowerButtonVisuals>();
        towerButtonVisuals.Visualize(tower);
        _visualButtons.Add(towerButtonVisuals);
        towerButtonVisuals.OnButtonClick += OnTowerClickHandler;

    }

}
