using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private WayPointSystem wayPointSystem;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float timeBetweenEnemies = 1f;
    
    [Header("WaveData")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemiesPerWave = 40;

    private int currentWave = 0;
    private bool isWaveInProgress = false;

    public void StartGameLoop()
    {
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        while (true) //게임이 끝날 때까지 무한 반복(또는 조건부)
        {
            currentWave++;
            Debug.Log($"{currentWave}웨이브 시작!");
            isWaveInProgress = true;

            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnEnemy();
                //다음 적 생성 전 대기
                yield return new WaitForSeconds(timeBetweenEnemies);
            }
            isWaveInProgress = false;
            Debug.Log($"{currentWave}종료. 다음 웨이브 대기 중..");
            
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        EnemyMovement movement = enemyObj.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.Initialize(wayPointSystem.WayPoints);
        }
    }
    
    
}
