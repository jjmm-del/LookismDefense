using UnityEngine;
using System.Collections.Generic;
public class AbilityController : MonoBehaviour
{
    private List<AbilityData> myAbilities = new List<AbilityData>();
    
    //유닛잇 스폰될때 UnitEntity에서 이 함수 불러줌
    public void Initialize(List<AbilityData> abilities)
    {
        if (abilities != null)
        {
            myAbilities = abilities;
        }
    }
    
    //적을 타격 할 때 호출 됨(최종데미지 리턴)
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
                //발동 성공! -> 능력 종류별로 효과 적용
                switch (ability.abilityType)
                {
                    case AbilityType.Critical:
                        finalDamage *= ability.value;//데미지 배율 적용
                        break;
                    case AbilityType.Stun:
                        //target.ApplyStun(ability.value);
                        break;
                    case AbilityType.Slow:
                        //target.ApplySlow(ability.value);
                        break;
                }
            }
        }

        return finalDamage;
    }
}
