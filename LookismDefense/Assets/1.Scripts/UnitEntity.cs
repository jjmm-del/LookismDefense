using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

    
[RequireComponent(typeof(NavMeshAgent))]
public class UnitEntity : MonoBehaviour
{
    [SerializeField]private UnitData unitData;

    private UnitRecord runtimeData;
    public UnitData Data => unitData;
    public UnitRecord RuntimeData => runtimeData;    
    
    
    
    [Header("Attack Settings")]
    [SerializeField] private LayerMask enemyLayer; // 적 레이어(설정 필수)
    
    [Header("UI & Visuals")]
    [SerializeField] private GameObject selectionIndicator; //초록색 원
    
    private NavMeshAgent agent;
    private AbilityController abilityController;
    private float lastAttackTime;
    private Transform currentTarget; //현재 공격 대상
    
    private float buffDamageMultiplier = 1f;
    private float buffSpeedMultiplier = 1f;
    private float buffDamageTimer = 0f;
    private float buffSpeedTimer = 0f;

    //스탯 ( 버프 적용 등을 위해 변수로 관리)
    private float currentAttackDamage;
    private float currentAttackRange;
    private float currentAttackSpeed;

    public string DisplayName
    {
        get
        {
            if (runtimeData != null)
                return runtimeData.DisplayName;

            if (unitData != null)
            {
                return string.IsNullOrEmpty(unitData.Title)
                    ? unitData.EntityName
                    : $"[{unitData.Title}]{unitData.EntityName}";
                
            }

            return gameObject.name;
        }
    }
    public float AttackDamage => currentAttackDamage;
    public float AttackRange => currentAttackRange;

    public float AttackSpeed => currentAttackSpeed;

    public UnitTier Tier => runtimeData != null ? runtimeData.tier : unitData != null ? unitData.Tier : default;

    public int MaxAttackTargets => runtimeData != null ? runtimeData.maxAttackTargets :
        unitData != null ? unitData.MaxAttackTargets : 1;
    
    
    
    //상태 관리 변수
    //private UnitState currentState = UnitState.Idle;
    //private Vector3 attackMoveDest; //어택땅 목적지 기억용
    
    // 순간이동 관련 변수
    private Vector3 homePosition;
    public bool IsInStoryZone { get; private set; } = false;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        abilityController = GetComponent<AbilityController>();
    }
    public void Initialize(UnitData data)
    {
        // [초기화 ] 유닛 데이터
        runtimeData = null;
        unitData = data;

        // [초기화] 생성시 유닛 데이터의 기본 스탯;
        currentAttackDamage = data.AttackDamage;
        currentAttackRange = data.AttackRange;
        currentAttackSpeed = data.AttackSpeed;
        
        // [초기화] 이동속도
        if (agent != null)
        {
            agent.speed = data.MoveSpeed;
            
        }
        // [초기화] 유닛 특수능력
        if (abilityController != null)
        {
            abilityController.Initialize(data.Abilities, this);
        }
    }

    public void Initialize(UnitRecord data)
    {
        runtimeData = data;
        // [초기화] 생성시 유닛 데이터의 기본 스탯;
        currentAttackDamage = data.attackDamage;
        currentAttackRange = data.attackRange;
        currentAttackSpeed = data.attackSpeed;

        if (unitData != null)
        {
            // [초기화] 이동속도
            if (agent != null)
            {
                agent.speed = unitData.MoveSpeed;
            
            }
            // [초기화] 유닛 특수능력
            if (abilityController != null)
            {
                abilityController.Initialize(unitData.Abilities, this);
            }
        }
        else if (abilityController != null)
        {
            abilityController.Initialize(null, this);
        }
        
        
        
    }

    private void Start()
    {
        //태어날 때 게임매니저에 나를 등록
        if (EntityRegistry.Instance != null)
        {
            EntityRegistry.Instance.RegisterUnit(this);
            Debug.Log($"{this.DisplayName} 등록");
        }
    }

    // public void SetHomeCell(GridCell cell)
    // {
    //     HomeCell = cell;
    // }
    
    private void Update()
    {
        //버프 타이머 체크
        if (buffDamageMultiplier > 1f)
        {
            buffDamageTimer -= Time.deltaTime;
            if (buffDamageTimer <= 0)
            {
                buffDamageMultiplier = 1f;
            }
        }

        if (buffSpeedMultiplier > 1f)
        {
            buffSpeedTimer -= Time.deltaTime;
            if (buffSpeedTimer <= 0)
            {
                buffSpeedMultiplier = 1f;
            }
        }
    }


    public void TryAttack(EnemyEntity target)
    {
        if (target == null)
            return;
        
        float attackCooldown = 1f / (currentAttackSpeed * buffSpeedMultiplier);

        if (Time.time < lastAttackTime + attackCooldown)
            return;
        
        PerformAttack(target);
        lastAttackTime = Time.time;
        
    }
    

    public void PerformAttack(EnemyEntity primaryEnemy)
    {
        if (primaryEnemy == null) 
            return;
        
        // 업그레이드 적용 데미지 계산
        float baseFinalDamage = UpgradeManager.Instance.GetFinalDamage(currentAttackDamage, Tier) * buffDamageMultiplier;
        
        // 공격할 타겟 리스트 만들기
        List<EnemyEntity> targetsToHit = new();
        
        targetsToHit.Add(primaryEnemy);
        
        // 다중 공격 처리
        if (MaxAttackTargets > 1)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, currentAttackRange, enemyLayer);
            foreach (Collider hit in hits)
            {
                if (targetsToHit.Count >= MaxAttackTargets)
                {
                    break; // 최종 타겟 수 도달 시 종료
                }

                EnemyEntity enemy = hit.GetComponent<EnemyEntity>();
                
                if (enemy != null &&
                    enemy != primaryEnemy&&
                    !targetsToHit.Contains(enemy))
                {
                    targetsToHit.Add(enemy);
                }
            }
        }
        
        
        foreach (EnemyEntity target in targetsToHit)
        {
            // 특수능력 적용
            float finalDamage = baseFinalDamage;
            if (abilityController != null)
            {
                finalDamage = abilityController.ProcessOnHitAbilities(target, baseFinalDamage);
            }
            
            //적에게 피해 입히기
            target.OnDamage(finalDamage);
            
            //(선택, 추가) 이펙트, 타격음 추가
            //EffectManager.Instance.PlayHitEffect(currentTarget.Position);
            Debug.Log($"{DisplayName}이 {primaryEnemy.Data.EntityName}을 공격!");
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
    public void ApplyAttackDamageBuff(float percent, float duration)
    {
        buffDamageMultiplier = 1f + (percent / 100f);
        buffDamageTimer = duration;
    }
    // [추가 기능] 나중에 업그레이드 시스템에서 호출할 함수
    public void ApplyAttackSpeedBuff(float percent, float duration)
    {
        buffSpeedMultiplier = 1f + (percent / 100f);
        buffSpeedTimer = duration;
    }

    private void OnDestroy()
    {
        UnitAIController ai = GetComponent<UnitAIController>();

        GridCell homeCell = ai?.HomeCell;

        if (homeCell != null && homeCell.OccupiedUnit == gameObject)
        {
            homeCell.RemoveUnit();
        }
        if (EntityRegistry.Instance != null)
        {
            EntityRegistry.Instance.UnregisterUnit(this);
        }
    }
}

