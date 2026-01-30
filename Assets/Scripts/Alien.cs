using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AlienType { Squid, Crab, Octopus }

public class Alien : MonoBehaviour
{
    public int pointValue = 10;
    public AlienType alienType = AlienType.Squid;
    public AudioClip deathSound;
    public GameObject explosionPrefab;

    private AlienFormation formation;
    private bool isAlive = true;

    void Start()
    {
        formation = FindObjectOfType<AlienFormation>();

        // Set point value based on type
        switch (alienType)
        {
            case AlienType.Squid:
                pointValue = 10;
                break;
            case AlienType.Crab:
                pointValue = 20;
                break;
            case AlienType.Octopus:
                pointValue = 30;
                break;
        }
    }

    public void Die()
    {
        if (!isAlive) return;
        isAlive = false;

        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // Spawn explosion effect (Part D Requirement 3)
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Add score
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.AddScore(pointValue);
        }

        // Notify formation
        if (formation != null)
        {
            formation.OnAlienKilled(this);
        }

        // Convert to physics debris instead of destroying
        PhysicsAlien physicsAlien = GetComponent<PhysicsAlien>();
        if (physicsAlien != null)
        {
            physicsAlien.ConvertToDebris();
        }
        else
        {
            // Fallback: destroy if no physics component
            Destroy(gameObject);
        }
    }

    public bool IsAlive()
    {
        return isAlive;
    }
}