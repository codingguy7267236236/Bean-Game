using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class joiner : MonoBehaviour
{
    GameObject cam;
    [SerializeField] GameObject ui;
    GameObject holder;
    [SerializeField] List<GameObject> models;
    [SerializeField] List<GameObject> cameras;

    [SerializeField] private GameObject mod;

    private void Start()
    {
        cam = GameObject.Find("Main Camera");
        holder = transform.GetChild(0).gameObject;
        mod = holder.transform.GetChild(0).gameObject;
    }

    public void SelectCharacter(int num)
    {
        foreach(Transform t in holder.transform)
        {
            Destroy(t.gameObject);
        }

        mod = Instantiate(models[num], holder.transform);
        holder.GetComponent<ObjectCustomiser>().UpdateCostume(PlayerData.costume);
        PlayerData.SetModel(num);

        holder.GetComponent<ObjectCustomiser>().SetModel(mod);
    }

    public void SelectCostume(int num)
    {
        PlayerData.costume = num;
        holder.GetComponent<ObjectCustomiser>().UpdateCostume(PlayerData.costume);
    }

    public void TurnOff()
    {
        ui.SetActive(false);

        cam.GetComponent<CinemachineBrain>().enabled = true;

        foreach(GameObject camm in cameras)
        {
            camm.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    public void HostGame()
    {
        TurnOff();
        NetworkManager.Singleton.StartHost();
    }

    public void JoinGame()
    {
        TurnOff();
        NetworkManager.Singleton.StartClient();
    }
}
