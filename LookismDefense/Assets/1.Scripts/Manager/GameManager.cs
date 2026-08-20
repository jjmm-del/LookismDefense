using UnityEngine;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    public bool IsGameStarted { get; private set; } = false;
    [Header("Settings")]
    [SerializeField] private DifficultyData[] difficultyPresets; //에디터에서 Easy, Normal, Hard
    
    // 게임 시스템 변수
    [Header("Debug/Resources")]
    [SerializeField] private int startGold = 500;   // 초기 골드 (난이도 별로 다르게 할 수도 있음)
    [SerializeField] private int startChoice = 10; //랜덤 흔함
    
    //현재 적용된 난이도 (외부에서는 프로퍼티로 정보 가져간다)
    private DifficultyData currentDifficulty;
    public DifficultyData CurrentDifficulty => currentDifficulty;
    
    // [신규] 외부(UI)에서 난이도 목록을 읽어갈 수 있게 열어주는 프로퍼티
    public DifficultyData[] DifficultyPresets => difficultyPresets;
    
    //보스전 관련
    private bool isBossRound = false;
    private float bossTimer = 0f;

    //스토리 관련(예시:1단계부터 시작)
    private int currentStoryStep = 1;
    
    

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (EntityRegistry.Instance != null)
        {
            EntityRegistry.Instance.OnEnemyCountChanged += HandleEnemyCountChanged;
        }
        StartGame(SessionManager.SelectedDifficultyIndex);

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCurrency(CurrencyType.Gold, startGold);
            CurrencyManager.Instance.AddCurrency(CurrencyType.RandomCommon, startChoice);
        }
    }

    public void StartGame(int difficultyIndex)
    {
        if (IsGameStarted)
        {
            return;
        }
        
        // [난이도 세팅]
        SetDifficulty(difficultyIndex);
        // 게임 시작 플래그 ON
        IsGameStarted = true;
        Debug.Log($"[{difficultyPresets[difficultyIndex].name}] 난이도로 게임이 시작 되었습니다.");

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.StartGameRounds();
        }

        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.StartStory();
        }
        

    }
    private void Update()
    {
        //보스 라운드 일 때만 타이머 작동-> 수정 예정 모든 라운드 시간 체크 
        if (isBossRound)
        {
            bossTimer -= Time.deltaTime;

            //보스사 체크
            if (bossTimer <= 0)
            {
                TriggerGameOver("보스 제한 시간 초과!(보스사");
            }
        }
    }
    public void SetDifficulty(int index)
    {
        if (index >= 0 && index < difficultyPresets.Length)
        {
            currentDifficulty = difficultyPresets[index];
            Debug.Log($"난이도가 {currentDifficulty.name}로 설정됨");
            
            //난이도 변경시 초기 골드나 라이프 설정 로직 추가 가능
        }
    }

    private void HandleEnemyCountChanged(int currentCount)
    {
        if (currentDifficulty == null)
            return;

        int maxCount = currentDifficulty.MaxUnitCountLimits;

        if (currentCount >= maxCount)
        {
            TriggerGameOver($"라인 유닛 수 초과! [{currentCount}/{maxCount}]-라인사");
        }
    }
    
    // --- 스토리사 관련 ---
    public void CheckStoryCondition(int currentRound)
    {
        if (currentRound == 40)
        {
            if (currentStoryStep < currentDifficulty.StoryLimit) //예: 현재 스토리 3 < 스토리 제한 
            {
                TriggerGameOver("정해진 라운드 내에 스토리 클리어 실패!");
            }
        }
    }

    
    // --- 3. 보스전 관리 ---
    public void StartBossRound()
    {
        isBossRound = true;
        bossTimer = currentDifficulty.BossTimeLimit;
        Debug.Log($"보스 라운드 시작! {bossTimer}초 안에 잡으세요");
    }

    public void BossDefeated()
    {
        isBossRound = false;
        Debug.Log("보스 처치 성공!");
    }

    public void TriggerGameOver(string reason)
    {
        Debug.LogError("GameOver"+reason);
        Time.timeScale = 0;//게임 정지
        
        //여기에 GameOverUI 팝업 띄우는 로직 추가
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverPanel();
        }
    }

    protected override void OnDestroy()
    {
        if (EntityRegistry.Instance != null)
        {
            EntityRegistry.Instance.OnEnemyCountChanged -= HandleEnemyCountChanged;
        }
        base.OnDestroy();
    }
}
