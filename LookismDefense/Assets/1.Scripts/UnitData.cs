using UnityEngine;

public enum UnitTier
{
    Common,         //흔함
    Uncommon,       //안흔함
    Special,        //특별함
    Rare,           //희귀함
    Legendary,      //전설적인
    Hidden,         //히든조합
    Changed,        //변화된
    Transcendence,  //초월함
    Immortal,       //불멸의
    Eternal,        //영원함
    Limited         //제한됨
}
[CreateAssetMenu(menuName = "LookismDefense/UnitData")]
public class UnitData : EntityData
{
    [Header("Unit Specifics")]
    [SerializeField] private UnitTier tier; //등급
    [SerializeField] private float attackDamage; //공격력
    [SerializeField] private int attackRange; //공격 사거리
    [SerializeField] private float attackSpeed; //공격속도

    public UnitTier Tier => tier;
    public float AttackDamage => attackDamage;
    public int AttackRange => attackRange;
    public float AttackSpeed => attackSpeed;
}