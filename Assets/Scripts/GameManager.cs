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

        // Hide game over text
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

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

        // Start next wave after delay
        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        yield return new WaitForSeconds(2f);

        // Reset alien formation
        alienFormation.ResetFormation();
    }

    public void GameOver()
    {
        isGameOver = true;

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "GAME OVER\n\nScore: " + score + "\nHigh Score: " + highScore + "\n\nPress ENTER to restart";
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