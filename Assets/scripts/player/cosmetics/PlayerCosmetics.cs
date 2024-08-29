using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

public class PlayerCosmetics : MonoBehaviour
{
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private List<GameObject> accessories;

    private void Start()
    {
        accessories = Resources.LoadAll("prefabs/cosmetics", typeof(GameObject)).Cast<GameObject>().ToList();
    }
    public GameObject GetModel(int index)
    {
        GameObject model = Instantiate(accessories[index], transform.GetChild(0));
        return model;
    }
}
