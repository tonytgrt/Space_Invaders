using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBlock : MonoBehaviour
{
    public int maxHealth = 4;
    private int currentHealth;
    private Renderer blockRenderer;
    private Color originalColor;
    private Vector3 originalScale;


    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        blockRenderer = GetComponent<Renderer>();
        originalScale = transform.localScale;
        if (blockRenderer != null)
        {
            originalColor = blockRenderer.material.color;
        }
    }

    public void TakeDamage()
    {
        currentHealth--;
        UpdateVisual();
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    void UpdateVisual()
    {
        if (blockRenderer != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            Color newColor = originalColor;
            newColor.a = healthPercent;
            blockRenderer.material.color = newColor;

            float scaleMultiplier = 0.7f + 0.3f * healthPercent;
            transform.localScale = originalScale * scaleMultiplier;
        }
    }
}
