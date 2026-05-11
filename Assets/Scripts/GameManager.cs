using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event Action OnGameOver;
    public static event Action<int> OnWaveStarted;

    public Transform player;
    public float loseHeight = -10f;
    private bool gameOver;

    private void Update()
    {
        if (!gameOver && player != null && player.position.y < loseHeight)
        {
            gameOver = true;
            OnGameOver?.Invoke();
        }
    }

    public static void RaiseWaveStarted(int waveNumber)
    {
        OnWaveStarted?.Invoke(waveNumber);
    }
}
