using UnityEngine;

public class PlayerGroundDetection : MonoBehaviour
{
    private bool onGround;

    private Rigidbody playerRb;
    [SerializeField] private Transform playerTr;

    [SerializeField] private Vector3 colPos;
    [SerializeField] private Vector3 colSize;



    void Start()
    {
        playerRb = GetComponentInParent<Rigidbody>();
    }


    public bool OnGround()
    {
        return onGround;
    }

    private void Update()
    {
        Collider[] colliders = Physics.OverlapBox(colPos + playerTr.position, colSize, Quaternion.Euler(0, playerTr.transform.rotation.y, 0));

        onGround = false;

        foreach (Collider collider in colliders)
        {
            if (collider.transform.root.name != gameObject.transform.parent.name && !collider.isTrigger)
            {
                onGround = playerRb.linearVelocity.y < 1.0f;
                break;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(colPos + playerTr.position, colSize * 2);
    }
}