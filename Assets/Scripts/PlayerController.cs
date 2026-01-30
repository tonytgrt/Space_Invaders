using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float leftBound = -7f;
    public float rightBound = 7f;

    [Header("Physics")]
    public float pushForce = 5f;           // Force applied to debris
    public float debrisSlowdown = 0.5f;    // Speed multiplier when in debris

    [Header("Firing")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public AudioClip shootSound;
    public float fireCooldown = 0.5f;  // Time between shots

    private float lastFireTime = -999f;
    private AudioSource audioSource;
    private Rigidbody rb;
    private float currentSpeedMultiplier = 1f;
    private int debrisContactCount = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup rigidbody for physics interactions
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                        RigidbodyConstraints.FreezePositionZ |
                        RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationY |
                        RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 5f;  // Heavier than debris
    }

    void Update()
    {
        HandleMovement();
        HandleFiring();
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // Apply slowdown if in debris
        float effectiveSpeed = moveSpeed * currentSpeedMultiplier;

        Vector3 newPosition = transform.position;
        newPosition.x += horizontalInput * effectiveSpeed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, leftBound, rightBound);
        transform.position = newPosition;
    }

    void HandleFiring()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastFireTime + fireCooldown)
        {
            Fire();
        }
    }

    void Fire()
    {
        if (bulletPrefab != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.forward * 0.5f;
            Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            lastFireTime = Time.time;

            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }
    }

    public void EnableFiring()
    {
    }

    // Physics collision with debris
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Debris"))
        {
            debrisContactCount++;
            UpdateSpeedMultiplier();

            // Push debris away
            PhysicsAlien debris = collision.gameObject.GetComponent<PhysicsAlien>();
            if (debris != null)
            {
                Vector3 pushDir = (collision.transform.position - transform.position).normalized;
                pushDir.y = 0;
                pushDir.z = 0; // Only push along X axis
                debris.Push(pushDir, pushForce);
            }

            // Also push bullet debris
            PhysicsBullet bulletDebris = collision.gameObject.GetComponent<PhysicsBullet>();
            if (bulletDebris != null)
            {
                Vector3 pushDir = (collision.transform.position - transform.position).normalized;
                pushDir.y = 0;
                pushDir.z = 0;
                bulletDebris.Push(pushDir, pushForce);
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Debris"))
        {
            // Continue pushing while in contact
            Rigidbody debrisRb = collision.gameObject.GetComponent<Rigidbody>();
            if (debrisRb != null)
            {
                float moveDir = Input.GetAxisRaw("Horizontal");
                if (moveDir != 0)
                {
                    debrisRb.AddForce(Vector3.right * moveDir * pushForce * 0.5f, ForceMode.Force);
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Debris"))
        {
            debrisContactCount = Mathf.Max(0, debrisContactCount - 1);
            UpdateSpeedMultiplier();
        }
    }

    void UpdateSpeedMultiplier()
    {
        if (debrisContactCount > 0)
        {
            // More debris = slower movement
            //currentSpeedMultiplier = debrisSlowdown / debrisContactCount;
            //currentSpeedMultiplier = debrisSlowdown;
            currentSpeedMultiplier = 1f;
        }
        else
        {
            currentSpeedMultiplier = 1f;
        }
    }

    // Note: AlienBullet collision is handled by PhysicsBulletAlien.cs
    // Player's collider is non-trigger for physics interactions with debris

    public void Die()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.PlayerHit();
        }
    }
}