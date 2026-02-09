using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private DifficultyData difficultyData;

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

    // --- 1. 라인사 관리(유닛 등록/해제) ---
    public void ResisterEnemy(EnemyEntity enemy)
    {
        activeEnemies.Add(enemy);

        if (activeEnemies.Count >= difficultyData.MaxUnitCountLimits)
        {
            TriggerGameOver($"라인 유닛 수 초과!({activeEnemies.Count}/{difficultyData.MaxUnitCountLimits})-라인사");
        }
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

    // --- 2. 스토리사 관련 ---
    public void CheckStoryCondition(int currentRound)
    {
        if (currentRound == 40)
        {
            if (currentStoryStep < difficultyData.StoryLimit) //예: 현재 스토리 3 < 스토리 제한 
            {
                TriggerGameOver("정해진 라운드 내에 스토리 클리어 실패!");
            }
        }
    }
    
    // --- 3. 보스전 관리 ---
    public void StartBossRound()
    {
        isBossRound = true;
        bossTimer = difficultyData.BossTimeLimit;
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
