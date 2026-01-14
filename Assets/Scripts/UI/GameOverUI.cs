using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set;}
    public TMP_Text gameOverText;
    public Button mainMenuButton;
    public Button restartButton;

    public GameOverUI ()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void OnRestartButtonPress()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Area1");
    }

    void OnMainMenuButtonPress()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void Start()
    {
        mainMenuButton.onClick.AddListener(OnMainMenuButtonPress);
        restartButton.onClick.AddListener(OnRestartButtonPress);
        EventBus.Instance.Register<EnemyWaveEndEvent>(OnWaveEndHandler);
        EventBus.Instance.Register<GameOverEvent>(OnGameOverHandler);
        Hide();
    }

    private void OnGameOverHandler(GameOverEvent evnt)
    {
        Show(false);
    }

    private void OnWaveEndHandler(EnemyWaveEndEvent evnt)
    {
        if (evnt.gameWon)
        {
            Show(true);
        }
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Show(bool playerWon)
    {
        this.gameObject.SetActive(true);
        if (playerWon)
        {
            gameOverText.text = "Victory!";
        }
        else
        {
            gameOverText.text = "Defeat!";
        }
    }
}
