using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBullet : MonoBehaviour
{
    [Header("Flight Settings")]
    public float speed = 10f;
    public bool isPlayerBullet = true;

    [Header("Physics Settings")]
    public float groundZ = -6f;           // Ground position (behind player)
    public float missedZ = -5f;           // Z position where alien bullet "missed" and becomes debris
    public float groundGravity = 5f;      // Pull toward ground when not hitting anything
    public float debrisDrag = 5f;
    public float lifetime = 30f;          // Destroy debris after this time

    private Rigidbody rb;
    private bool isDebris = false;
    private bool isOnGround = false;
    private PlayerController playerController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.mass = 0.1f;

        if (isPlayerBullet)
        {
            playerController = FindObjectOfType<PlayerController>();
            rb.velocity = Vector3.forward * speed;
        }
        else
        {
            rb.velocity = Vector3.back * speed;
        }
    }

    void Update()
    {
        if (!isDebris)
        {
            // Check bounds - convert to debris when bullet misses target
            if (isPlayerBullet && transform.position.z > 8f)
            {
                // Player bullet went past aliens
                ConvertToDebris();
            }
            else if (!isPlayerBullet && transform.position.z < missedZ)
            {
                // Alien bullet went past player - becomes debris and falls to ground
                ConvertToDebris();
            }
        }

        // Backup ground check for debris
        if (isDebris && !isOnGround && transform.position.z <= groundZ)
        {
            BecomeGroundDebris();
        }
    }

    void FixedUpdate()
    {
        if (isDebris && !isOnGround && rb != null)
        {
            // Apply gravity toward ground
            rb.AddForce(Vector3.back * groundGravity, ForceMode.Acceleration);

            // Clamp velocity to prevent overshooting
            Vector3 vel = rb.velocity;
            vel.z = Mathf.Max(vel.z, -10f);
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

        if (isPlayerBullet)
        {
            if (other.CompareTag("Alien"))
            {
                Alien alien = other.GetComponent<Alien>();
                if (alien != null)
                {
                    alien.Die();
                }
                ConvertToDebris();

                // Re-enable player firing
                if (playerController != null)
                {
                    playerController.EnableFiring();
                }
            }
            else if (other.CompareTag("Shield"))
            {
                ShieldBlock shield = other.GetComponent<ShieldBlock>();
                if (shield != null)
                {
                    shield.TakeDamage();
                }
                ConvertToDebris();

                if (playerController != null)
                {
                    playerController.EnableFiring();
                }
            }
            else if (other.CompareTag("UFO"))
            {
                UFO ufo = other.GetComponent<UFO>();
                if (ufo != null)
                {
                    ufo.Die();
                }
                ConvertToDebris();

                if (playerController != null)
                {
                    playerController.EnableFiring();
                }
            }
        }
        else // Alien bullet
        {
            if (other.CompareTag("Player"))
            {
                // Call player.Die() since player's collider is non-trigger
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
    }

    void ConvertToDebris()
    {
        isDebris = true;

        // Stop current movement
        rb.velocity = Vector3.zero;

        // Change to debris tag
        gameObject.tag = "Debris";

        // Change collider from trigger to physical
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
        }

        // Add some random tumble
        rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

        // Start lifetime countdown
        Destroy(gameObject, lifetime);
    }

    void BecomeGroundDebris()
    {
        if (isOnGround) return; // Prevent multiple calls
        isOnGround = true;

        // Clamp to ground
        Vector3 pos = transform.position;
        pos.z = groundZ;
        pos.y = 0; // Ensure on play plane
        transform.position = pos;

        if (rb != null)
        {
            // Stop Z velocity
            Vector3 vel = rb.velocity;
            vel.z = 0;
            rb.velocity = vel;

            // High friction
            rb.drag = debrisDrag;
            rb.angularDrag = debrisDrag;

            // Lock Z position
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