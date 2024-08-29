using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class gun : NetworkBehaviour
{
    public GameObject bullet;
    public GameObject barrel;

    // Start is called before the first frame update
    private void Start()
    {
        barrel = transform.GetChild(0).gameObject;
    }
}
