using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : MonoBehaviour
{
    private Transform tr;
    private Rigidbody rb;
    private Animator anim;

    private PlayerGroundDetection groundDetection;

    private float playerGravity = 9.81f;

    [SerializeField] private float velocity;

    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpTimer;
    [SerializeField] private float actualJumpTimer;

    [SerializeField] private float linearDamping;

    private Vector2 axis;

    void Start()
    {
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        groundDetection = GetComponentInChildren<PlayerGroundDetection>();
    }

    private void OnEnable()
    {
        InputManager.Instance.OnMovement += SetAxis;
        InputManager.Instance.OnJump += Jump;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnMovement -= SetAxis;
        InputManager.Instance.OnJump -= Jump;

    }

    private void SetAxis(Vector2 input) { axis = input; }

    void Update()
    {


        if (groundDetection.OnGround())
        {
            rb.linearDamping = linearDamping;
        }
        else
        {
            rb.linearDamping = 0.0f;
        }
    }

    private void FixedUpdate()
    {
        if (actualJumpTimer > 0)
        {
            actualJumpTimer -= Time.fixedDeltaTime;

            rb.AddForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        Movement();

        //GRAVEDAD
        //Vector3 gravity = playerGravity * rb.mass * Vector3.up;
        //rb.AddForce(-gravity, ForceMode.Acceleration);
    }

    private void Movement()
    {
        if(axis != Vector2.zero)
        {
            float tan;

            tan = Mathf.Atan(axis.x / axis.y) * Mathf.Rad2Deg;

            if (axis.y < 0)
            {
                tan += 180 + Camera.main.transform.rotation.eulerAngles.y;
            }
            else
            {
                tan += Camera.main.transform.rotation.eulerAngles.y;
            }


            tr.rotation = Quaternion.Euler(0, tan, 0);

            Vector3 targetXZ = tr.forward.normalized * velocity * axis.magnitude;

            Vector3 target = new Vector3(targetXZ.x, rb.linearVelocity.y, targetXZ.z);

            Vector3 deltaV = target - rb.linearVelocity;

            rb.AddForce(deltaV, ForceMode.VelocityChange);


            anim.SetBool("Walk", true);
            anim.SetFloat("WalkVel", axis.magnitude);
        }
        else
        {
            anim.SetBool("Walk", false);
        }
    }

    private void Jump(bool jump)
    {
        if (groundDetection.OnGround() && jump)
        {
            actualJumpTimer = jumpTimer;
            anim.SetTrigger("Jump");

        }
        else if(!jump)
        {
            actualJumpTimer = 0.0f;
        }
    }
}
