using UnityEngine;

public abstract class EntityData : ScriptableObject
{
    [SerializeField] private string entityName; //이름
    [SerializeField] private float moveSpeed; //이동 속도
    [SerializeField] private GameObject prefab; //프리펩

    public string EntityName => entityName;
    public float MoveSpeed => moveSpeed;
    public GameObject Prefab => prefab;
}

[CreateAssetMenu(fileName = "NewUnitData", menuName = "LookismDefense/UnitData")]
public class UnitData : EntityData
{
    [Header("Unit Specifics")]
    [SerializeField] private int tier; //등급
    [SerializeField] private float attackDamage; //공격력
    [SerializeField] private int attackRange; //공격 사거리
    [SerializeField] private float attackSpeed; //공격속도

    public int Tier => tier;
    public float AttackDamage => attackDamage;
    public int AttackRange => attackRange;
    public float AttackSpped => attackSpeed;
}


public  enum EnemyType
{
    Normal, //일반 라인 몹
    Mission, //퀘스트 미션용 몹
    Boss, // 보스
    Story //스토리 건물/유닛
    
}
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "LookismDefense/EnemyData")]
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
