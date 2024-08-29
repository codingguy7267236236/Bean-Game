using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerJoiner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void StartPlayer()
    {
        if (PlayerData.host == true)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }

        GameObject.Find("lobbyCanvas").transform.GetChild(0).gameObject.SetActive(false);
        GameObject.Find("lobbyCanvas").transform.GetChild(1).gameObject.SetActive(true);
        GameObject.Find("Main Camera").GetComponent<CinemachineBrain>().enabled = true;
    }

    public void HostLobby()
    {
        PlayerData.SetHost(true);
        StartPlayer();
    }

    public void JoinLobby()
    {
        PlayerData.SetHost(false);
        StartPlayer();
    }
}
