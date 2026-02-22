using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
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
    [SerializeField] private LayerMask enemyLayer; // 적 레이어(설정 필수)
    
    [Header("UI & Visuals")]
    [SerializeField] private GameObject selectionIndicator; //초록색 원
    
    private NavMeshAgent agent;
    private AbilityController abilityController;
    private float lastAttackTime;
    private Transform currentTarget; //현재 공격 대상

    //스탯 ( 버프 적용 등을 위해 변수로 관리)
    private float currentAttackDamage;
    private float currentAttackRange;
    private float currentAttackSpeed;

    //상태 관리 변수
    private UnitState currentState = UnitState.Idle;
    private Vector3 attackMoveDest; //어택땅 목적지 기억용
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        abilityController = GetComponent<AbilityController>();
    }
    public void Initialize(UnitData data)
    {
        // [초기화 ] 유닛 데이터
        unitData = data;

        // [초기화] 생성시 유닛 데이터의 기본 스탯;
        currentAttackDamage = unitData.AttackDamage;
        currentAttackRange = unitData.AttackRange;
        currentAttackSpeed = unitData.AttackSpeed;
        
        // [초기화] 이동속도
        if (agent != null)
        {
            agent.speed = unitData.MoveSpeed;
            // [중요] 멈추는 거리를 사거리보다 약간 짧게 설정하여 확실히 공격 범위 안에 들게함)
            
        }
        // [초기화] 유닛 특수능력
        if (abilityController != null)
        {
            abilityController.Initialize(data.Abilities);
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
        // 1. 타겟 유효성 검사 (죽었거나 사라졌으면 null 처리)
        CheckTargetValidity();
        
        // 2. 상태별 행동 처리
        switch (currentState)
        {
            case UnitState.Idle:
                HandleIdleState();
                break;
            case UnitState.Move:
                HandleMoveState();
                break;
            case UnitState.AttackMove:
                HandleAttackMoveState();
                break;
            case UnitState.Hold :
                HandleHoldState(); 
                break;
        }
    }

    // --- 상태별 로직 메서드 ---
    private void HandleIdleState()
    {
        //타겟이 없다면 주변 스캔
        if (currentTarget == null)
        {
            currentTarget = FindNearestEnemy();
        }
        //타겟이 있다면? -> 쫓아가서 공격(전투 처리 위임)
        if (currentTarget != null)
        {
            ProcessCombat(true); // true = 움직여서 쫓아가라
        }
    }

    private void HandleMoveState()
    {
        //남은 거리가 정지 거리보다 작거나,
        // 길은 있는데 다른 유닛에 막혀서 거의 움직이지 못하고 있을 때(속도 0.1 미만) 도착 처리
        if(!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance || agent.velocity.sqrMagnitude < 0.1f)
            {
                agent.isStopped = true;
                currentState = UnitState.Idle;
                
            }
        }
    }
    private void HandleAttackMoveState()
    {
        // 1. 타겟이 없다면 주변 스캔
        if (currentTarget == null)
        {
            currentTarget = FindNearestEnemy();
        }
        
        // 2. 타겟이 있다면 -> 전투모드 (쫓아가서 공격)
        if (currentTarget != null)
        {
            ProcessCombat(true);
        }
        else
        {
            // 3. 타겟이 없으면 -> 어택땅 목적지로 계속 이동
            if (agent.destination != attackMoveDest)
            {
                agent.SetDestination(attackMoveDest);
                agent.isStopped = false;
            }
            
            //도착 체크
            if(!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance || agent.velocity.sqrMagnitude < 0.1f)
                {
                    agent.isStopped = true;
                    currentState = UnitState.Idle;
                
                }
            }
        }
    }
    
    private void HandleHoldState()
    {
        if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > currentAttackRange)
        {
            currentTarget = FindNearestEnemy();
        }

        //전투 처리하되, 절대 움직이지 않음(false)
        if (currentTarget != null)
        {
            ProcessCombat(false); // false = 추적 금지 (제자리 공격)
        }
        else
        {
            agent.isStopped = true; //적 없으면 가만히
        }
    }
    
    // --- 핵심 전투 로직 (추적 + 공격) ---
    // canChase : ture 면 적이 멀 때 쫓아감, false면 (홀드) 제자리에서 사거리 닿을 때만 공격
    private void ProcessCombat(bool canChase)
    {
        agent.stoppingDistance = Mathf.Max(0.5f, currentAttackRange * 0.9f);
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        
        // A. 사거리 안인가? -> 멈추고 공격
        if (distance <= currentAttackRange)
        {
            agent.isStopped = true; //이동 멈춤
            transform.LookAt(currentTarget); //적 바라보기
            TryAttack();
        }
        // B. 사거리 밖인가? 
        else
        {
            if (canChase)
            {
                // 추적 허용: 적 위치로 이동
                agent.isStopped = false;
                agent.SetDestination(currentTarget.position);
            }
            else
            {
                //추적 불가(홀드) : 그냥 가만히 있음
                agent.isStopped = true;
            }
        }
    }
    
    // --- 유틸리티 메서드 ---
    private void CheckTargetValidity()
    {
        if (currentTarget != null)
        {
            //적 오브젝트가 파괴되었거나, 비활성되었으면 타겟 해제
            if (currentTarget.gameObject == null || !currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = null;
            }
            // (참고) 적이 죽었는지 확인하는 스크립트 연결부가 있다면 여기서 체크
            //EnemyEntity enemy = currentTarget.GetComponent<EnemyEntity>();
            //if(enemy != null && enemy.CurrentHealth <= 0) currentTarget = null;
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
        
        // 업그레이드 적용 데미지 계산
        float baseFinalDamage = UpgradeManager.Instance.GetFinalDamage(currentAttackDamage, unitData.Tier);
        
        // 공격할 타겟 리스트 만들기
        List<EnemyEntity> targetToHit = new List<EnemyEntity>();
        
        
        EnemyEntity enemy = currentTarget.GetComponent<EnemyEntity>();
        if (enemy != null)
        {
            
            
            // 특수능력 적용
            float realFinalDamage = baseFinalDamage;
            if (abilityController != null)
            {
                realFinalDamage = abilityController.ProcessOnHitAbilities(enemy, baseFinalDamage);
            }
            
            //적에게 피해 입히기
            enemy.OnDamage(realFinalDamage);
            
            //(선택, 추가) 이펙트, 타격음 추가
            //EffectManager.Instance.PlayHitEffect(currentTarget.Position);
            Debug.Log($"{unitData.EntityName}이 {enemy.Data.EntityName}을 공격!");
        }
    }
    // [UI] 선택 상태를 켜고 끄는 함수
    public void SetSelected(bool isSelected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(isSelected);
        }
    }

    
    // --- 외부 명령 메서드 --
    // 강제 이동 M
    public void OrderMove(Vector3 destination)
    {
        currentState = UnitState.Move;
        agent.isStopped = false;
        
        agent.stoppingDistance = 0.1f;
        
        agent.SetDestination(destination);
        currentTarget = null; //타겟 무시
    }
    //어택땅 A
    public void OrderAttackMove(Vector3 destination)
    {
        currentState = UnitState.AttackMove;
        attackMoveDest = destination;
        agent.isStopped = false;
        agent.stoppingDistance = 0.1f;
        agent.SetDestination(destination);
        currentTarget = null;
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

