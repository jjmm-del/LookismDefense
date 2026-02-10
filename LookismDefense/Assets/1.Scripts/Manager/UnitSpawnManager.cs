using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class UnitSpawnManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform spawnAreaCenter; // 유닛이 생성될 구역 중심
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(5, 5); //생성 구역
    
    [Header("Gacha Data")]
    [SerializeField] private List<UnitData> tier1Units; //

    public void SpawnRandomUnit()
    {
        if (tier1Units.Count <= 0)
        {
            Debug.LogError("뽑을 유닛 데이터가 없습니다.");
            return;
        }
        
        // 1. 랜덤 유닛 선택
        int randomIndex = Random.Range(0, tier1Units.Count);
        UnitData selectedUnit = tier1Units[randomIndex];
        
        // 2. 랜덤 위치 계산 (겹치지 않게 하려면 나중에 그리드 시스템 적용 필요)
        Vector2 randomPos = GetRandomPosition();
        
        // 3. 생성
        GameObject unitObj = Instantiate(selectedUnit.Prefab, randomPos, Quaternion.identity);
        
        // 4. 데이터 주입
        UnitEntity unitEntity = unitObj.GetComponent<UnitEntity>();
        unitEntity.Initialize(selectedUnit);
    }

    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float z = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);
        return spawnAreaCenter.position + new Vector3(x, 0, z);
    }
    
    //에디터에서 생성 범위 확인용
    private void OnDrawGizmos()
    {
        if (spawnAreaCenter != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(spawnAreaCenter.position, new Vector3(spawnAreaSize.x,1,spawnAreaSize.y));
        }
    }
}
