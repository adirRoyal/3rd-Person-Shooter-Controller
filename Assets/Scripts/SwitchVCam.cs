using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;

// This class handles the switching of virtual cameras and UI canvases when aiming.
public class SwitchVCam : MonoBehaviour
{
    // Serialized fields for assigning in the Unity Editor
    [SerializeField] PlayerInput playerInput;
    [SerializeField] int priorityBoostAmount = 10;
    [SerializeField] Canvas thirdPersonCanvas;
    [SerializeField] Canvas aimCanvas;

    // Private variables
    private CinemachineVirtualCamera virtualCamera;
    private InputAction aimAction;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        aimAction = playerInput.actions["Aim"];
    }

    // OnEnable is called when the object becomes enabled and active
    private void OnEnable()
    {
        aimAction.performed += OnAimPerformed;
        aimAction.canceled += OnAimCanceled;
    }

    // OnDisable is called when the behaviour becomes disabled or inactive
    private void OnDisable()
    {
        aimAction.performed -= OnAimPerformed;
        aimAction.canceled -= OnAimCanceled;
    }

    // Method to handle the start of aiming
    private void StartAim()
    {
        virtualCamera.Priority += priorityBoostAmount;
        aimCanvas.enabled = true;
        thirdPersonCanvas.enabled = false;
    }

    // Method to handle the cancellation of aiming
    private void CancelAim()
    {
        virtualCamera.Priority -= priorityBoostAmount;
        aimCanvas.enabled = false;
        thirdPersonCanvas.enabled = true;
    }

    // Event handler for when the aim action is performed
    private void OnAimPerformed(InputAction.CallbackContext context)
    {
        StartAim();
    }

    // Event handler for when the aim action is canceled
    private void OnAimCanceled(InputAction.CallbackContext context)
    {
        CancelAim();
    }
}
