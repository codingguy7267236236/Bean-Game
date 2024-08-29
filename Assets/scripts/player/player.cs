using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.InputSystem;

public class player : NetworkBehaviour
{
    private CharacterController controller;

    //player variables
    [SerializeField] private int speed = 5;
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private float mass = 10f;

    public Transform cam;
    private Animator animator;
    private Rigidbody rb;
    [SerializeField] private GameObject hand;

    //hand item related
    [SerializeField] private List<GameObject> weapons;
    [SerializeField] private int weapIndex=0;
    [SerializeField] private GameObject inHand;

    public bool aim;
    bool isGrounded=true;

    private GameObject freelookcam;

    [SerializeField] private GameObject model;
    private int modd;

    //ui canvas reticles
    [SerializeField] private GameObject reticles;

    Vector3 mov;


    //network variables
    private NetworkVariable<int> anim = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> mod = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> handItemNW = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    public float groundDistance = 0.4f;
    public LayerMask groundmask;
    public Transform groundCheck;

    [SerializeField] PlayerInput pi;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction shootAction;
    public InputAction rotAction;

    // Start is called before the first frame update
    void Start()
    {

        //checking if owner
        if (!IsOwner) {
            CheckPlayerModel();
            return;
        };

        mod.Value = PlayerData.model;
        Debug.Log(mod.Value);
        handItemNW.Value = weapIndex;
        CheckPlayerModel();

        //getting input stuff
        pi = GetComponent<PlayerInput>();
        moveAction = pi.actions.FindAction("Move");
        jumpAction = pi.actions.FindAction("Jump");
        rotAction = pi.actions.FindAction("Look");
        shootAction = pi.actions.FindAction("Shoot");
        pi.camera = GameObject.Find("Main Camera").GetComponent<Camera>();

        rb = transform.GetComponent<Rigidbody>();

        //getting reticles
        reticles = GameObject.Find("playerUI");

        //getting character controller
        controller = transform.GetComponent<CharacterController>();

        cam = GameObject.Find("Main Camera").transform;
        freelookcam = GameObject.Find("FreeLook Camera");
        freelookcam.GetComponent<CinemachineVirtualCamera>().Follow = gameObject.transform;
        //freelookcam.GetComponent<CinemachineFreeLook>().LookAt = gameObject.transform;
    }

    void CheckPlayerModel()
    {
        //deleting existing objects in transform
        foreach(Transform g in transform.GetChild(0))
        {
            Destroy(g.gameObject);
        }

        //Debug.Log(OwnerClientId + " Model: " + mod.Value);
        model = transform.GetComponent<PlayerCosmetics>().GetModel(mod.Value);
        model.GetComponent<NetworkObject>().Spawn();
        animator = model.GetComponent<Animator>();

        //getting hand and adding weapon
        hand = model.transform.Find("hand").gameObject;
        inHand = Instantiate(weapons[handItemNW.Value], hand.transform);
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsOwner)
        {
            //checking if the model has changed
            if(modd != mod.Value)
            {
                modd = mod.Value;
                CheckPlayerModel();
            }

            //checking animation
            if(anim.Value == 1)
            {
                animator.SetBool("walking", true);
            }
            else
            {
                animator.SetBool("walking", false);
            }
            return;
        }

        Jump();
        MovePlayer();
        PlayerRotation();
    }

    private void PlayerRotation()
    {
        if (!IsOwner) return;
        var camRot = cam.transform.rotation;
        camRot.x = 0;
        //camRot.z = 0;
        transform.rotation = camRot;
    }

    public void Focus(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        GameObject ret = reticles.transform.GetChild(0).gameObject;

        if (context.performed)
        {
            if (aim == false)
            {
                ret.SetActive(true);
                aim = true;
            }

            else
            {
                ret.SetActive(false);
                aim = false;
            }
        }
    }

    public void MovePlayer()
    {
        if (!IsOwner) return;

        Vector2 direction = moveAction.ReadValue<Vector2>().normalized;

        if(direction.magnitude >= 0.1)
        {
            animator.SetBool("walking", true);
            anim.Value = 1;

            float targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            //controller.Move(moveDir.normalized * speed * Time.deltaTime);
            transform.position += moveDir.normalized * speed * Time.deltaTime;
            //rb.velocity = moveDir*speed*Time.fixedDeltaTime;
        }
        else
        {
            animator.SetBool("walking", false);
            anim.Value = 0;
        }

    }


    public void Jump()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundmask);
        float jum = jumpAction.ReadValue<float>();

        float gravity = Physics.gravity.y;

        mov.y += gravity * 5f * Time.fixedDeltaTime;

        if (isGrounded)
        {
            mov.y = 0f;
        }

        if (isGrounded && mov.y < 0)
        {
            mov.y = -2f;
        }

        if (isGrounded && jum == 1)
        {
            mov.y = jumpSpeed;
        }


        //controller.Move(mov.normalized * gravity * Time.deltaTime);
        transform.Translate(mov*Time.fixedDeltaTime);
        //rb.AddForce(Vector3.up * jumpSpeed * Time.fixedDeltaTime);

    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            RaycastHit hit;

            gun gn = inHand.GetComponent<gun>();
            GameObject bull = Instantiate(gn.bullet, gn.barrel.transform.position,gn.barrel.transform.rotation);
            Bullet b = bull.GetComponent<Bullet>();

            if (Physics.Raycast(cam.position, cam.forward, out hit, Mathf.Infinity))
            {
                b.target = hit.point;
                b.hit = true;
            }
            else
            {
                b.target = cam.position + cam.forward * 25f;
                b.hit = false;
            }
        }
    }
}
