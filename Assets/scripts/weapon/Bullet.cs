using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public float speed = 20f;
    private float timeToDestroy = 3f;
    public int damage = 10;

    private Rigidbody rb;

    public Vector3 target { get; set; }
    public bool hit { get; set; }


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.velocity = transform.forward * speed;
    }
    // Start is called before the first frame update
    private void OnEnable()
    {
        Invoke(nameof(DestroyBulletServerRpc), timeToDestroy);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        //if (!hit && Vector3.Distance(transform.position,target) < .01f)
        //{
            //DestroyBulletServerRpc();
        //}
    }
    private void OnCollisionEnter (Collision collision)
    {
        //seeing if it's a player
        if(collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<Movement>().TakeDamage(damage);
        }
        //Debug.Log(collision.gameObject.name);
        DestroyBulletServerRpc();
    }

    [ServerRpc]
    private void DestroyBulletServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
        DestroyBulletsOnAllClientRpc();
    }

    [ClientRpc]
    private void DestroyBulletsOnAllClientRpc()
    {
        Destroy(gameObject);
    }
}
