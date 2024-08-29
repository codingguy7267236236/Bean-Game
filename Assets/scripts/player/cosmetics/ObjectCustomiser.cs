using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ObjectCustomiser : NetworkBehaviour
{
    private NetworkVariable<int> costume = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private int acc = 0;
    GameObject cost;
    public GameObject model;
    private void Start()
    {
        model = transform.GetChild(0).gameObject;
        CheckAccessory();
    }

    private void Update()
    {
        if (acc != costume.Value)
        {
            UpdateCostume(costume.Value);
            acc = costume.Value;
        }
    }

    public void SetModel(GameObject mod)
    {
        model = mod;
    }

    public void UpdateCostume(int num)
    {
        //if costume assigned and needs updating old one is destoryed
        if(cost != null)
        {
            Destroy(cost);
        }
        //Debug.Log(costume.Value);
        //checking if 0 then no costume needed
        if (num == 0) return;

        //instatiating and adding costume model
        cost = Instantiate(Resources.Load<GameObject>($"prefabs/cosmetics/{num}"),model.transform);
    }

    public void CheckAccessory()
    {
        //if is owner set the network variable
        if (IsOwner)
        {
            costume.Value = PlayerData.costume;
            //Debug.Log(PlayerData.costume+" "+costume.Value);
        }
        UpdateCostume(costume.Value);
    }
}
