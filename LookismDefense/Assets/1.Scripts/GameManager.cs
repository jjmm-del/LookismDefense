using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game Stats")]
    [SerializeField] private int playerLives = 80;
    [SerializeField] private int playerGold = 100;
    
    [Header("References")]
    [SerializeField] private WaveManager waveManager;
    
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        if (waveManager != null)
        {
            waveManager.StartGameLoop();
        }
    }

    public void OnEnemyLeak()
    {
        if (isGameOver) return;
        playerLives--;
        Debug.Log($"적 통과! 남은 목숨{playerLives}");

        if (playerLives <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        Debug.Log("게임 오버!");
        Time.timeScale = 0f;
    }
    
    
}
