using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomiserUI : MonoBehaviour
{
    InputField username;
    private void Start()
    {
        username = GameObject.Find("username").GetComponent<InputField>();
        PlayerData.lobby = null;
    }
    public void HostGame()
    {
        PlayerData.SetHost(true);
    }

    public void SetJoiner()
    {
        PlayerData.SetHost(false);
    }

    public void JoinGame()
    {
        PlayerData.SetUsername(username.text);
        SceneChanger.ChangeScene(1);
    }
}
