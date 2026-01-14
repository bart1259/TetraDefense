using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PlatformCardVisuals : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text PlatformNameLabel;
    public TMP_Text PlatformCostLabel;
    public TMP_Text PlatformRarityLabel;
    public RawImage PlatformVisualizationImage;
    public Color SelectedColor = new Color(1f, 1f, 0.82f);

    public delegate void OnCardClickHandler(PlatformCardVisuals self);
    public event OnCardClickHandler OnCardClick;

    public PlatformSO Platform { get { return _platform; } }
    private PlatformSO _platform;

    public int HoverHeight = 150;
    public int NonHoverHeight = 0;
    private int _targetHeight = 0;
    private RectTransform _rectTransform;
    private int _targetX = 0;

    private bool _hovering = false;
    public bool Hovering { get { return _hovering; } }

    public bool Selected { get { return _selected; } set { _selected = value; GetComponent<Image>().color = _selected ? SelectedColor : Color.white; } }
    private bool _selected;

    public void Start()
    {
        _targetHeight = NonHoverHeight;
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Visualize(PlatformSO platform)
    {
        _platform = platform;

        PlatformNameLabel.text = platform.PlatformName;
        PlatformCostLabel.text = "Cost: " + platform.Price.ToString();
        PlatformRarityLabel.text = "?";

        PieceShape shape = PieceShape.FromString(platform.PlatformShape);

        PlatformVisualizationImage.texture = TextureUtils.CreatePieceTexture(
            shape, 
            platform.PlatformColor,
            Color.clear,
            Color.black,
            5,
            7
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetHeight = HoverHeight;
        _hovering = true;
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetHeight = NonHoverHeight;
        _hovering = false;
        transform.SetAsLastSibling();
    }

    public void Update()
    {
        if (_hovering && Input.GetMouseButtonDown(0))
        {
            if(OnCardClick != null)
                OnCardClick.Invoke(this);
        }

        // Move smoothing
        float currentY = _rectTransform.anchoredPosition.y;
        float nextY = Mathf.Lerp(currentY, _targetHeight, Time.deltaTime * 10f);

        float currentX = _rectTransform.anchoredPosition.x;
        float nextX = Mathf.Lerp(currentX, _targetX, Time.deltaTime * 5f);

        _rectTransform.anchoredPosition = new Vector2(nextX, nextY);
    }

    public void SetTargetX(int targetX)
    {
        _targetX = targetX;
    }
}
