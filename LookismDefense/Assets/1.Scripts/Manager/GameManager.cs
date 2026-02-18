using UnityEngine;
using System.Collections.Generic;

public enum CurrencyType
{
    Gold,                   //골드
    RandomCommon,           //흔함 랜덤
    SelectCommon,           //흔함 선택
    RandomSpecial,          //특별함 랜덤
    RandomRare,             //희귀함 랜덤
    RandomLegendary,        //전설적인 랜덤
    SelectSpecialtyGoods    //특수재화
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private DifficultyData[] difficultyPresets; //에디터에서 Easy, Normal, Hard
    
    private Dictionary<CurrencyType, int> currencyRepository = new Dictionary<CurrencyType, int>();
    // 게임 시스템 변수
    [Header("Debug/Resources")]
    [SerializeField] private int startGold = 500;   // 초기 골드 (난이도 별로 다르게 할 수도 있음)
    [SerializeField] private int startChoice = 10; //랜덤 흔함
    //public int CurrentGold => currentGold;

    //현재 적용된 난이도 (외부에서는 프로퍼티로 정보 가져간다)
    private DifficultyData currentDifficulty;
    public DifficultyData CurrentDifficulty => currentDifficulty;
    
    //플레이어 소유 유닛 리스트
    private List<UnitEntity> playerUnits = new List<UnitEntity>();
    public List<UnitEntity> PlayerUnits => playerUnits; //외부(조합 매니저 등) 에서 접근용
    
    //현재 필드에 존재하는 적 리스트(라인사 체크용)
    private List<EnemyEntity> activeEnemies = new List<EnemyEntity>();
    
    
    //보스전 관련
    private bool isBossRound = false;
    private float bossTimer = 0f;

    //스토리 관련(예시:1단계부터 시작)
    private int currentStoryStep = 1;
    
    

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
        
        //[테스트 용] 게임 시작 시 자동으로 0번(easy) 난이도로 설정
        // 나중에는 로비 씬에서 버튼 눌러서 선택하게 변경 가능
        if (difficultyPresets != null && difficultyPresets.Length > 0)
        {
            SetDifficulty(0);
        }
        //1. 모든 재화 0으로 초기화
        foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
        {
            currencyRepository[type] = 0;
        }
        // 2. 초기 자금 지급
        AddCurrency(CurrencyType.Gold, startGold);
        AddCurrency(CurrencyType.RandomCommon, startChoice);
        //테스트용 선택권 지급
        AddCurrency(CurrencyType.SelectCommon, 1);
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
        //UIManager.Instance.UpdateGold();
        UIManager.Instance.UpdateUnitCount(activeEnemies.Count, currentDifficulty.MaxUnitCountLimits);
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

    public void RegisterUnit(UnitEntity unit)
    {
        if (!playerUnits.Contains(unit))
        {
            playerUnits.Add(unit);
        }
    }

    public void UnregisterUnit(UnitEntity unit)
    {
        if (playerUnits.Contains(unit))
        {
            playerUnits.Remove(unit);
        }
    }
    
    
    
    // --- 1. 라인사 관리(유닛 등록/해제) ---
    public void RegisterEnemy(EnemyEntity enemy)
    {
        activeEnemies.Add(enemy);

        if (currentDifficulty!= null && activeEnemies.Count >= currentDifficulty.MaxUnitCountLimits)
        {
            TriggerGameOver($"라인 유닛 수 초과!({activeEnemies.Count}/{currentDifficulty.MaxUnitCountLimits})-라인사");
        }
        
        //UI 갱신 요청 나중에 구현
    }

    public void UnRegisterEnemy(EnemyEntity enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
        //보스를 잡았을 경우 보스 라운드 종료
        if (enemy.Data.Type == EnemyType.Boss)
        {
            BossDeafeated();
        }
    }

    //---재화 공동 관련 ---
    // 1. 재화 개수 확인
    public int GetCurrency(CurrencyType type)
    {
        return currencyRepository.ContainsKey(type) ? currencyRepository[type] : 0;
    }
    // 2. 재화 획득
    public void AddCurrency(CurrencyType type, int amount)
    {
        currencyRepository[type] += amount;

        //UI갱신 (UIManager에 해당 함수 만들어서 연결)
        //UIManager.Instance.UpdateCurrencyUI(type,currencyRepository[type]
        Debug.Log($"{type} 획득: +{amount}(현재:{currencyRepository[type]})");
    }
    //3. 재화 사용
    public bool SpendCurrency(CurrencyType type, int amount)
    {
        if(GetCurrency(type)>= amount)
        {
            currencyRepository[type] -= amount;

            //UI갱신
            //UIManager.Instance.UpdateCurrencyUI
            Debug.Log("재화 사용");
            return true;
        }
        else
        {
            Debug.Log($"[실패] {type}이 부족합니다");
            return false;
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

    private void BossDeafeated()
    {
        isBossRound = false;
        Debug.Log("보스 처치 성공!");
    }

    public void TriggerGameOver(string reason)
    {
        Debug.LogError("GameOver"+reason);
        Time.timeScale = 0;//게임 정지
        
        //여기에 GameOverUI 팝업 띄우는 로직 추가
    }
    
    
}
