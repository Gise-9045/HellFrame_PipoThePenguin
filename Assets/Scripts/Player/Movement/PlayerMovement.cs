using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : MonoBehaviour
{
    private Transform tr;
    private Rigidbody rb;
    private Animator anim;

    private PlayerGroundDetection groundDetection;

    [SerializeField] private float velocity;

    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpTimer;
    [SerializeField] private float actualJumpTimer;

    [SerializeField] private float linearDamping;

    private PlayerSlide playerSlide;

    private Vector2 axis;
    
    [Header("Fall")]
    private bool wasGrounded;
    private float airTime;
    private float lastVerticalVelocity;

    [Header("Debug Fall Info")]
    [SerializeField] private float lastImpactSpeed;
    [SerializeField] private float lastAirTime;
    
    public Action OnJump;
    public Action OnLand;
    
    void Start()
    {
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        groundDetection = GetComponentInChildren<PlayerGroundDetection>();

        playerSlide = GetComponent<PlayerSlide>();
        wasGrounded = groundDetection.OnGround();
        airTime = 0f;
        lastVerticalVelocity = 0f;
        lastImpactSpeed = 0f;
        lastAirTime = 0f;
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
        bool isGrounded = groundDetection.OnGround();
        UpdateDamping(isGrounded);
    }

    private void FixedUpdate()
    {
        bool isGrounded = groundDetection.OnGround();

        UpdateGroundState(isGrounded);
        UpdateJumpHold();
        Movement();
    }

    #region Movement & Physics
    
    private void UpdateDamping(bool grounded)
    {
        if (grounded && !playerSlide.isSliding)
        {
            rb.linearDamping = linearDamping;
        }
        else
        {
            rb.linearDamping = 0.0f;
        }
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

            if(!playerSlide.isSliding)
            {
                Vector3 targetXZ = tr.forward.normalized * velocity * axis.magnitude;

                Vector3 target = new Vector3(targetXZ.x, rb.linearVelocity.y, targetXZ.z);

                Vector3 deltaV = target - rb.linearVelocity;

                rb.AddForce(deltaV, ForceMode.VelocityChange);
            }


            anim.SetBool("Walk", true);
            anim.SetFloat("WalkVel", axis.magnitude);
        }
        else
        {
            anim.SetBool("Walk", false);
        }
    }

    #endregion
    
    #region Jump

    private void UpdateJumpHold()
    {
        if (actualJumpTimer <= 0f)
            return;

        actualJumpTimer -= Time.fixedDeltaTime;
        rb.AddForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }
    
    private void Jump(bool button)
    {
        if (groundDetection.OnGround() && button)
        {
            actualJumpTimer = jumpTimer;
            anim.SetBool("Land", false);
            anim.SetTrigger("Jump");
            OnJump?.Invoke();
        }
        else if(!button)
        {
            actualJumpTimer = 0.0f;
        }
    }
    
    #endregion
    
    #region Ground Feedback
    
    private void UpdateGroundState(bool grounded)
    {
        if (!grounded && wasGrounded)
        {
            airTime = 0f;

            // Air Feedback
        }
        
        if (!grounded)
        {
            airTime += Time.fixedDeltaTime;
        }
        
        if (grounded && !wasGrounded)
        {
            float impactSpeed = Mathf.Abs(lastVerticalVelocity);

            lastImpactSpeed = impactSpeed;
            lastAirTime = airTime;

            // Landing feedback
            anim.SetBool("Land", true);
            OnLand?.Invoke();
        }
        wasGrounded = grounded;
        lastVerticalVelocity = rb.linearVelocity.y;
    }
    
    #endregion
}
