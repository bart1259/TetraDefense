using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WarningUI : MonoBehaviour
{
    public static WarningUI Instance { get; private set;}
    private TMP_Text _textComponent;

    public WarningUI ()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void Start()
    {
        _textComponent = GetComponent<TMP_Text>();
        ClearWarning();
    }

    public void ShowWarning(string message, float expiration = 5.0f)
    {
        _textComponent.text = message;
        CancelInvoke("ClearWarning");
        Invoke("ClearWarning", expiration);
    }

    public void ClearWarning()
    {
        _textComponent.text = "";
    }
}
