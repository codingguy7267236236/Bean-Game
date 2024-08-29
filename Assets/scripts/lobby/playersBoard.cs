using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;

public class playersBoard : MonoBehaviour
{
    private GameObject playersHolder;


    private void Start()
    {
        playersHolder = transform.GetChild(0).gameObject;
    }

    public void DisplayPlayers(Lobby lobby)
    {
        playersHolder = transform.GetChild(0).gameObject;
        foreach (Transform t in playersHolder.transform)
        {
            Destroy(t.gameObject);
        }

        Debug.Log("Players: "+lobby.Players.Count);
        
        // adding display names
        foreach(Player player in lobby.Players)
        {
            Debug.Log("Player name: "+player.Data["Name"].Value);
            GameObject txt = new GameObject("Player");
            txt.AddComponent<Text>();
            txt.transform.SetParent(playersHolder.transform);

            Text text = txt.GetComponent<Text>();
            text.text = player.Data["Name"].Value;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 40;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
