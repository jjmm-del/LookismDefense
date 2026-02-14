using UnityEngine;
using UnityEngine.AI;

public enum UnitState
{
    Idle, // 대기( 사거리 내 적 자동 공격)
    Move, // 강제 이동(적 무시)
    AttackMove, // 이동하며 적 발견 시 공격(어택땅)
    Hold // 위치 고정(움직이지 않고 사거리 내 적 공격)
}
    
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

    //상태 관리 변수
    private UnitState currentState = UnitState.Idle;
    private Vector3 attackMoveDest; //어택땅 목적지 기억용
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

    private void Start()
    {
        //태어날 때 게임매니저에 나를 등록
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterUnit(this);
            Debug.Log($"{this.unitData.name} 등록");
        }
    }
    
    private void Update()
    {
        switch (currentState)
        {
            case UnitState.Idle:
            case UnitState.Hold :
                ScanAndAttack(); //제자리 경계
                    break;
            case UnitState.Move:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    currentState = UnitState.Idle;
                }
                break;
            
            case UnitState.AttackMove:
                HandleAttackMove();
                break;
        }
    }

    private void HandleAttackMove()
    {
        // 1. 현재 타겟이 유효한지 확인(죽었거나 사거리 밖)
        if (currentTarget != null)
        {
            if (Vector3.Distance(transform.position, currentTarget.position) > unitData.AttackRange ||
                currentTarget.gameObject == null)
            {
                currentTarget = null; //타겟 해제 -> 다시 이동 모드로
            }
        }
        // 2. 타겟이 없다면 주변 스캔
        if (currentTarget == null)
        {
            currentTarget = FindNearestEnemy();

            if (currentTarget != null)
            {
                //적 발견 -> 멈춰서 공격
                agent.isStopped = true;
                TryAttack();
            }
            else
            {
                //적 없음 ->  목적지로 계속 이동
                agent.isStopped = false;
                agent.SetDestination(attackMoveDest);
                
                //목적지 도착 체크
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    currentState = UnitState.Idle;
                }
            }
        }
        else
        {
            //타겟이 있음 -> 제자리에서 공격
            agent.isStopped = true;
            transform.LookAt(currentTarget);
            TryAttack();
        }
    }

    //제자리 경계 (Idle, Hold)
    private void ScanAndAttack()
    {
        if (currentTarget == null ||
            Vector3.Distance(transform.position, currentTarget.position) > unitData.AttackRange)
        {
            currentTarget = FindNearestEnemy();
        }

        if (currentTarget != null)
        {
            transform.LookAt(currentTarget);
            TryAttack();
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, unitData.AttackRange, enemyLayer);
        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTarget = hit.transform;
            }
        }
        return bestTarget;
    }

    private void TryAttack()
    {
        float attackCooldown = 1f / currentAttackSpeed;
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }
    

    private void PerformAttack()
    {
        if (currentTarget == null) return;
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

    // 강제 이동 M
    public void OrderMove(Vector3 destination)
    {
        currentState = UnitState.Move;
        agent.isStopped = false;
        agent.SetDestination(destination);
        currentTarget = null; //타겟 무시
    }
    //어택땅 A
    public void OrderAttackMove(Vector3 destination)
    {
        currentState = UnitState.AttackMove;
        attackMoveDest = destination;
        agent.isStopped = false;
        agent.SetDestination(destination);
    }
    
    //적 우클릭 타겟 공격
    public void OrderAttackTarget(EnemyEntity targetEnemy)
    {
        //간단한 구현: 어택땅 모드인데 적 위치로 설정하고 타겟을 강제지정
        currentState = UnitState.AttackMove;
        attackMoveDest = targetEnemy.transform.position;
        currentTarget = targetEnemy.transform;
    }
    
    //홀드 H
    public void OrderHold()
    {
        currentState = UnitState.Hold;
        agent.isStopped = true;
        agent.ResetPath();
    }

    //스탑 S
    public void OrderStop()
    {
        currentState = UnitState.Idle;
        agent.isStopped = true;
        agent.ResetPath();
        currentTarget = null;
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

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterUnit(this);
        }
    }
}

