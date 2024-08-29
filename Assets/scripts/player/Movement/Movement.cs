using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.InputSystem;
using System;
using Random = UnityEngine.Random;

public class Movement : NetworkBehaviour
{
    //input fields
    [SerializeField] private PlayerInputActions paa;
    private InputAction move;
    public InputAction rotAction;
    InputAction jumpAction;

    public GameObject bullet;
    [SerializeField] LayerMask aimColliderMask;

    //movement fields
    private Rigidbody rb;
    [SerializeField] private float movementForce = 1f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float jumpSpeed = 10f;
    private Vector3 forceDirection = Vector3.zero;

    [SerializeField] private Camera cam;
    private GameObject freelookcam;
    private PlayerInput pi;
    private Animator animator;

    public bool aim=false;

    public float turnSmoothTime = 0.1f;
    public float groundDistance = 0.4f;
    public LayerMask groundmask;
    public Transform groundCheck;

    private int modd = 0;
    private int an = 0;

    [SerializeField] private int health=100;

    private GameObject hand;
    private GameObject model;
    [SerializeField] List<GameObject> weapons;
    [SerializeField] List<GameObject> models;

    [SerializeField] private GameObject inHand;

    //ui canvas reticles
    [SerializeField] private GameObject reticles;
    public HealthBar hb;

    private NetworkVariable<int> handItemNW = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> mod = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> anim = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> hp = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //camera mouse variables
    Ray mouse;
    Vector3 mousePos = Vector3.zero;

    private void Awake()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        gameObject.name = $"Player {OwnerClientId}";
        base.OnNetworkSpawn();
        SpawnPoint();
    }

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        //turning on cosmetics
        ObjectCustomiser oc = this.GetComponent<ObjectCustomiser>();
        oc.enabled = true;

        if (!IsOwner)
        {
            CheckCharacter();
            return;
        }

        mod.Value = PlayerData.model;
        CheckCharacter();

        pi = this.GetComponent<PlayerInput>();
        move = pi.actions.FindAction("Move");
        rotAction = pi.actions.FindAction("Look");
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        freelookcam = GameObject.Find("FreeLook Camera");
        freelookcam.GetComponent<CinemachineVirtualCamera>().Follow = gameObject.transform;

        //getting reticles
        reticles = GameObject.Find("playerUI");
        reticles.transform.GetChild(1).gameObject.SetActive(true);
        reticles.transform.GetChild(2).gameObject.SetActive(true);

        //healthbar
        hb = reticles.transform.GetChild(2).GetComponent<HealthBar>();
        SetHealth();

    }

    private void CheckCharacter()
    {
        //checking if any objects in holder
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        model = Instantiate(models[mod.Value], transform);
        model.name = $"Player {OwnerClientId+1}";
        //getting hand and adding weapon
        hand = model.transform.Find("hand").gameObject;
        inHand = Instantiate(weapons[handItemNW.Value], hand.transform);
        //animation
        animator = model.GetComponent<Animator>();

    }

    private void FixedUpdate()
    {
        //checking if player dead or not
        if (anim.Value == 2) return;

        //if not dead checking if owner
        if (!IsOwner)
        {
            //checking if the model has changed
            if (modd != mod.Value)
            {
                modd = mod.Value;
                CheckCharacter();
            }

            //checking animation
            if (anim.Value == 1)
            {
               animator.SetBool("walking", true);
            }
            else
            {
                animator.SetBool("walking", false);
            }
            return;
        }

        //checking movement
        CheckMove();
        //Move(move.ReadValue<Vector2>());


        //updating mouse position
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        mouse = Camera.main.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(mouse, out RaycastHit raycasthit, 999f, aimColliderMask))
        {
            mousePos = raycasthit.point;
        }

        //if aim cam on
        if (aim == true)
        {
            //Vector3 aimTarget = mousePos;
            //aimTarget.y = transform.position.y;
            //Vector3 aimDir = (aimTarget - transform.position).normalized;

            //transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.fixedDeltaTime + 20f);
            transform.rotation = cam.transform.rotation;
        }
        transform.rotation = cam.transform.rotation;
        LookAt();
        an = anim.Value;
    }

    private void LookAt()
    {
        Vector3 direction = rb.velocity;
        direction.y = 0f;


        if(move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
        {
            this.rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    private Vector3 GetCameraForward(Camera cam)
    {
        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera cam)
    {
        Vector3 right = cam.transform.right;
        right.y = 0;
        return right.normalized;
    }

    public void DoJump(InputAction.CallbackContext obj)
    {
        if (IsGrounded())
        {
            forceDirection += Vector3.up * jumpSpeed;
            //rb.AddForce(Vector3.up * jumpSpeed * Time.fixedDeltaTime);
        }
    }

    private bool IsGrounded()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up * 0.25f, Vector3.down);
        if(Physics.Raycast(ray,out RaycastHit hit,3))
        {
            return true;
        }
        return false;
    }

    public void Focus(InputAction.CallbackContext context)
    {
        
        if (!IsOwner) return;

        GameObject ret = reticles;
        if (context.performed)
        {
            if (aim == false)
            {
                ret.transform.GetChild(0).gameObject.SetActive(true);
                ret.transform.GetChild(1).gameObject.SetActive(false);
                aim = true;
            }

            else
            {
                ret.transform.GetChild(1).gameObject.SetActive(true);
                ret.transform.GetChild(0).gameObject.SetActive(false);
                aim = false;
            }
        }
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            gun gn = inHand.GetComponent<gun>();
            Vector3 bullDir = (mousePos - gn.barrel.transform.position).normalized;
            ShootServerRpc(gn.barrel.transform.position,Quaternion.LookRotation(bullDir,Vector3.up));
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Quaternion rot)
    {
        GameObject bull = Instantiate(bullet, pos, rot);
        bull.GetComponent<NetworkObject>().Spawn();
    }

    private void SetHealth()
    {
        hb.SetMax(health);
        hp.Value = health;
    }

    public void TakeDamage(int damage)
    {
        hb.SetHealth(hp.Value - damage);
        
        if(hp.Value - damage <= 0)
        {
            hp.Value = 0;
            Dead();
            return;
        }

        hp.Value -= damage;
    }

    private void Dead()
    {
        if (isDead.Value == true) return;

        if (IsOwner)
        {
            isDead.Value = true;
            anim.Value = 2;
        }
        DeadServerRpc();
        //animator.SetTrigger("dead");
        Invoke(nameof(Respawn), 5f);

    }

    [ClientRpc]
    private void DeadClientRpc()
    {
        animator.SetTrigger("dead");
    }

    [ServerRpc]
    private void DeadServerRpc()
    {
        animator.SetTrigger("dead");
        DeadClientRpc();
    }

    private void Respawn()
    {
        animator.ResetTrigger("dead");
        if (IsOwner)
        {
            hp.Value = health;
            hb.SetHealth(hp.Value);
            isDead.Value = false;
            anim.Value = 0;
            SpawnPoint();
        }
    }


    private void SpawnPoint()
    {
        GameObject spawns = GameObject.Find("spawns");
        int num = spawns.transform.childCount;
        int ind = Random.Range(0, num);
        GameObject spawn = spawns.transform.GetChild(ind).gameObject;
        transform.position = spawn.transform.position;
    }

    private void CheckMove()
    {
        if(IsServer && IsLocalPlayer)
        {
            //Move(move.ReadValue<Vector2>());
            MoveServerRpc(move.ReadValue<Vector2>());
        }

        else if(IsClient && IsLocalPlayer)
        {
            MoveServerRpc(move.ReadValue<Vector2>());
        }
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 position)
    {
        //Debug.Log(position);
        UpdateMoveClientRpc(position);
    }

    [ClientRpc]
    private void UpdateMoveClientRpc(Vector2 position)
    {
        Move(position);
    }

    private void Move(Vector2 position)
    {
        //Debug.Log(move.ReadValue<Vector2>());
        forceDirection += position.x * GetCameraRight(cam) * movementForce;
        forceDirection += position.y * GetCameraForward(cam) * movementForce;

        if (position == new Vector2(0, 0))
        {
            animator.SetBool("walking", false);
            anim.Value = 0;
        }
        else
        {
            animator.SetBool("walking", true);
            anim.Value = 1;
        }

        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        //smooth fall so its realistic
        if (rb.velocity.y < 0f)
        {
            rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;
        }

        //cap speed
        Vector3 horizontalVel = rb.velocity;
        horizontalVel.y = 0;
        if (horizontalVel.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.velocity = horizontalVel.normalized * maxSpeed + Vector3.up * rb.velocity.y;
        }
    }
}
