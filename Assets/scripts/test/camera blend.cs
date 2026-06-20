using UnityEngine;
using Unity.Cinemachine; 
using UnityEngine.InputSystem; // Required for the new input system

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Camera Blend Settings")]
    public CinemachineCamera vCamWide;
    public CinemachineCamera vCamCloseUp;

    void Update()
    {
        // Safety check to ensure a keyboard is connected, then check the Space key
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SwitchToCloseUp();
        }
        
        // Check for the R key
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            SwitchToWide();
        }
    }

    public void SwitchToCloseUp()
    {
        vCamWide.Priority = 10;
        vCamCloseUp.Priority = 20; 
    }

    public void SwitchToWide()
    {
        vCamWide.Priority = 20;
        vCamCloseUp.Priority = 10;
    }
}