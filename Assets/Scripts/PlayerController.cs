using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    private const float ArenaBodyHeight = 0.86f;
    private const float ArenaRadius = 4.05f;

    public float speed = 9f;
    public float gravityModifier = 1.8f;
    public float powerupStrength = 18f;
    public float powerupDuration = 7f;
    public GameObject powerupIndicator;
    public ParticleSystem collectParticles;
    public ParticleSystem knockoutParticles;
    public ParticleSystem moveTrail;
    public AudioClip powerupClip;
    public AudioClip hitClip;
    public Material normalMaterial;
    public Material poweredMaterial;

    private Rigidbody playerRb;
    private GameObject focalPoint;
    private AudioSource audioSource;
    private Renderer playerRenderer;
    private bool hasPowerup;

    public bool HasPowerup => hasPowerup;

    private void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        playerRenderer = GetComponent<Renderer>();
        focalPoint = GameObject.Find("Focal Point");
        Physics.gravity = Vector3.down * 9.81f * gravityModifier;
        playerRb.linearDamping = 1.2f;
        playerRb.angularDamping = 0.8f;
        playerRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        playerRb.position = new Vector3(playerRb.position.x, ArenaBodyHeight, playerRb.position.z);
        playerRb.linearVelocity = Vector3.zero;
        DisableVisualColliders(powerupIndicator);
        DisableVisualColliders(GameObject.Find("Emissive Arena Edge"));
        SetPowerup(false);
    }

    private void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        if (focalPoint != null)
        {
            Vector3 moveDirection = focalPoint.transform.forward;
            moveDirection.y = 0f;
            moveDirection.Normalize();
            playerRb.AddForce(moveDirection * verticalInput * speed);
        }

        if (moveTrail != null)
        {
            bool moving = Mathf.Abs(verticalInput) > 0.05f || playerRb.linearVelocity.magnitude > 0.75f;
            if (moving && !moveTrail.isPlaying) moveTrail.Play();
            if (!moving && moveTrail.isPlaying) moveTrail.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Powerup"))
        {
            return;
        }

        Destroy(other.gameObject);
        SetPowerup(true);
        if (collectParticles != null)
        {
            collectParticles.transform.position = transform.position;
            collectParticles.Play();
        }
        if (audioSource != null && powerupClip != null)
        {
            audioSource.PlayOneShot(powerupClip, 0.75f);
        }
        StartCoroutine(PowerupCountdownRoutine());
    }

    private void FixedUpdate()
    {
        if (IsOverArena())
        {
            Vector3 velocity = playerRb.linearVelocity;
            if (transform.position.y > ArenaBodyHeight + 0.35f && velocity.y > 0f)
            {
                playerRb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);
            }
            else if (transform.position.y < ArenaBodyHeight - 0.45f)
            {
                playerRb.position = new Vector3(transform.position.x, ArenaBodyHeight, transform.position.z);
                playerRb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);
            }
        }
    }

    private IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(powerupDuration);
        SetPowerup(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasPowerup || !collision.gameObject.CompareTag("Enemy"))
        {
            return;
        }

        Rigidbody enemyRigidbody = collision.gameObject.GetComponent<Rigidbody>();
        if (enemyRigidbody != null)
        {
            Vector3 awayFromPlayer = collision.gameObject.transform.position - transform.position;
            awayFromPlayer.y = 0f;
            enemyRigidbody.AddForce(awayFromPlayer.normalized * powerupStrength, ForceMode.Impulse);
        }

        if (knockoutParticles != null)
        {
            knockoutParticles.transform.position = collision.contacts[0].point;
            knockoutParticles.Play();
        }
        if (audioSource != null && hitClip != null)
        {
            audioSource.PlayOneShot(hitClip, 0.65f);
        }
    }

    private void SetPowerup(bool active)
    {
        hasPowerup = active;
        if (powerupIndicator != null)
        {
            powerupIndicator.SetActive(active);
            Animator animator = powerupIndicator.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("Powered", active);
            }
        }

        if (playerRenderer != null)
        {
            playerRenderer.sharedMaterial = active && poweredMaterial != null ? poweredMaterial : normalMaterial;
        }
    }

    private bool IsOverArena()
    {
        Vector2 flatPosition = new Vector2(transform.position.x, transform.position.z);
        return flatPosition.magnitude <= ArenaRadius;
    }

    private static void DisableVisualColliders(GameObject visualObject)
    {
        if (visualObject == null)
        {
            return;
        }

        foreach (Collider collider in visualObject.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }
    }
}
