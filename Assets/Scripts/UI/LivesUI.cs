using UnityEngine;
using TMPro;

public class LivesUI : MonoBehaviour
{
    public TMP_Text LivesText;
    private GameStateManager _gameStateManager;


    void Start()
    {
        _gameStateManager = GameStateManager.GetInstance();
    }


    // Update is called once per frame
    void Update()
    {
        int lives = _gameStateManager.PlayerLives;
        LivesText.text = "Lives: " + lives.ToString();
    }
}
