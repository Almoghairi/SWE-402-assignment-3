using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    public float speed = 4f;
    private Rigidbody enemyRb;
    private GameObject player;

    private void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
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
        lookDirection.Normalize();
        enemyRb.AddForce(lookDirection * speed);
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }
}
