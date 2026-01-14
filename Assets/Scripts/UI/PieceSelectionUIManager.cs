using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PieceSelectionUIManager : MonoBehaviour
{
    public GameObject pieceCardPrefab;
    public Button redrawButton;
    public TMP_Text redrawCostText;
    public int redrawCost = 10;
    public int redrawIncrement = 5;
    public float spacing = 50.0f;
    public float startingY = -300;

    private List<PlatformCardVisuals> _visualCards = new List<PlatformCardVisuals>();
    private bool _needsReordering = true;
    private PlatformCardVisuals _selectedCard = null;
    private bool _waveInProgress = false;
    private Economy _economy;

    void Start()
    {
        _economy = GameStateManager.GetInstance().Economy;

        List<PlatformSO> platforms = new List<PlatformSO>(Resources.LoadAll<PlatformSO>(""));

        DrawNNewCard(5);

        EventBus.Instance.Register<PlatformPlaceEvent>(OnPlatformPlaceHandler);
        EventBus.Instance.Register<CancelPlaceEvent>(OnPiecePlaceCancelHandler);
        EventBus.Instance.Register<EnemyWaveStartEvent>(OnEnemyWaveStart);
        EventBus.Instance.Register<EnemyWaveEndEvent>(OnEnemyWaveEnd);

        redrawButton.onClick.AddListener(OnButtonPress);
        redrawCostText.text = $"Redraw ({redrawCost})";
    }

    void OnButtonPress()
    {
        if (_economy.HasEnoughMoney(redrawCost))
        {
            _economy.SpendMoney(redrawCost);
            redrawCost += redrawIncrement;
            redrawCostText.text = $"Redraw ({redrawCost})";

            foreach (var card in _visualCards)
            {
                card.OnCardClick -= OnCardClickHandler;
                Destroy(card.gameObject);
            }
            _visualCards.Clear();
            _selectedCard = null;

            DrawNNewCard(5);
        }
        else
        {
            WarningUI.Instance.ShowWarning("Not enough money to redraw!");
        }
    }
    
    private void OnPlatformPlaceHandler(PlatformPlaceEvent evnt)
    {
        if(evnt.Platform == _selectedCard.Platform)
            RemovePlatformCard(_selectedCard);
        else
            Debug.LogWarning($"Somehow the placed platform does not match the selected card's platform? ({evnt.Platform} != {_selectedCard.Platform})");
    }

    private void OnEnemyWaveStart(EnemyWaveStartEvent evnt)
    {
        _waveInProgress = true;
        foreach (var cardVisual in _visualCards)
        {
            cardVisual.Selected = false;
        }
    }

    private void OnEnemyWaveEnd(EnemyWaveEndEvent evnt)
    {
        _waveInProgress = false;
    }

    private void OnPiecePlaceCancelHandler(CancelPlaceEvent evnt)
    {
        if(evnt.PlacingPlatform)
            _selectedCard.Selected = false;
    }

    private void OnCardClickHandler(PlatformCardVisuals card)
    {
        if (_waveInProgress)
        {
            WarningUI.Instance.ShowWarning("Cannot place platforms during a wave!");
            return;
        }

        foreach (var cardVisual in _visualCards)
        {
            cardVisual.Selected = false;
        }
        card.Selected = true;
        _selectedCard = card;
        EventBus.Instance.Publish<PlatformCardSelectEvent>(new PlatformCardSelectEvent(card.Platform));
    }


    private void RedistributeCards()
    {
        for (int i = 0; i < _visualCards.Count; i++)
        {
            _visualCards[i].SetTargetX(i * (int)spacing - (_visualCards.Count * (int)spacing / 2));
        }
    }

    public void RemovePlatformCard(PlatformCardVisuals card)
    {
        _selectedCard = null;
        card.OnCardClick -= OnCardClickHandler;
        _visualCards.Remove(card);
        //TODO: If we make visuals, we could animate the removal here
        Destroy(card.gameObject);

        RedistributeCards();

        if (_visualCards.Count == 0)
        {
            DrawNNewCard(5);
        }
    }


    public void AddPlatformCard(PlatformSO platform)
    {
        GameObject newCard = Instantiate(pieceCardPrefab, transform);
        newCard.transform.SetParent(transform);
        newCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, startingY);

        _visualCards.Add(newCard.GetComponent<PlatformCardVisuals>());
        PlatformCardVisuals cardVisuals = newCard.GetComponent<PlatformCardVisuals>();
        cardVisuals.Visualize(platform);
        cardVisuals.OnCardClick += OnCardClickHandler;

        RedistributeCards();
    }

    void DrawNNewCard(int n)
    {
        List<PlatformSO> platforms = new List<PlatformSO>(Resources.LoadAll<PlatformSO>(""));

        for (int i = 0; i < n; i++)
        {
            StartCoroutine(DrawCardWithDelay(platforms, i * 0.2f));
        }
    }

    IEnumerator DrawCardWithDelay(List<PlatformSO> platforms, float delay)
    {
        yield return new WaitForSeconds(delay);
        AddPlatformCard(platforms[Random.Range(0, platforms.Count)]);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            List<PlatformSO> platforms = new List<PlatformSO>(Resources.LoadAll<PlatformSO>(""));
            AddPlatformCard(platforms[Random.Range(0, platforms.Count)]);
        }

        // Card reordering so none get buried
        bool hoveringOverCards = false;
        foreach (var card in _visualCards)
        {
            if (card.Hovering)
            {
                hoveringOverCards = true;
                break;
            }
        }

        if (!hoveringOverCards && _needsReordering)
        {
            // Reorder the cards in their original order
            for (int i = 0; i < _visualCards.Count; i++)
                _visualCards[i].transform.SetAsLastSibling();
            _needsReordering = false;
        } else
        {
            _needsReordering = true;
        }
    }
}
