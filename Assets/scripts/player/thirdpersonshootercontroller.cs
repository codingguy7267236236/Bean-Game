using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class thirdpersonshootercontroller : NetworkBehaviour
{
    [SerializeField] private CinemachineVirtualCamera aimCam;
    private Movement ply;

    private void Start()
    {
        if (!IsOwner) return;

        ply = transform.GetComponent<Movement>();
        aimCam = GameObject.Find("aimCam").GetComponent<CinemachineVirtualCamera>();
        aimCam.Follow = transform;
        aimCam.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (ply.aim == true)
        {
            aimCam.gameObject.SetActive(true);
        }
        else
        {
            aimCam.gameObject.SetActive(false);
        }
    }
}
