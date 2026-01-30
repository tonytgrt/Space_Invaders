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
        // Convert to debris when past player (missed)
        if (!isDebris && transform.position.z < missedZ)
        {
            ConvertToDebris();
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

    void OnTriggerEnter(Collider other)
    {
        if (isDebris) return;

        if (other.CompareTag("Player"))
        {
            // Hit player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
            ConvertToDebris();
        }
        else if (other.CompareTag("Shield"))
        {
            // Hit shield
            ShieldBlock shield = other.GetComponent<ShieldBlock>();
            if (shield != null)
            {
                shield.TakeDamage();
            }
            ConvertToDebris();
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
}
