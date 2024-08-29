using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraRotation : MonoBehaviour
{

    [SerializeField] GameObject player;
    [SerializeField] InputAction moveAction;

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = transform.GetComponent<CinemachineVirtualCamera>().Follow.gameObject;
            moveAction = player.GetComponent<Movement>().rotAction;
        }

        else
        {
            player.transform.rotation = transform.rotation;
            //Vector2 direction = moveAction.ReadValue<Vector2>().normalized;
            //RotateAim(direction);
        }
    }

    private void RotateAim(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
