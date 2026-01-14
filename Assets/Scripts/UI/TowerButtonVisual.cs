using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TowerButtonVisuals : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text TowerNameLabel;
    public TMP_Text TowerCostLabel;
    public RawImage TowerVisualizationImage;

    public TowerSO Tower { get { return _tower; } }
    private TowerSO _tower;

    private bool _hovering;

    public Color hoverColor;
    public Color defaultColor;
    public Color selectedColor;
    public bool _selected = false;

    private Image _panel;

    public delegate void OnButtonClickHandler(TowerButtonVisuals self);
    public OnButtonClickHandler OnButtonClick;


    public void Start()
    {
        _panel = GetComponent<Image>();
        _panel.color = defaultColor;
    }

    public void Visualize(TowerSO tower)
    {
        _tower = tower;

        TowerNameLabel.text = _tower.TowerName;
        TowerCostLabel.text = "Cost: " + _tower.Price.ToString();

        PieceShape shape = PieceShape.FromString(_tower.TowerShape);

        TowerVisualizationImage.texture = TextureUtils.CreatePieceTexture(
            shape, 
            Color.red,
            Color.clear,
            Color.black,
            5,
            5
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovering = true;
        if (!_selected)
            _panel.color = hoverColor;
        EventBus.Instance.Publish<TowerButtonHoverEvent>(new TowerButtonHoverEvent(_tower, true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovering = false;
        if(!_selected)
            _panel.color = defaultColor;
        EventBus.Instance.Publish<TowerButtonHoverEvent>(new TowerButtonHoverEvent(_tower, false));
    }

    public void Select()
    {
        _selected = true;
        _panel.color = selectedColor;
    }

    public void Unselect()
    {
        _selected = false;
        _panel.color = defaultColor;
    }

    public void Update()
    {
        if (_hovering && Input.GetMouseButtonDown(0))
        {
            if(OnButtonClick != null)
                OnButtonClick.Invoke(this);
        }
    }

}
