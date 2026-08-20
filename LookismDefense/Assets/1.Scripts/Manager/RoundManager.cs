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
    private bool isGameRunning = true;
    private bool isGracePeriod = false;
    private bool isBossRound;
    private float bossTimer;

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
        if(GameManager.Instance != null && GameManager.Instance.CurrentDifficulty != null)
        {
            maxRounds = GameManager.Instance.CurrentDifficulty.MaxRounds;
            Debug.Log($"현재 난이도 설정에 따라 {maxRounds}라운드로 설정 되었습니다.");
        }
        else
        {
            maxRounds = 50; //난이도 데이터가 없을 경우 기본값
            Debug.LogWarning("난이도 데이터를 불러 올 수 없어 기본 50라운드로 설정");
        }

        isGracePeriod = true;
        roundTimer = gracePeriod;
        isGameRunning = true;

        // if (UIManager.Instance != null)
        // {
        //     UIManager.Instance.UpdateWaveName("게임 시작 대기 중...");
        // }
        
    }

    private void Update()
    {
        if (!isGameRunning)
            return;

        UpdateRoundTimer();

    }

    private void UpdateRoundTimer()
    {
        //타이머 감소
        if (roundTimer > 0)
        {
            roundTimer -= Time.deltaTime;
            
            //시간이 다 되면 다음 라운드
            OnRoundTimeChanged?.Invoke(Mathf.Max(roundTimer, 0f));
            
            if (roundTimer <= 0)
            { 
                if (isGracePeriod)
                {
                    isGracePeriod = false;
                    StartNextRound();
                }
                else
                {
                    StartNextRound();
                }
            }
            
        }
    }

    private void UpdateBossTimer()
    {
        if (!isBossRound)
            return;

        bossTimer -= Time.deltaTime;

        if (bossTimer < 0f)
            return;

        isBossRound = false;
        
        GameManager.Instance?.TriggerGameOver("보스 제한 시간 초과!");
    }

    private void StartNextRound()
    {
        //이전 라운드에 대한 스토리사 체크(GameManager에 위임)
        if (currentRound > 0)
        {
            StoryManager.Instance?.CheckRoundCondition(currentRound,GameManager.Instance?.CurrentDifficulty);
        }
        currentRound++;
        
        //라운드 클리어 보상 : 랜덤 흔함 위습 5개
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCurrency(CurrencyType.RandomCommon, 2);
        }

        if (currentRound > maxRounds)
        {
            Debug.Log("축하합니다 모든 라운드 클리어!");
            isGameRunning = false;
            return;
        }
        OnRoundChanged?.Invoke(currentRound, maxRounds);
        Debug.Log($"{currentRound}라운드 시작!");
        
        
        roundTimer = roundDuration;
        
        //WaveManager에게 현재 라운드에 맞는 적 소환 요청
        //(WaveManager의 SpawnWaveRoutine을 수정하건, 여기서 직접 함수를 호출해야 함
        waveManager.StartWave(currentRound);
        //UIManager.Instance.UpdateWaveName(currentRound.ToString());
        
        //특정 라운드(예: 10, 20, 30, ...)는 보스 라운드 처리
        if (currentRound % 10 == 0)
        {
            StartBossRound();
        }
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
}
