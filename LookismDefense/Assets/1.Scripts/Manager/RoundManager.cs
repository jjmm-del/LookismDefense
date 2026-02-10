using UnityEngine;
using TMPro;
public class RoundManager : MonoBehaviour
{
    [Header("Round Settings")]
    [SerializeField] private float roundDuration = 60f; //한 라운드 시간
    [SerializeField] private int maxRounds = 50; //전체 라운드 수 -> 난이도 별 라운드 수 수정 예정
    
    [Header("References")]
    [SerializeField] private WaveManager waveManager;

    private int currentRound = 0;
    private float roundTimer = 0f;
    private bool isGameRunning = true;
    
    private void Start()
    {
        //게임 시작 시 1라운드 시작
        StartNextRound();
    }

    private void Update()
    {
        if (!isGameRunning) return;

        //타이머 감소
        if (roundTimer > 0)
        {
            roundTimer -= Time.deltaTime;
            
            //시간이 다 되면 다음 라운드
            if (roundTimer <= 0)
            {
                StartNextRound();
            }
        }
    }

    private void StartNextRound()
    {
        //이전 라운드에 대한 스토리사 체크(GameManager에 위임)
        if (currentRound > 0)
        {
            GameManager.Instance.CheckStoryCondition(currentRound);
        }
        currentRound++;

        if (currentRound > maxRounds)
        {
            Debug.Log("축하합니다 모든 라운드 클리어!");
            isGameRunning = false;
            return;
        }
        Debug.Log($"{currentRound}라운드 시작!");
        roundTimer = roundDuration;
        
        //WaveManager에게 현재 라운드에 맞는 적 소환 요청
        //(WaveManager의 SpawnWaveRoutine을 수정하건, 여기서 직접 함수를 호출해야 함
        waveManager.StartWave(currentRound);
        
        //특정 라운드(예: 10, 20, 30, ...)는 보스 라운드 처리
        if (currentRound % 10 == 0)
        {
            GameManager.Instance.StartBossRound();
        }
    }
}
