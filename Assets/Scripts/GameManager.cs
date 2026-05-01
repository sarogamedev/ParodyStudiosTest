using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public TMP_Text timerText;
    public TMP_Text feedBackText;
    
    [Header("Game Settings")]
    public float timeLimit = 120f;
    private float timeRemaining;
    private int totalCubes;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        timeRemaining = timeLimit;
        totalCubes = GameObject.FindGameObjectsWithTag("Collectible").Length;
        if (feedBackText != null) feedBackText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isGameOver) return;

        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        if (timeRemaining <= 0)
        {
            TriggerGameOver(false);
        }
    }

    public void CollectCube()
    {
        totalCubes--;
        if (totalCubes <= 0)
        {
            TriggerGameOver(true);
        }
    }

    public void TriggerGameOver(bool win)
    {
        isGameOver = true;
        if (feedBackText != null)
        {
            feedBackText.gameObject.SetActive(true);
            feedBackText.text = win ? "You Win!" : "Game Over!";
        }
        Time.timeScale = 0f; // Pause the game
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}