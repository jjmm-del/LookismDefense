using UnityEngine;

public  enum EnemyType
{
    Normal, //일반 라인 몹
    Mission, //퀘스트 미션용 몹
    Boss, // 보스
    Story //스토리 건물/유닛
    
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
    
    public int Round => round;
    public int MaxHealth => maxHealth;
    public EnemyType Type => type;
    public int Defense => defense;
    public float DropGold => dropGold;
}