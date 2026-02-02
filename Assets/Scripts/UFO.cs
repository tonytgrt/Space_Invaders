using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFO : MonoBehaviour
{
    public float speed = 3f;
    public float spawnInterval = 15f;
    public float spawnChance = 0.5f;
    public int[] pointValues = { 50, 100, 150, 300 };
    public AudioClip flyingSound;
    [Range(0f, 2f)] public float flyingVolume = 1f;
    public AudioClip explosionSound;
    [Range(0f, 2f)] public float explosionVolume = 1f;
    public GameObject explosionPrefab;

    public float ufoZ = 5f;  // Z position where UFO flies
    private float spawnTimer = 0f;
    private int direction = 1;
    private bool isActive = false;
    private AudioSource audioSource;
    private float leftSpawn = -15f;
    private float rightSpawn = 15f;
    private Renderer ufoRenderer;
    private Collider ufoCollider;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true;
        audioSource.clip = flyingSound;

        // Cache renderer (may be on this object or child)
        ufoRenderer = GetComponent<Renderer>();
        if (ufoRenderer == null)
        {
            ufoRenderer = GetComponentInChildren<Renderer>();
        }

        // Cache collider
        ufoCollider = GetComponent<Collider>();
        if (ufoCollider == null)
        {
            ufoCollider = GetComponentInChildren<Collider>();
        }

        Deactivate();
    }

    void Update()
    {
        if (!isActive)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                TrySpawn();
            }
        }
        else
        {
            // Move UFO along X axis
            transform.position += Vector3.right * speed * direction * Time.deltaTime;

            // Check if off screen
            if ((direction > 0 && transform.position.x > rightSpawn) ||
                (direction < 0 && transform.position.x < leftSpawn))
            {
                Deactivate();
            }
        }
    }

    void TrySpawn()
    {
        if (Random.value < spawnChance)
        {
            Spawn();
        }
    }

    void Spawn()
    {
        isActive = true;

        // Randomly choose direction
        direction = Random.value > 0.5f ? 1 : -1;

        // Position at appropriate edge, at Y=0
        float startX = direction > 0 ? leftSpawn : rightSpawn;
        transform.position = new Vector3(startX, 0, ufoZ);

        // Show and enable
        if (ufoRenderer != null)
        {
            ufoRenderer.enabled = true;
        }
        if (ufoCollider != null)
        {
            ufoCollider.enabled = true;
        }

        // Play sound
        if (flyingSound != null)
        {
            audioSource.volume = flyingVolume;
            audioSource.Play();
        }
    }

    void Deactivate()
    {
        isActive = false;

        // Hide renderer
        if (ufoRenderer != null)
        {
            ufoRenderer.enabled = false;
        }

        // Disable collider
        if (ufoCollider != null)
        {
            ufoCollider.enabled = false;
        }

        // Stop sound
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Move off screen to be safe
        transform.position = new Vector3(leftSpawn - 10f, 0, ufoZ);
    }

    public void Die()
    {
        if (!isActive)
        {
            Debug.Log("UFO.Die() called but UFO is not active");
            return;
        }

        Debug.Log("UFO destroyed!");

        // Random point value
        int points = pointValues[Random.Range(0, pointValues.Length)];

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.AddScore(points);
            gm.ShowFloatingScore(points, transform.position);
        }

        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        }

        // Spawn explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Deactivate();
    }

    /// <summary>
    /// Check if UFO is currently flying
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }

    // Fallback: detect player bullets hitting UFO directly
    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("PlayerBullet"))
        {
            Die();
            Destroy(other.gameObject);
        }
    }
}