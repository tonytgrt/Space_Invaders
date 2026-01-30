using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsAlien : MonoBehaviour
{
    [Header("Physics Settings")]
    public float groundGravity = 2f;      // Pull toward ground (-Z)
    public float groundZ = -7f;           // Where debris stops (behind player)
    public float debrisDrag = 3f;         // Friction when on ground
    public float deathForce = 3f;         // Initial push when killed

    private Rigidbody rb;
    private bool isDead = false;
    private bool isOnGround = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isDead && !isOnGround && rb != null)
        {
            // Apply "gravity" toward ground (-Z direction)
            rb.AddForce(Vector3.back * groundGravity, ForceMode.Acceleration);

            // Clamp velocity to prevent overshooting
            Vector3 vel = rb.velocity;
            vel.z = Mathf.Max(vel.z, -10f); // Limit downward speed
            rb.velocity = vel;

            // Check if reached ground
            if (transform.position.z <= groundZ)
            {
                BecomeDebris();
            }
        }
    }

    void Update()
    {
        // Backup check in Update (in case FixedUpdate misses it)
        if (isDead && !isOnGround && transform.position.z <= groundZ)
        {
            BecomeDebris();
        }
    }

    public void ConvertToDebris()
    {
        isDead = true;

        // Enable physics
        rb.isKinematic = false;
        rb.useGravity = false;

        // IMPORTANT: Change collider from trigger to physical
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;  // Now it can physically collide
        }

        // Remove from formation control
        transform.SetParent(null);

        // Apply random force for variety
        Vector3 randomForce = new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-deathForce, -deathForce * 0.5f)
        );
        rb.AddForce(randomForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

        // Change tag to Debris
        gameObject.tag = "Debris";

        // Disable alien script behaviors
        Alien alienScript = GetComponent<Alien>();
        if (alienScript != null)
        {
            alienScript.enabled = false;
        }
    }


    void BecomeDebris()
    {
        if (isOnGround) return; // Prevent multiple calls
        isOnGround = true;

        // Stop at ground level
        Vector3 pos = transform.position;
        pos.z = groundZ;
        pos.y = 0; // Ensure on play plane
        transform.position = pos;

        // Stop all movement in Z direction
        if (rb != null)
        {
            Vector3 vel = rb.velocity;
            vel.z = 0;
            rb.velocity = vel;

            // Increase drag to simulate friction
            rb.drag = debrisDrag;
            rb.angularDrag = debrisDrag;

            // Constrain to ground - freeze Y and Z position
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                            RigidbodyConstraints.FreezePositionZ |
                            RigidbodyConstraints.FreezeRotationX |
                            RigidbodyConstraints.FreezeRotationZ;
        }
    }

    // Called when player pushes debris
    public void Push(Vector3 direction, float force)
    {
        if (isOnGround && rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }
}