using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicGameOverResponder : MonoBehaviour
{
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        GameManager.OnGameOver += StopMusic;
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= StopMusic;
    }

    private void StopMusic()
    {
        source.Stop();
    }
}
