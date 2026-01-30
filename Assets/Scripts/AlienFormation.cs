using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienFormation : MonoBehaviour
{
    [Header("Formation Settings")]
    public int rows = 5;
    public int columns = 11;
    public float horizontalSpacing = 0.8f;  // X spacing
    public float depthSpacing = 1.0f;       // Z spacing between rows

    [Header("Movement Settings")]
    public float baseStepTime = 1.0f;
    public float minStepTime = 0.1f;
    public float stepDistance = 0.3f;       // X step distance
    public float dropDistance = 0.4f;       // Z drop distance (toward player)
    public float leftBound = -10f;
    public float rightBound = 10f;
    public float playerZ = -5f;             // Z position where player is (game over line)

    [Header("Prefabs")]
    public GameObject squidPrefab;
    public GameObject crabPrefab;
    public GameObject octopusPrefab;
    public GameObject alienBulletPrefab;

    [Header("Audio")]
    public AudioClip stepSound;
    public AudioClip[] stepSounds; // For the classic 4-note rhythm

    private List<Alien> aliens = new List<Alien>();
    private int direction = 1; // 1 = right, -1 = left
    private float stepTimer = 0f;
    private float currentStepTime;
    private int aliensRemaining;
    private int totalAliens;
    private AudioSource audioSource;
    private int stepSoundIndex = 0;

    [Header("Firing")]
    public float baseFireInterval = 2f;
    public float minFireInterval = 0.3f;
    private float currentFireInterval;
    private float fireTimer = 0f;

    [Header("Descent Speed Scaling")]
    public float startZ = 5f;              // Starting Z position of formation
    public float maxDescentSpeedBonus = 3f; // Max speed multiplier from descent

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        SpawnFormation();
        currentStepTime = baseStepTime;
        currentFireInterval = baseFireInterval;
    }

    void SpawnFormation()
    {
        aliens.Clear();

        float startX = -(columns - 1) * horizontalSpacing / 2f;
        float startZ = 0;  // Local Z position within formation

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Rows are arranged in -Z direction (front rows closer to player)
                Vector3 localPosition = new Vector3(
                    startX + col * horizontalSpacing,
                    0,  // All on Y=0 plane
                    startZ - row * depthSpacing
                );

                GameObject prefab;
                AlienType type;

                // Back row (highest Z) = Octopus, middle = Crab, front = Squid
                if (row == 0)
                {
                    prefab = octopusPrefab != null ? octopusPrefab : squidPrefab;
                    type = AlienType.Octopus;
                }
                else if (row < 3)
                {
                    prefab = crabPrefab != null ? crabPrefab : squidPrefab;
                    type = AlienType.Crab;
                }
                else
                {
                    prefab = squidPrefab;
                    type = AlienType.Squid;
                }

                if (prefab != null)
                {
                    GameObject alienObj = Instantiate(prefab, transform.position + localPosition, Quaternion.identity, transform);
                    Alien alien = alienObj.GetComponent<Alien>();
                    if (alien != null)
                    {
                        alien.alienType = type;
                        aliens.Add(alien);
                    }
                }
            }
        }

        totalAliens = aliens.Count;
        aliensRemaining = totalAliens;
    }

    void Update()
    {
        stepTimer += Time.deltaTime;
        fireTimer += Time.deltaTime;

        // Move formation
        if (stepTimer >= currentStepTime)
        {
            stepTimer = 0f;
            MoveFormation();
        }

        // Random alien fires
        if (fireTimer >= currentFireInterval)
        {
            fireTimer = 0f;
            AlienFire();
        }
    }

    void MoveFormation()
    {
        // Check if any alien would go out of bounds (X direction)
        bool shouldDrop = false;

        foreach (Alien alien in aliens)
        {
            if (alien == null || !alien.IsAlive()) continue;

            float nextX = alien.transform.position.x + (stepDistance * direction);
            if (nextX > rightBound || nextX < leftBound)
            {
                shouldDrop = true;
                break;
            }
        }

        if (shouldDrop)
        {
            // Drop toward player (-Z direction) and reverse horizontal direction
            transform.position += Vector3.back * dropDistance;
            direction *= -1;

            // Update speed based on new position
            UpdateDescentSpeed();

            // Check if any alien reached the player's line
            CheckPlayerLineReached();
        }
        else
        {
            // Move horizontally (X direction)
            transform.position += Vector3.right * stepDistance * direction;
        }

        // Play step sound
        PlayStepSound();
    }

    void PlayStepSound()
    {
        if (stepSounds != null && stepSounds.Length > 0)
        {
            audioSource.PlayOneShot(stepSounds[stepSoundIndex]);
            stepSoundIndex = (stepSoundIndex + 1) % stepSounds.Length;
        }
        else if (stepSound != null)
        {
            audioSource.PlayOneShot(stepSound);
        }
    }

    void AlienFire()
    {
        if (alienBulletPrefab == null) return;

        // Get list of frontmost aliens (lowest Z) in each column
        List<Alien> shooters = GetFrontmostAliens();

        if (shooters.Count > 0)
        {
            // Random alien fires
            Alien shooter = shooters[Random.Range(0, shooters.Count)];
            if (shooter != null && shooter.IsAlive())
            {
                // Spawn bullet slightly in front of alien (-Z direction toward player)
                Vector3 spawnPos = shooter.transform.position + Vector3.back * 0.3f;
                Instantiate(alienBulletPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    List<Alien> GetFrontmostAliens()
    {
        // Find the frontmost (lowest Z, closest to player) alien in each column
        Dictionary<int, Alien> frontmost = new Dictionary<int, Alien>();

        foreach (Alien alien in aliens)
        {
            if (alien == null || !alien.IsAlive()) continue;

            int col = Mathf.RoundToInt((alien.transform.localPosition.x + (columns - 1) * horizontalSpacing / 2f) / horizontalSpacing);

            // Lower Z = closer to player = frontmost
            if (!frontmost.ContainsKey(col) || alien.transform.position.z < frontmost[col].transform.position.z)
            {
                frontmost[col] = alien;
            }
        }

        return new List<Alien>(frontmost.Values);
    }

    void CheckPlayerLineReached()
    {
        foreach (Alien alien in aliens)
        {
            if (alien != null && alien.IsAlive() && alien.transform.position.z <= playerZ)
            {
                GameManager gm = FindObjectOfType<GameManager>();
                if (gm != null)
                {
                    gm.GameOver();
                }
                return;
            }
        }
    }

    void UpdateDescentSpeed()
    {
        // Calculate how far the formation has descended (0 = at start, 1 = at player)
        float descentProgress = Mathf.Clamp01((startZ - transform.position.z) / (startZ - playerZ));

        // Apply descent bonus to speed (higher = faster movement)
        float descentSpeedMultiplier = 1f + descentProgress * maxDescentSpeedBonus;

        // Combine with remaining aliens multiplier (avoid division by zero)
        float aliensMultiplier = 1f;
        if (totalAliens > 0)
        {
            aliensMultiplier = 1f + (1f - (float)aliensRemaining / totalAliens) * 3f;
        }
        float totalMultiplier = descentSpeedMultiplier * aliensMultiplier;

        currentStepTime = Mathf.Max(minStepTime, baseStepTime / totalMultiplier);

        // Also speed up firing based on descent
        float fireMultiplier = 1f + descentProgress * 2f; // Fire up to 3x faster at player line
        currentFireInterval = Mathf.Max(minFireInterval, baseFireInterval / fireMultiplier);
    }

    public void OnAlienKilled(Alien alien)
    {
        aliensRemaining--;

        // Recalculate speeds (combines descent + remaining aliens multipliers)
        UpdateDescentSpeed();

        // Check for wave complete
        if (aliensRemaining <= 0)
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.WaveComplete();
            }
        }
    }

    public void ResetFormation()
    {
        // Destroy existing aliens
        foreach (Alien alien in aliens)
        {
            if (alien != null)
            {
                Destroy(alien.gameObject);
            }
        }

        // Reset position
        transform.position = new Vector3(0, 0, startZ);
        direction = 1;
        currentStepTime = baseStepTime;
        currentFireInterval = baseFireInterval;

        // Spawn new formation
        SpawnFormation();
    }

    public int GetRemainingAliens()
    {
        return aliensRemaining;
    }
}