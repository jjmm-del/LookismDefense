using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;     //이름(구분용)
        public EnemyData enemyData; //생성할 적 데이터(여기에 스크립터블 오브젝트를 넣습니다)
        public int count;           //생성할 마리 수
        public float spawnInterval; //생성 간격
    }
    
    
    [Header("Settings")]
    [SerializeField] private WayPointSystem waypointSystem;
    [SerializeField] private Transform spawnPoint;

    [Header("Waves")] [SerializeField] private List<Wave> waves; //웨이브 목록 리스트

    private int currentWaveIndex = 0;
    //private bool isWaveInProgress;

    
    //게임 시작 시 호출
    public void StartWave(int roundIndex)
    {
        //라운드 인덱스는 0부터 시작하므로 -1
        int index = roundIndex - 1;
        
        if (index >= 0 && index < waves.Count)
        {
            StartCoroutine(SpawnWaveRoutine(waves[index]));
        }
        else
        {
            Debug.LogWarning($"라운드 {roundIndex}에 해당하는 웨이브 데이터가 없습니다.");
        }
        
    }
    
    private IEnumerator SpawnWaveRoutine(Wave wave)
    {
        //준비된 웨이브 리스트 만큼 반복
        //isWaveInProgress = true;
        Debug.Log($"{currentWaveIndex+1}웨이브 시작!{wave.waveName}");

        for (int i = 0; i < wave.count; i++)
        {
            SpawnEnemy(wave.enemyData, waypointSystem.WayPoints);
            //다음 적 생성 전 대기
            yield return new WaitForSeconds(wave.spawnInterval);
        } 
        //isWaveInProgress = false;
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
            enemyEntity.Setup(data, path);
        }
        else
        {
            Debug.LogError("SpawnEnemy: 생성된 프리팹에 EnemyEntity 컴포넌트가 없습니다.");
        }
    }
    
    
}
