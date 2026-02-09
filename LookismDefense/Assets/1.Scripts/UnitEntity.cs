using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitEntity : MonoBehaviour
{
    [SerializeField]private UnitData unitData;
    public UnitData Data => unitData;

    [Header("Attack Settings")]
    [SerializeField] private LayerMask enemyLayer; //적 레이어(설정 필수)
    
    private NavMeshAgent agent;
    private float lastAttackTime;
    private Transform currentTarget;

    private float currentAttackDamage;
    private float currentAttackRange;
    private float currentAttackSpeed;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    public void Initialize(UnitData data)
    {
        unitData = data;

        // [초기화] 생성시 유닛 데이터의 기본 스탯을 가져옴;
        currentAttackDamage = unitData.AttackDamage;
        currentAttackRange = unitData.AttackRange;
        currentAttackSpeed = unitData.AttackSpeed;
        
        if (agent != null)
        {
            agent.speed = unitData.MoveSpeed;
        }
    }

    private void Update()
    {
        // 1. 타겟 검색 및 갱신
        UpdateTarget();
        
        // 2. 공격 쿨타임 체크 및 공격    
        if (currentTarget != null)
        {
            float attackCooldown = 1f / currentAttackSpeed;
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    private void UpdateTarget()
    {
        // 기존 타겟이 사거리 밖으로 나갔거나 죽어서 사라졌는지 확인
        if (currentTarget != null)
        {
            //타겟이 파괴되었으면 null 처리
            if (currentTarget.gameObject == null)
            {
                currentTarget = null;
            }
            //사거리 체크
            else if (Vector3.Distance(transform.position, currentTarget.position) > unitData.AttackRange)
            {
                currentTarget = null;
            }
        }
        //타겟이 null이면 새로 찾기
        if (currentTarget == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, unitData.AttackRange, enemyLayer);

            float closestDistance = Mathf.Infinity;
            Transform bestTarget = null;

            foreach (Collider hit in hits)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = hit.transform;
                }
            }
            currentTarget = bestTarget;
        }
    }

    private void PerformAttack()
    {
        if (currentTarget == null) return;
        
        // 1. 적을 바라봄(Lookism 특유의 액션감을 위해 회전)
        transform.LookAt(currentTarget);
        
        // 2. 적의 컴포넌트를 가져와서 때림
        EnemyEntity enemy = currentTarget.GetComponent<EnemyEntity>();
        if (enemy != null)
        {
            enemy.OnDamage(unitData.AttackDamage);
            
            //(선택, 추가) 이펙트, 타격음 추가
            //EffectManager.Instance.PlayHitEffect(currentTarget.Position);
            Debug.Log($"{unitData.EntityName}이 {enemy.Data.EntityName}을 공격!");
        }
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent != null)
        {
            agent.SetDestination(destination);
            currentTarget = null;
        }
    }
    
    //에디터에서 사거리 확인용
    private void OnDrawGizmos()
    {
        if (unitData != null)
        {
            Gizmos.color = Color.aquamarine;
            Gizmos.DrawWireSphere(transform.position, unitData.AttackRange);

        }
    }

    // [추가 기능] 나중에 업그레이드 시스템에서 호출할 함수
    public void ApplyAttackSpeedBuff(float amount)
    {
        currentAttackSpeed += amount;
    }
    
}

