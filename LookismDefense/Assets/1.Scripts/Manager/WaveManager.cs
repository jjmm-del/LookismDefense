using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName; //이름(구분용)
        public EnemyData enemyData; //생성할 적 데이터(여기에 스크립터블 오브젝트를 넣습니다)
        public int count; //생성할 마리 수
        public float spawnInterval; //생성 간격
    }
    
    
    [Header("Settings")]
    [SerializeField] private WayPointSystem wayPointSystem;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float timeBetweenWaves = 10f;

    [Header("Waves")] [SerializeField] private List<Wave> waves; //웨이브 목록 리스트

    private int currentWaveIndex = 0;
    private bool isWaveInProgress = false;

    
    //게임 시작 시 호출
    public void StartGameLoop()
    {
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("WaveManager: 설정된 웨이브가 없습니다!");
            return;
        }
        StartCoroutine(SpawnWaveRoutine());
    }
    
    private IEnumerator SpawnWaveRoutine()
    {
        //준비된 웨이브 리스트 만큼 반복
        while (currentWaveIndex<waves.Count) 
        {
            Wave currentWave = waves[currentWaveIndex];
            
            Debug.Log($"{currentWaveIndex+1}웨이브 시작!{currentWave.waveName}");
            isWaveInProgress = true;

            for (int i = 0; i < currentWave.count; i++)
            {
                SpawnEnemy(currentWave.enemyData, wayPointSystem.WayPoints);
                //다음 적 생성 전 대기
                yield return new WaitForSeconds(currentWave.spawnInterval);
            }
            isWaveInProgress = false;
            Debug.Log($"{currentWave}종료. 다음 웨이브 대기 중..");
            
            yield return new WaitForSeconds(timeBetweenWaves);
            currentWaveIndex++;
        }
        Debug.Log("모든 웨이브가 종료되었습니다.");
    }

    private void SpawnEnemy(EnemyData data, Transform[] path)
    {
        if (data == null || data.Prefab == null)
        {
            Debug.LogError("SpawnEnemy: EnemyData 또는 Prefab이 비어있습니다.");
        }
        
        GameObject enemyObj = Instantiate(data.Prefab, spawnPoint.position, Quaternion.identity);

        EnemyEntity enemyEntity= enemyObj.GetComponent<EnemyEntity>();
        
        if (enemyEntity != null)
        {
            enemyEntity.SetUp(data, path);
        }
        else
        {
            Debug.LogError("SpawnEnemy: 생성된 프리팹에 EnemyEntity 컴포넌트가 없습니다.");
        }
    }
    
    
}
