using UnityEngine;
using System.Collections.Generic;
public  enum EnemyType
{
    Normal, //일반 라인 몹
    Mission, //퀘스트 미션용 몹
    Boss, // 보스
    Story //스토리 건물/유닛
    
}

[System.Serializable]
public struct RewardInfo
{
    public CurrencyType currencyType;   //재화 종류
    public int amount;                  //지급할 개수
}
    


[CreateAssetMenu(menuName = "LookismDefense/EnemyData")]
public class EnemyData : EntityData
{
    
    [Header("Enemy Specifics")]
    [SerializeField] private int round;
    [SerializeField] private EnemyType type;
    [SerializeField] private int maxHealth;
    [SerializeField] private int defense;
    [SerializeField] private float dropGold;
    
    [Header("StoryRewards")]
    [SerializeField] private List<RewardInfo> storyRewards = new List<RewardInfo>();
    
    public int Round => round;
    public int MaxHealth => maxHealth;
    public EnemyType Type => type;
    public int Defense => defense;
    public float DropGold => dropGold;
    public List<RewardInfo> StoryRewards => storyRewards;
}