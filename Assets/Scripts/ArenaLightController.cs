using UnityEngine;

public class ArenaLightController : MonoBehaviour
{
    public Light dynamicLight;
    public Renderer arenaRenderer;
    public Color earlyWaveColor = new Color(0.08f, 0.2f, 0.35f);
    public Color lateWaveColor = new Color(0.45f, 0.08f, 0.18f);

    private void OnEnable()
    {
        GameManager.OnWaveStarted += OnWaveStarted;
    }

    private void OnDisable()
    {
        GameManager.OnWaveStarted -= OnWaveStarted;
    }

    private void OnWaveStarted(int wave)
    {
        float t = Mathf.Clamp01((wave - 1) / 5f);
        if (dynamicLight != null)
        {
            dynamicLight.intensity = 1.1f + wave * 0.18f;
            dynamicLight.color = Color.Lerp(Color.cyan, new Color(1f, 0.45f, 0.25f), t);
        }
        if (arenaRenderer != null)
        {
            arenaRenderer.material.SetColor("_BaseColor", Color.Lerp(earlyWaveColor, lateWaveColor, t));
        }
    }
}
