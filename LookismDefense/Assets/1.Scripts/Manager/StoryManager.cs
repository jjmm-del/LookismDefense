using UnityEngine;
using System.Collections.Generic;
public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }
    
    [Header("StorySettings")]
    [SerializeField] private Transform storySpawnPoint;
    [SerializeField] private EnemyData[] storySequence;

    [SerializeField] private Transform storyTeleportPoint;
    public Vector3 StoryTeleportPosition => storyTeleportPoint != null ? storyTeleportPoint.position : storySpawnPoint.position;
    public int currentStoryStep { get; private set; } = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void StartStory()
    {
        SpawnNextStory();
    }

    private void SpawnNextStory()
    {
        if (storySpawnPoint == null )
        {
            return;
        }

        if (currentStoryStep >= storySequence.Length)
        {
            Debug.Log("모든 스토리를 클리어 했습니다.");
            return;
        }

        EnemyData nextData = storySequence[currentStoryStep];
        if (nextData.Prefab == null)
        {
            Debug.LogError($"[StoryManager]");
        }
        GameObject storyObj = Instantiate(nextData.Prefab, storySpawnPoint.position, Quaternion.identity);
        EnemyEntity storyEntity = storyObj.GetComponent<EnemyEntity>();

        if (storyEntity != null)
        {
            storyEntity.Setup(nextData);
        }
        Debug.Log($"[스토리] {nextData.name}출현!");
    }
    //스토리 존 파괴했을 때 호출될 함수
    public void AdvanceStory(List<RewardInfo> rewards)
    {
        
        if (GameManager.Instance != null)
        {
            //보상 지급
            foreach (RewardInfo reward in rewards)
            {
                GameManager.Instance.AddCurrency(reward.currencyType, reward.amount);
            }
        }
        currentStoryStep++;
        Debug.Log($"스토리 {currentStoryStep}단계 클리어! 보상 지급 완료!");
        if (UIManager.Instance != null)
        {
            //화면 중앙에 "스토리 클리어! 보상 { 보상 종류 ,개수 }를 지급합니다 함수 연결
        }

        SpawnNextStory();
    }
}
