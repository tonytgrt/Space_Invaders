using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float leftBound = -10f;
    public float rightBound = 10f;

    [Header("Firing")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public AudioClip shootSound;

    private bool canFire = true;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        Vector3 newPosition = transform.position;
        newPosition.x += horizontalInput * moveSpeed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, leftBound, rightBound);
        transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.Space) && canFire)
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
            canFire = false;

            if (shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }
    }

    public void EnableFiring()
    {
        canFire = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AlienBullet"))
        {
            Die();
            Destroy(other.gameObject);
        }
    }

    public void Die()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.PlayerHit();
        }
    }



}


