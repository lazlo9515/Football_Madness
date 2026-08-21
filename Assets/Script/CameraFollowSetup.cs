using UnityEngine;
using Photon.Pun;
using Unity.Cinemachine; // Use Unity.Cinemachine for Unity 6

public class CameraFollowSetup : MonoBehaviourPun
{
    void Start()
    {
        // ONLY do this if this is MY character
        if (photonView.IsMine)
        {
            // Find the Cinemachine Camera in the scene
            CinemachineCamera vcam = GameObject.FindAnyObjectByType<CinemachineCamera>();

            if (vcam != null)
            {
                // Set the camera to follow and look at THIS transform
                vcam.Target.TrackingTarget = transform;
            }
        }
    }
}