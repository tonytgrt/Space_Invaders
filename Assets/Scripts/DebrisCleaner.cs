using UnityEngine;

/// <summary>
/// Destroys debris that is pushed off the sides of the play area
/// Reports to DebrisRecycleManager for reward tracking
/// </summary>
public class DebrisCleaner : MonoBehaviour
{
    public AudioClip clearSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Debris"))
        {
            // Report to recycle manager for reward tracking
            if (DebrisRecycleManager.Instance != null)
            {
                DebrisRecycleManager.Instance.OnDebrisCleared(other.transform.position);
            }
            else
            {
                // Fallback: play sound locally if no manager
                if (clearSound != null)
                {
                    AudioSource.PlayClipAtPoint(clearSound, transform.position, 0.5f);
                }
            }

            Destroy(other.gameObject);
        }
    }
}
