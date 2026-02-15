using UnityEngine;

[CreateAssetMenu(menuName = "LookismDefense/UnitData")]
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
    public float AttackSpeed => attackSpeed;
}