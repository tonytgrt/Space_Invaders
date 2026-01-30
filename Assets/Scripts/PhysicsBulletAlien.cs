using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Alien bullet with physics - converts to debris after hitting or missing target
/// </summary>
public class PhysicsBulletAlien : MonoBehaviour
{
    [Header("Flight Settings")]
    public float speed = 5f;
    public float missedZ = -5.5f;  // Convert to debris when past player (missed)
    public float maxZ = 8f;        // Convert to debris when past aliens (as player bullet)

    [Header("Physics Settings")]
    public float groundZ = -6f;           // Ground position (behind player)
    public float groundGravity = 8f;      // Pull toward ground (faster than player bullet)
    public float debrisDrag = 5f;
    public float lifetime = 30f;          // Destroy debris after this time

    private Rigidbody rb;
    private bool isDebris = false;
    private bool isOnGround = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.mass = 0.1f;

        // Alien bullets fly toward player (-Z direction)
        rb.velocity = Vector3.back * speed;
    }

    void Update()
    {
        // Convert to debris when past player (missed) - for alien bullet
        if (!isDebris && gameObject.CompareTag("AlienBullet") && transform.position.z <= missedZ)
        {
            ConvertToDebris();
        }

        // Convert to debris when past aliens (missed) - for player bullet
        if (!isDebris && gameObject.CompareTag("PlayerBullet") && transform.position.z > maxZ)
        {
            ConvertToPlayerDebris();
        }

        // Ground check for debris
        if (isDebris && !isOnGround && transform.position.z <= groundZ)
        {
            BecomeGroundDebris();
        }
    }

    void FixedUpdate()
    {
        if (isDebris && !isOnGround && rb != null)
        {
            // Apply gravity toward ground (-Z)
            rb.AddForce(Vector3.back * groundGravity, ForceMode.Acceleration);

            // Clamp velocity to prevent overshooting
            Vector3 vel = rb.velocity;
            vel.z = Mathf.Max(vel.z, -15f);
            rb.velocity = vel;

            if (transform.position.z <= groundZ)
            {
                BecomeGroundDebris();
            }
        }
    }

    void ConvertToDebris()
    {
        if (isDebris) return;
        isDebris = true;

        // Stop forward movement
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }

        // Change to debris tag
        gameObject.tag = "Debris";

        // Change collider from trigger to physical
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
        }

        // Add random tumble
        if (rb != null)
        {
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
        }

        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void BecomeGroundDebris()
    {
        if (isOnGround) return;
        isOnGround = true;

        // Clamp to ground position
        Vector3 pos = transform.position;
        pos.z = groundZ;
        pos.y = 0;
        transform.position = pos;

        if (rb != null)
        {
            // Stop all Z velocity
            Vector3 vel = rb.velocity;
            vel.z = 0;
            rb.velocity = vel;

            // High friction
            rb.drag = debrisDrag;
            rb.angularDrag = debrisDrag;

            // Lock position on ground
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                            RigidbodyConstraints.FreezePositionZ |
                            RigidbodyConstraints.FreezeRotationX |
                            RigidbodyConstraints.FreezeRotationZ;
        }
    }

    public void Push(Vector3 direction, float force)
    {
        if (isOnGround && rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    // When player touches debris on ground, fire it as a player bullet
    void OnCollisionEnter(Collision collision)
    {
        //if (!isOnGround) return;  // Only when on ground as debris

        if (collision.gameObject.CompareTag("Player"))
        {
            FireAsPlayerBullet(collision.gameObject);
        }
    }

    void FireAsPlayerBullet(GameObject player)
    {
        // Get player's fire point
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null) return;

        gameObject.tag = "PlayerBullet";

        Transform firePoint = playerController.firePoint;
        Vector3 spawnPos = firePoint != null ? firePoint.position : player.transform.position + Vector3.forward * 0.5f;

        // Move to fire point
        transform.position = spawnPos;

        

        // Reset physics state
        isOnGround = false;
        isDebris = false;

        // Remove constraints so it can move freely
        rb.constraints = RigidbodyConstraints.None;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;

        // Make collider a trigger again for hit detection
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Fire in +Z direction (toward aliens)
        rb.velocity = Vector3.forward * speed * 2f;  // Faster than alien bullet
        rb.angularVelocity = Vector3.zero;

        // Cancel any pending destroy
        CancelInvoke();
    }

    // Handle hitting targets when fired as player bullet
    void OnTriggerEnter(Collider other)
    {
        // Original alien bullet behavior (hitting player/shield)
        if (!isDebris && gameObject.CompareTag("AlienBullet"))
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.Die();
                }
                ConvertToDebris();
            }
            else if (other.CompareTag("Shield"))
            {
                ShieldBlock shield = other.GetComponent<ShieldBlock>();
                if (shield != null)
                {
                    shield.TakeDamage();
                }
                ConvertToDebris();
            }
        }
        // Player bullet behavior (hitting aliens/shields/UFO)
        else if (!isDebris && gameObject.CompareTag("PlayerBullet"))
        {
            if (other.CompareTag("Alien"))
            {
                Alien alien = other.GetComponent<Alien>();
                if (alien != null)
                {
                    alien.Die();
                }
                ConvertToPlayerDebris();
            }
            else if (other.CompareTag("Shield"))
            {
                ShieldBlock shield = other.GetComponent<ShieldBlock>();
                if (shield != null)
                {
                    shield.TakeDamage();
                }
                ConvertToPlayerDebris();
            }
            else if (other.CompareTag("UFO"))
            {
                UFO ufo = other.GetComponent<UFO>();
                if (ufo != null)
                {
                    ufo.Die();
                }
                ConvertToPlayerDebris();
            }
        }
    }

    // Convert to debris when fired as player bullet and misses
    void ConvertToPlayerDebris()
    {
        if (isDebris) return;
        isDebris = true;

        rb.velocity = Vector3.zero;
        gameObject.tag = "Debris";

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
        }

        rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
        Destroy(gameObject, lifetime);
    }
}
