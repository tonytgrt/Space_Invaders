using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldGenerator : MonoBehaviour
{
    public GameObject shieldBlockPrefab;
    public int blocksWide = 5;
    public int blocksDeep = 4;
    public float blockSize = 0.3f;


    // Start is called before the first frame update
    void Start()
    {
        GenerateShield();
    }

    void GenerateShield()
    {
        if (shieldBlockPrefab == null) return;

        float startX = -(blocksWide - 1) * blockSize / 2f;
        float startZ = (blocksDeep - 1) * blockSize / 2f;

        for (int row = 0; row < blocksDeep; row++)
        {
            for (int col = 0; col < blocksWide; col++)
            {
                // Skip blocks to create arch shape (optional)
                if (row == 0 && col > 0 && col < blocksWide - 1)
                {
                    // Skip middle front blocks for arch opening
                    if (col >= blocksWide / 2 - 1 && col <= blocksWide / 2 + 1)
                        continue;
                }

                Vector3 pos = new Vector3(
                    startX + col * blockSize,
                    0,  // Y = 0
                    startZ - row * blockSize
                );

                Instantiate(shieldBlockPrefab, transform.position + pos, Quaternion.identity, transform);
            }
        }
    }
}

