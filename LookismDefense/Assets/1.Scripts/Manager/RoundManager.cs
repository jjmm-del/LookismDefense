using UnityEngine;
using TMPro;
using System;
public class RoundManager : MonoBehaviour
{
    // 싱글턴으로 변경
    public static RoundManager Instance { get; private set; }
    
    [Header("Round Settings")]
    [SerializeField] private float roundDuration = 60f; //한 라운드 시간
    [SerializeField] private float gracePeriod = 10f; //게임 시작 시 여유 시간 10초

    private int maxRounds;
    
    [Header("References")]
    [SerializeField] private WaveManager waveManager;

    private int currentRound = 0;
    private float roundTimer = 0f;

    private RoundState currentState = RoundState.Ready;
    private bool isBossRound;
    private float bossTimer;
    public RoundState CurrentState => currentState;

    public event Action<float> OnRoundTimeChanged;
    public event Action<int, int> OnRoundChanged;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void StartGameRounds()
    {
        if(GameManager.Instance?.CurrentDifficulty != null)
        {
            maxRounds = GameManager.Instance.CurrentDifficulty.MaxRounds;
            Debug.Log($"현재 난이도 설정에 따라 {maxRounds}라운드로 설정 되었습니다.");
        }
        else
        {
            maxRounds = 50; //난이도 데이터가 없을 경우 기본값
            Debug.LogWarning("난이도 데이터를 불러 올 수 없어 기본 50라운드로 설정");
        }

        currentRound = 0;
        ChangeState(RoundState.Ready);
        roundTimer = gracePeriod;
    }

    private void Update()
    {
        switch (currentState)
        {
            case RoundState.Ready:
                UpdateReady();
                break;
            case RoundState.Playing:
                UpdatePlaying();
                break;
            case RoundState.Finished:
                break;
        }
    }

    private void ChangeState(RoundState newState)
    {
        if (currentState == newState)
            return;
        
        currentState = newState;
        Debug.Log($"RoundState -> {currentState}");
    }

    private void UpdateReady()
    {
        UpdateTimer();
        if (roundTimer > 0f)
            return;

        StartNextRound();
    }

    private void UpdatePlaying()
    {
        UpdateTimer();
        if (isBossRound)
        {
            UpdateBossTimer();
        }

        if (roundTimer > 0f)
            return;
        
        StartNextRound();
    }

    private void UpdateTimer()
    {
        if (roundTimer <= 0f)
            return;
        //타이머 감소

        roundTimer -= Time.deltaTime;
        roundTimer = Mathf.Max(roundTimer, 0f);
        
        OnRoundTimeChanged?.Invoke(roundTimer);
    }

    private void UpdateBossTimer()
    {
        if (!isBossRound)
            return;

        bossTimer -= Time.deltaTime;

        if (bossTimer > 0f)
            return;

        bossTimer = 0f;
        isBossRound = false;
        
        GameManager.Instance?.TriggerGameOver("보스 제한 시간 초과!");
    }

    private void StartNextRound()
    {
        //이전 라운드에 대한 스토리사 체크 - StoryManager
        if (currentRound > 0)
        {
            StoryManager.Instance?.CheckRoundCondition(currentRound,GameManager.Instance?.CurrentDifficulty);
        }
        
        //마지막 라운드 - 게임 클리어
        if (currentRound >= maxRounds)
        {
            Debug.Log("축하합니다 모든 라운드 클리어!");
            FinishRounds();
            return;
        }

        currentRound++;
        ChangeState(RoundState.Playing);
        GiveRoundReward();
        OnRoundChanged?.Invoke(currentRound, maxRounds);
        Debug.Log($"{currentRound}라운드 시작!");
        
        
        roundTimer = roundDuration;
        
        waveManager.StartWave(currentRound);

        //특정 라운드(예: 10, 20, 30, ...)는 보스 라운드 처리
        isBossRound = currentRound % 10 == 0;
        
        if (isBossRound)
        {
            StartBossRound();
        }
    }

    private void GiveRoundReward()
    {
        CurrencyManager.Instance?.AddCurrency(CurrencyType.RandomCommon, 2);
    }
    private void StartBossRound()
    {
        if (GameManager.Instance.CurrentDifficulty == null)
            return;
        
        isBossRound = true;
        bossTimer = GameManager.Instance.CurrentDifficulty.BossTimeLimit;
        Debug.Log($"보스 라운드 시작! {bossTimer}초 안에 잡으세요");
    }
    
    public void NotifyBossDefeated()
    {
        if (!isBossRound)
            return;

        isBossRound = false;
        bossTimer = 0f;
        
        Debug.Log("보스 처치 성공)");
    }

    private void FinishRounds()
    {
        ChangeState(RoundState.Finished);

        isBossRound = false;
        roundTimer = 0f;
        bossTimer = 0f;
        
        Debug.Log("축하합니다. 모든 라운드 클리어!");
    }
}
