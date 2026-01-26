using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float speed = 10f;
    public float maxZ = 8f;  // Destroy when bullet reaches this Z position

    private PlayerController playerController;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        // Move in +Z direction (toward aliens)
        GetComponent<Rigidbody>().velocity = Vector3.forward * speed;
    }

    void Update()
    {
        // Destroy if out of bounds (past the aliens)
        if (transform.position.z > maxZ)
        {
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        if (playerController != null)
        {
            playerController.EnableFiring();
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Alien"))
        {
            Alien alien = other.GetComponent<Alien>();
            if (alien != null)
            {
                alien.Die();
            }
            DestroyBullet();
        }
        else if (other.CompareTag("Shield"))
        {
            ShieldBlock shield = other.GetComponent<ShieldBlock>();
            if (shield != null)
            {
                shield.TakeDamage();
            }
            DestroyBullet();
        }
        else if (other.CompareTag("UFO"))
        {
            UFO ufo = other.GetComponent<UFO>();
            if (ufo != null)
            {
                ufo.Die();
            }
            DestroyBullet();
        }
    }
}