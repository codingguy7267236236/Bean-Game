using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireObject : MonoBehaviour
{
    [SerializeField] int damage = 10;
    private void OnCollisionEnter(Collision collision)
    {
        //seeing if it's a player
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<Movement>().TakeDamage(damage);
        }
    }
}
