using System.Collections;
using UnityEngine;

public class CameraPolish : MonoBehaviour
{
    public PlayerController player;
    public SpawnManager spawnManager;
    public float baseFov = 52f;
    public float zoomPerWave = 1.1f;
    public float poweredFovBonus = 7f;
    private Camera cam;
    private Vector3 defaultLocalPosition;
    private float shakeTime;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        defaultLocalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        GameManager.OnWaveStarted += OnWaveStarted;
    }

    private void OnDisable()
    {
        GameManager.OnWaveStarted -= OnWaveStarted;
    }

    private void LateUpdate()
    {
        float targetFov = baseFov;
        if (spawnManager != null)
        {
            targetFov += spawnManager.waveNumber * zoomPerWave;
        }
        if (player != null && player.HasPowerup)
        {
            targetFov += poweredFovBonus;
        }
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * 4f);

        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            transform.localPosition = defaultLocalPosition + Random.insideUnitSphere * 0.14f;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, defaultLocalPosition, Time.deltaTime * 8f);
        }
    }

    private void OnWaveStarted(int wave)
    {
        StartCoroutine(WaveIntroMove());
    }

    public void Shake()
    {
        shakeTime = 0.18f;
    }

    private IEnumerator WaveIntroMove()
    {
        Vector3 start = defaultLocalPosition + Vector3.back * 1.2f;
        float elapsed = 0f;
        while (elapsed < 0.6f)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(start, defaultLocalPosition, elapsed / 0.6f);
            yield return null;
        }
    }
}
