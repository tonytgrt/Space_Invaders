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
    public AudioClip explosionSound;
    public GameObject explosionPrefab;

    public float ufoZ = 6f;  // Z position where UFO flies
    private float spawnTimer = 0f;
    private int direction = 1;
    private bool isActive = false;
    private AudioSource audioSource;
    private float leftSpawn = -10f;
    private float rightSpawn = 10f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true;
        audioSource.clip = flyingSound;

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
        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;

        // Play sound
        if (flyingSound != null)
        {
            audioSource.Play();
        }
    }

    void Deactivate()
    {
        isActive = false;
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        audioSource.Stop();
    }

    public void Die()
    {
        if (!isActive) return;

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
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Spawn explosion
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Deactivate();
    }
}