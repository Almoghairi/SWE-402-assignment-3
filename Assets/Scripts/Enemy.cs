using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    private const float ArenaBodyHeight = 0.86f;
    private const float ArenaRadius = 4.05f;

    public float speed = 4f;
    private Rigidbody enemyRb;
    private GameObject player;

    private void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        enemyRb.linearDamping = 1.2f;
        enemyRb.angularDamping = 0.8f;
        enemyRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        if (IsOverArena() && transform.position.y < ArenaBodyHeight - 0.2f)
        {
            transform.position = new Vector3(transform.position.x, ArenaBodyHeight, transform.position.z);
            enemyRb.linearVelocity = Vector3.zero;
        }
        player = GameObject.Find("Player");
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        Vector3 lookDirection = player.transform.position - transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            enemyRb.AddForce(lookDirection.normalized * speed);
        }
        KeepStableOnArena();
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    private void KeepStableOnArena()
    {
        if (!IsOverArena())
        {
            return;
        }

        Vector3 velocity = enemyRb.linearVelocity;
        transform.position = new Vector3(transform.position.x, ArenaBodyHeight, transform.position.z);
        enemyRb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);
    }

    private bool IsOverArena()
    {
        Vector2 flatPosition = new Vector2(transform.position.x, transform.position.z);
        return flatPosition.magnitude <= ArenaRadius;
    }
}
