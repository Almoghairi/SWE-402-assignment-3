using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
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
        Physics.gravity *= gravityModifier;
        SetPowerup(false);
    }

    private void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        if (focalPoint != null)
        {
            playerRb.AddForce(focalPoint.transform.forward * verticalInput * speed);
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
}
