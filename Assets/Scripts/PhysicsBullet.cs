using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player bullet with physics - converts to debris after hitting or missing target
/// </summary>
public class PhysicsBullet : MonoBehaviour
{
    [Header("Flight Settings")]
    public float speed = 10f;
    public float maxZ = 8f;  // Convert to debris when past this Z

    [Header("Physics Settings")]
    public float groundZ = -6f;           // Ground position (behind player)
    public float groundGravity = 5f;      // Pull toward ground
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
        rb.velocity = Vector3.forward * speed;
    }

    void Update()
    {
        // Convert to debris when past aliens
        if (!isDebris && transform.position.z > maxZ)
        {
            ConvertToDebris();
        }

        // Backup ground check
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

            // Clamp velocity
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

        if (other.CompareTag("Alien"))
        {
            Alien alien = other.GetComponent<Alien>();
            if (alien != null)
            {
                alien.Die();
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Shield"))
        {
            ShieldBlock shield = other.GetComponent<ShieldBlock>();
            if (shield != null)
            {
                shield.TakeDamage();
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("UFO"))
        {
            UFO ufo = other.GetComponent<UFO>();
            if (ufo != null)
            {
                ufo.Die();
            }
            Destroy(gameObject);
        }
    }

    void ConvertToDebris()
    {
        isDebris = true;

        // Stop movement
        rb.velocity = Vector3.zero;

        // Change to debris tag
        gameObject.tag = "Debris";

        // Change collider from trigger to physical
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
        }

        // Add random tumble
        rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void BecomeGroundDebris()
    {
        if (isOnGround) return;
        isOnGround = true;

        // Clamp to ground
        Vector3 pos = transform.position;
        pos.z = groundZ;
        pos.y = 0;
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

            // Lock position
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
