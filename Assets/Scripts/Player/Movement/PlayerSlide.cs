using UnityEngine;

public class PlayerSlide : MonoBehaviour
{
    private Transform tr;
    private Rigidbody rb;
    private Animator anim;

    private PlayerGroundDetection groundDetection;


    public bool isSliding = false;

    [SerializeField] private float startSlideVel;

    [SerializeField] private float slideTimer;
    [SerializeField] private float actualSlideTimer;

    void Start()
    {
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        groundDetection = GetComponentInChildren<PlayerGroundDetection>();
    }
    private void OnEnable()
    {
        InputManager.Instance.OnDive += Slide;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnDive -= Slide;
    }

    void Update()
    {
        anim.SetBool("Slide", isSliding);

        if (rb.linearVelocity.magnitude < 0.1f && isSliding)
        {
            isSliding = false;
        }
    }

    private void FixedUpdate()
    {
        if(actualSlideTimer > 0)
        {
            actualSlideTimer -= Time.fixedDeltaTime;

            rb.AddForce(tr.forward.normalized * startSlideVel * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    private void Slide(bool button)
    {
        if (button && !isSliding && actualSlideTimer <= 0.0f)
        {
            isSliding = true;

            actualSlideTimer = slideTimer;
        }
        else if (button && isSliding)
        {
            isSliding = false;
        }

    }
}