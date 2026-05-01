using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the core game loop, including timers, collectible tracking, and win/loss states.
/// Implements a basic Singleton pattern for easy global access.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance;

    [Header("UI References")]
    [Tooltip("Text element displaying the remaining time.")]
    public TMP_Text timerText;
    [Tooltip("Text element for win/loss feedback messages.")]
    public TMP_Text feedBackText;
    
    [Header("Game Settings")]
    [Tooltip("Total time limit for the level in seconds.")]
    public float timeLimit = 120f;
    
    private float timeRemaining;
    private int totalCubes;
    private bool isGameOver = false;

    private void Awake()
    {
        // Enforce singleton pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Initialize timer and find all collectibles
        timeRemaining = timeLimit;
        totalCubes = GameObject.FindGameObjectsWithTag("Collectible").Length;
        
        // Hide feedback text initially
        if (feedBackText != null) feedBackText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isGameOver) return;

        // Tick down the timer
        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        // Check for time-out loss condition
        if (timeRemaining <= 0)
        {
            TriggerGameOver(false);
        }
    }

    /// <summary>
    /// Called when the player collects a cube. Decrements the required cube count.
    /// Triggers a win condition if all cubes are collected.
    /// </summary>
    public void CollectCube()
    {
        totalCubes--;
        if (totalCubes <= 0)
        {
            TriggerGameOver(true);
        }
    }

    /// <summary>
    /// Ends the game, displaying the appropriate UI and pausing time.
    /// </summary>
    /// <param name="win">True if the player won, false if they lost.</param>
    public void TriggerGameOver(bool win)
    {
        isGameOver = true;
        if (feedBackText != null)
        {
            feedBackText.gameObject.SetActive(true);
            feedBackText.text = win ? "You Win!" : "Game Over!";
        }
        
        // Pause the game mechanics
        Time.timeScale = 0f; 
    }

    /// <summary>
    /// Updates the on-screen timer text in an MM:SS format.
    /// </summary>
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
    
    /// <summary>
    /// Reloads the current scene to restart the game.
    /// </summary>
    public void RestartGame()
    {
        // Ensure timescale is reset before loading
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}