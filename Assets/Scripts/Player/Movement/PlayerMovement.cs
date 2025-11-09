using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : MonoBehaviour
{
    private Transform tr;
    private Rigidbody rb;
    private Animator anim;

    [SerializeField] private float velocity;
    [SerializeField] private float fallVel;


    void Start()
    {
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        InputManager.Instance.OnMovement += Movement;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnMovement -= Movement;
    }

    void Update()
    {
        
    }

    private void Movement(Vector2 axis)
    {
        if(axis != Vector2.zero)
        {
            float tan;

            tan = (Mathf.Atan(axis.x / axis.y) * Mathf.Rad2Deg);

            if (axis.y < 0)
            {
                tan += 180 + Camera.main.transform.rotation.eulerAngles.y;
            }
            else
            {
                tan += Camera.main.transform.rotation.eulerAngles.y;
            }


            tr.rotation = Quaternion.Euler(0, tan, 0);


            rb.linearVelocity = (tr.forward.normalized * velocity) + (Vector3.up * rb.linearVelocity.y);

            anim.SetBool("Walk", true);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

            anim.SetBool("Walk", false);
        }
    }
}
