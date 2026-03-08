using UnityEngine;

//발동 시점
public enum AbilityTrigger
{
    OnHit,      //적을 때렸을 때 (치명타, 스턴 등)
    Periodic    //일정 주기마다(주변 도트딜, 오라 등)
}
public enum AbilityTargetType
{
    Enemy,  //적에게 적용
    Ally,   //아군에게 적용
}

//능력 종류
public enum AbilityType
{
    Critical,   //치명타
    Stun,       //스턴
    Slow,       //이감
    ArmorBreak,  //방깍
    Splash,
    BuffDamage,
    BuffSpeed
}
[CreateAssetMenu(fileName = "New Ability", menuName = "LookismDefense/AbilityData")]
public class AbilityData : ScriptableObject
{
    [Header("UI Settings")]
    public string abilityName;
	public Sprite abilityIcon;

    public AbilityTrigger triggerType;
    public AbilityTargetType targetType;
    public AbilityType abilityType;
    
    [Header("Settings")]
    [Range(0,100f)] public float chance;    // 발동 확률
    public float value;     // 수치(배율, 스턴 시간, 방깎수치 등)
    public float duration;  //지속 시간(스턴 시간, 버프 시간)
    public float radius;    //범위 (0이면 단일타겟, 0보다 크면 범위 적용)
    
    [Header("Periodic Settings")]
    public float tickRate = 1f; //몇 초마다 오라를 갱신할 것인가? (기본 1초)
}
