using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float speed = 10f;
    public float maxZ = 8f;

    private PlayerController playerController;


    // Start is called before the first frame update
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        GetComponent<Rigidbody>().velocity = Vector3.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
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
        //else if (other.CompareTag("Shield"))
        //{
        //    ShieldBlock shield = other.GetComponent<ShieldBlock>();
        //    if (shield != null)
        //    {
        //        shield.TakeDamage();
        //    }
        //    DestroyBullet();
        //}
        //else if (other.CompareTag("UFO"))
        //{
        //    UFO ufo = other.GetComponent<UFO>();
        //    if (ufo != null)
        //    {
        //        ufo.Die();
        //    }
        //    DestroyBullet();
        //}
    }
}
