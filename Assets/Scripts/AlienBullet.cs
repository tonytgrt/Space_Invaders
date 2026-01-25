using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienBullet : MonoBehaviour
{
    public float speed = 5f;
    public float minZ = -7f;


    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.velocity = Vector3.back * speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.z < minZ)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Shield"))
        {
            //ShieldBlock shield = other.GetComponent<ShieldBlock>();
            //if (shield != null)
            //{
            //    shield.TakeDamage();
            //}
            //Destroy(gameObject);
        }
    }
}
