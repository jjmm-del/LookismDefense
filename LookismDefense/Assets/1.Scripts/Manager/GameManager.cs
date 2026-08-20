using UnityEngine;
using System.Collections.Generic;
using System;
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
    public event Action<string> OnGameOver;
    

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

        
    }

    public void StartGame(int difficultyIndex)
    {
        if (IsGameStarted)
            return;
        
        
        // [난이도 세팅]
        SetDifficulty(difficultyIndex);
        InitializeResources();
        
        // 게임 시작 플래그 ON
        IsGameStarted = true;

        
        RoundManager.Instance?.StartGameRounds();
        StoryManager.Instance?.StartStory();
        
        Debug.Log($"[{difficultyPresets[difficultyIndex].name}] 난이도로 게임이 시작 되었습니다.");
    }

    private void InitializeResources()
    {
        if (CurrencyManager.Instance == null)
            return;
        
        CurrencyManager.Instance.AddCurrency(CurrencyType.Gold, startGold);
        CurrencyManager.Instance.AddCurrency(CurrencyType.RandomCommon, startChoice);
        CurrencyManager.Instance.AddCurrency(CurrencyType.SelectCommon, 1);
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
    
    
    public void TriggerGameOver(string reason)
    {
        if (!IsGameStarted)
            return;
        IsGameStarted = false;
        
        Debug.LogError("GameOver"+reason);
        
        Time.timeScale = 0;//게임 정지

        OnGameOver?.Invoke(reason);
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
