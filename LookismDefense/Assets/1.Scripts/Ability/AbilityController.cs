using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityController : MonoBehaviour
{
    private List<AbilityData> myAbilities = new List<AbilityData>();
    private UnitEntity myUnit; //나 자신 (범위 탐색 기준점)

    //유닛잇 스폰될때 UnitEntity에서 이 함수 불러줌
    public void Initialize(List<AbilityData> abilities, UnitEntity unit)
    {
        myAbilities = abilities ?? new List<AbilityData>();
        myUnit = unit;

        //주기적 발동(오라) 능력이 있다면 코루틴 시작
        foreach (var ability in myAbilities)
        {
            if (ability.triggerType == AbilityTrigger.Periodic)
            {
                StartCoroutine(PeriodicAuraRoutine(ability));
            }
        }
    }

    // 1. 공격 시 발동 로직 (단일 및 스플래시)
    public float ProcessOnHitAbilities(EnemyEntity target, float baseDamage)
    {
        float finalDamage = baseDamage;

        // 내가 가진 능력들을 하나씩 검사
        foreach (AbilityData ability in myAbilities)
        {
            if (ability.triggerType != AbilityTrigger.OnHit) continue;

            //확률 검사 (0~ 100사이의 난수가 chance보다 낮으면 발동)
            if (Random.Range(0f, 100f) <= ability.chance)
            {
                // [단일 타겟 적용]
                if (ability.radius <= 0)
                {
                    ApplyEffectToEnemy(target, ability, ref finalDamage);
                }
                // [범위 타겟 적용]
                else
                {
                    //타겟 주변 반경(radius) 내의 모든 적 검색
                    Collider[] hits = Physics.OverlapSphere(target.transform.position, ability.radius, LayerMask.GetMask("Enemy"));
                    foreach(Collider hit in hits)
                    {
                        EnemyEntity splashTarget = hit.GetComponent<EnemyEntity>();
                        if (splashTarget != null)
                        {
                            float dummyDamage = baseDamage; //스플래시 대상용 임시 데미지
                            ApplyEffectToEnemy(splashTarget, ability, ref dummyDamage);

                            if (splashTarget != target && ability.abilityType == AbilityType.Splash)
                            {
                                splashTarget.OnDamage(dummyDamage);
                            }
                        }
                    }
                }
            }
        }
        return finalDamage;
    }
    
    // 2. 주기적 발동 로직(오라)
    private IEnumerator PeriodicAuraRoutine(AbilityData ability)
    {
        while (true)
        {
            //대상이 적군(이감, 도트딜 등)
            if (ability.targetType == AbilityTargetType.Enemy)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, ability.radius, LayerMask.GetMask("Enemy"));
                foreach (Collider hit in hits)
                {
                    EnemyEntity enemy = hit.GetComponent<EnemyEntity>();
                    if (enemy != null)
                    {
                        float dummy = 0;
                        ApplyEffectToEnemy(enemy, ability, ref dummy);
                    }
                }
            }
            //대상이 아군(공속, 공격력 버프 등)
            else if (ability.targetType == AbilityTargetType.Ally)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, ability.radius, LayerMask.GetMask("Unit"));
                foreach (Collider hit in hits)
                {
                    UnitEntity ally = hit.GetComponent<UnitEntity>();
                    if (ally != null)
                    {
                        ApplyEffectToAlly(ally, ability);
                    }
                }
            }

            yield return new WaitForSeconds(ability.tickRate); // 1초마다 반복
        }
    }

    // --- 실제 효과 적용부 ---
    private void ApplyEffectToEnemy(EnemyEntity enemy, AbilityData ability, ref float damage)
    {
        switch (ability.abilityType)
        {
            case AbilityType.Critical:
                damage *= ability.value; //데미지 배율 적용
                break;
            case AbilityType.Splash:
                damage *= ability.value; //스플래시 데미지 배율 (0.5 면 50퍼 대미지)
                break;
            case AbilityType.Stun:
                enemy.ApplyStun(ability.duration);
                break;
            case AbilityType.Slow: // 이감 적용 (value가 30퍼면 30퍼 느려짐)
                enemy.ApplySlow(ability.value, ability.duration);
                break;
            case AbilityType.ArmorBreak:
                enemy.ApplyArmorBreak(ability.value, ability.duration);
                break;
        }
    }

    private void ApplyEffectToAlly(UnitEntity ally, AbilityData ability)
    {
        switch (ability.abilityType)
        {
            case AbilityType.BuffDamage:
                ally.ApplyAttackDamageBuff(ability.value, ability.duration);
                break;
            case AbilityType.BuffSpeed:
                ally.ApplyAttackSpeedBuff(ability.value, ability.duration);
                break;
        }
    }
}
