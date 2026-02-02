using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages debris recycling rewards - tracks debris cleared and awards power-ups
/// Part D Requirement 5 & 6: New resource and new goal
/// </summary>
public class DebrisRecycleManager : MonoBehaviour
{
    public static DebrisRecycleManager Instance { get; private set; }

    [Header("Reward Thresholds")]
    public int extraLifeThreshold = 15;      // Debris needed for extra life
    public int powerUpThreshold = 30;        // Debris needed for power-up

    [Header("Power-Up Settings")]
    public float sizeMultiplier = 1.5f;      // How much bigger player gets
    public float fireRateMultiplier = 2f;    // How much faster player fires

    [Header("Audio")]
    public AudioClip debrisClearSound;
    [Range(0f, 2f)] public float debrisClearVolume = 0.5f;
    public AudioClip extraLifeSound;
    [Range(0f, 2f)] public float extraLifeVolume = 1f;
    public AudioClip powerUpSound;
    [Range(0f, 2f)] public float powerUpVolume = 1f;

    [Header("UI")]
    public Text debrisCountText;
    public Text rewardText;

    [Header("Visual Effects")]
    public GameObject extraLifeEffectPrefab;
    public GameObject powerUpEffectPrefab;

    private int debrisCleared = 0;
    private bool extraLifeAwarded = false;
    private bool powerUpAwarded = false;
    private GameManager gameManager;
    private PlayerController player;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        player = FindObjectOfType<PlayerController>();
        UpdateUI();
    }

    /// <summary>
    /// Called by DebrisCleaner when debris is destroyed
    /// </summary>
    public void OnDebrisCleared(Vector3 position)
    {
        debrisCleared++;

        // Play clear sound
        if (debrisClearSound != null)
        {
            AudioSource.PlayClipAtPoint(debrisClearSound, position, debrisClearVolume);
        }

        // Check for extra life reward
        if (!extraLifeAwarded && debrisCleared >= extraLifeThreshold)
        {
            AwardExtraLife();
        }

        // Check for power-up reward
        if (!powerUpAwarded && debrisCleared >= powerUpThreshold)
        {
            AwardPowerUp();
        }

        UpdateUI();
    }

    void AwardExtraLife()
    {
        extraLifeAwarded = true;

        // Give extra life
        if (gameManager != null)
        {
            gameManager.AddLife();
        }

        // Play sound
        if (extraLifeSound != null)
        {
            AudioSource.PlayClipAtPoint(extraLifeSound, Camera.main.transform.position, extraLifeVolume);
        }

        // Spawn effect at player position
        if (extraLifeEffectPrefab != null && player != null)
        {
            Instantiate(extraLifeEffectPrefab, player.transform.position, Quaternion.identity);
        }

        // Show reward text
        ShowRewardText("EXTRA LIFE!");

        Debug.Log("Debris Recycle: Extra Life Awarded!");
    }

    void AwardPowerUp()
    {
        powerUpAwarded = true;

        // Apply power-up to player
        if (player != null)
        {
            player.ApplyRecyclePowerUp(sizeMultiplier, fireRateMultiplier);
        }

        // Play sound
        if (powerUpSound != null)
        {
            AudioSource.PlayClipAtPoint(powerUpSound, Camera.main.transform.position, powerUpVolume);
        }

        // Spawn effect at player position
        if (powerUpEffectPrefab != null && player != null)
        {
            Instantiate(powerUpEffectPrefab, player.transform.position, Quaternion.identity);
        }

        // Show reward text
        ShowRewardText("POWER UP!");

        Debug.Log("Debris Recycle: Power-Up Awarded! Player is bigger and fires faster!");
    }

    void ShowRewardText(string message)
    {
        if (rewardText != null)
        {
            rewardText.text = message;
            rewardText.gameObject.SetActive(true);
            StartCoroutine(HideRewardText());
        }
    }

    IEnumerator HideRewardText()
    {
        yield return new WaitForSeconds(2f);
        if (rewardText != null)
        {
            rewardText.gameObject.SetActive(false);
        }
    }

    void UpdateUI()
    {
        if (debrisCountText != null)
        {
            string status = "";
            if (!extraLifeAwarded)
            {
                status = $"RECYCLE: {debrisCleared}/{extraLifeThreshold} (Life)";
            }
            else if (!powerUpAwarded)
            {
                status = $"RECYCLE: {debrisCleared}/{powerUpThreshold} (Power)";
            }
            else
            {
                status = $"RECYCLE: {debrisCleared} (MAX)";
            }
            debrisCountText.text = status;
        }
    }

    /// <summary>
    /// Reset for new wave/game
    /// </summary>
    public void ResetRecycling()
    {
        debrisCleared = 0;
        extraLifeAwarded = false;
        powerUpAwarded = false;
        UpdateUI();
    }

    public int GetDebrisCleared()
    {
        return debrisCleared;
    }

    public bool HasExtraLife()
    {
        return extraLifeAwarded;
    }

    public bool HasPowerUp()
    {
        return powerUpAwarded;
    }
}
