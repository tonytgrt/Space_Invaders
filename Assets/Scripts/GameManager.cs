using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public int score = 0;
    public int highScore = 0;
    public int lives = 3;
    public int currentWave = 1;
    public int extraLifeThreshold = 1500;

    [Header("References")]
    public Text scoreText;
    public Text livesText;
    public Text waveText;
    public Text gameOverText;
    public Text highScoreText;
    public GameObject gameOverPanel;
    public GameObject waveCompletePanel;
    public GameObject floatingScorePrefab;
    public PlayerController player;
    public AlienFormation alienFormation;

    [Header("Player Respawn")]
    public Vector3 playerStartPosition = new Vector3(0, 0, -5);

    [Header("Audio")]
    public AudioClip playerDeathSound;
    public AudioClip extraLifeSound;

    private bool isGameOver = false;
    private bool isPaused = false;
    private bool extraLifeAwarded = false;

    void Start()
    {
        // Load high score
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        // Find references if not assigned
        if (player == null)
            player = FindObjectOfType<PlayerController>();
        if (alienFormation == null)
            alienFormation = FindObjectOfType<AlienFormation>();

        // Hide panels at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        // Pause handling
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // Restart on game over
        if (isGameOver && Input.GetKeyDown(KeyCode.Return))
        {
            RestartGame();
        }
    }

    public void AddScore(int points)
    {
        score += points;

        // Check for extra life
        if (!extraLifeAwarded && score >= extraLifeThreshold)
        {
            lives++;
            extraLifeAwarded = true;
            if (extraLifeSound != null)
            {
                AudioSource.PlayClipAtPoint(extraLifeSound, Camera.main.transform.position);
            }
        }

        // Update high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        UpdateUI();
    }

    public void ShowFloatingScore(int points, Vector3 position)
    {
        if (floatingScorePrefab != null)
        {
            GameObject floatingScore = Instantiate(floatingScorePrefab, position, Quaternion.identity);
            Text text = floatingScore.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = points.ToString();
            }
            Destroy(floatingScore, 1f);
        }
    }

    public void PlayerHit()
    {
        lives--;

        if (playerDeathSound != null)
        {
            AudioSource.PlayClipAtPoint(playerDeathSound, player.transform.position);
        }

        UpdateUI();

        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            // Respawn player after delay
            StartCoroutine(RespawnPlayer());
        }
    }

    IEnumerator RespawnPlayer()
    {
        // Disable player temporarily
        player.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);

        // Reset player position
        player.transform.position = playerStartPosition;
        player.gameObject.SetActive(true);
    }

    public void WaveComplete()
    {
        currentWave++;
        UpdateUI();

        // Show wave complete panel
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(true);
        }

        // Pause the game while showing panel
        Time.timeScale = 0f;
    }

    // Called by Next Wave button
    public void StartNextWaveButton()
    {
        // Hide panel
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(false);
        }

        // Resume game
        Time.timeScale = 1f;

        // Reset alien formation for next wave
        alienFormation.ResetFormation();
    }

    public void GameOver()
    {
        isGameOver = true;

        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Update game over text with final score
        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER\n\nScore: " + score + "\nHigh Score: " + highScore;
        }

        // Stop the game
        Time.timeScale = 0f;
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score.ToString("D5");
        if (livesText != null)
            livesText.text = "LIVES: " + lives;
        if (waveText != null)
            waveText.text = "WAVE: " + currentWave;
        if (highScoreText != null)
            highScoreText.text = "HI-SCORE: " + highScore.ToString("D5");
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}