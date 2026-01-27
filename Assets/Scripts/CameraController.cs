using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Default View (Top Down)")]
    public Vector3 defaultPosition = new Vector3(0, 11, 0);
    public Vector3 defaultRotation = new Vector3(90, 0, 0);

    [Header("First Person View")]
    public Vector3 firstPersonOffset = new Vector3(0, 1.5f, -1f);  // Offset from player
    public Vector3 firstPersonRotation = new Vector3(15, 0, 0);   // Slight downward tilt

    [Header("Settings")]
    public float transitionSpeed = 5f;
    public bool isFirstPersonMode = false;

    [Header("References")]
    public Transform player;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null)
            {
                player = pc.transform;
            }
        }

        SetDefaultView();
    }

    void Update()
    {
        // Toggle camera with Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleView();
        }

        // Update target position based on mode
        if (isFirstPersonMode && player != null)
        {
            // Follow player in first person mode
            targetPosition = player.position + firstPersonOffset;
            targetRotation = Quaternion.Euler(firstPersonRotation);
        }

        // Smooth transition to target
        transform.position = Vector3.Lerp(transform.position, targetPosition, transitionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, transitionSpeed * Time.deltaTime);
    }

    public void ToggleView()
    {
        isFirstPersonMode = !isFirstPersonMode;

        if (isFirstPersonMode)
        {
            SetFirstPersonView();
        }
        else
        {
            SetDefaultView();
        }
    }

    void SetDefaultView()
    {
        targetPosition = defaultPosition;
        targetRotation = Quaternion.Euler(defaultRotation);
    }

    void SetFirstPersonView()
    {
        if (player != null)
        {
            targetPosition = player.position + firstPersonOffset;
            targetRotation = Quaternion.Euler(firstPersonRotation);
        }
    }
}