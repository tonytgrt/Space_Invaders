using UnityEngine;

public class DebrisCleaner : MonoBehaviour
{
    public AudioClip clearSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Debris"))
        {
            if (clearSound != null)
            {
                AudioSource.PlayClipAtPoint(clearSound, transform.position, 0.5f);
            }
            Destroy(other.gameObject);
        }
    }
}