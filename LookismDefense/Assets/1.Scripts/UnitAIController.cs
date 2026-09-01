using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UnitEntity))]
[RequireComponent(typeof(NavMeshAgent))]
public class UnitAIController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("HomeCell 중심으로 적을 처음 발견하는 거리")]
    [SerializeField] private float detectionRange = 5f;

    [Tooltip("적이 HomeCell에서 이 거리 이상 벗어나면 타겟 해제")]
    [SerializeField] private float leashRange = 7f;

    [Tooltip("적 탐색 주기")]
    [SerializeField] private float scanInterval = 0.2f;

    [Header("Movement Settings")]
    [SerializeField] private float homeArrivalDistance = 0.15f;

    [Tooltip("공격과 추격 상태가 반복되는 것을 방지하는 여유 거리")]
    [SerializeField] private float attackExitPadding = 0.35f;

    [Tooltip("공격 중 적을 바라보는 회전 속도")]
    [SerializeField] private float rotationSpeed = 10f;

    [Tooltip("움직이는 적의 경로를 다시 계산하는 주기")]
    [SerializeField] private float repathInterval = 0.1f;

    [Tooltip("적이 이 거리 이상 움직였을 때만 목적지 갱신")]
    [SerializeField] private float destinationUpdateDistance = 0.2f;

    private UnitEntity unit;
    private NavMeshAgent agent;

    private EnemyEntity currentTarget;
    private GridCell homeCell;

    private UnitAIState currentState = UnitAIState.Idle;

    private float nextScanTime;
    private float nextRepathTime;
    private Vector3 lastDestination;

    public UnitAIState CurrentState => currentState;
    public GridCell HomeCell => homeCell;
    public EnemyEntity CurrentTarget => currentTarget;

    private void Awake()
    {
        unit = GetComponent<UnitEntity>();
        agent = GetComponent<NavMeshAgent>();

        agent.autoBraking = true;
    }

    private void Update()
    {
        if (unit == null || homeCell == null)
            return;

        ValidateTarget();

        switch (currentState)
        {
            case UnitAIState.Idle:
                UpdateIdle();
                break;

            case UnitAIState.Chase:
                UpdateChase();
                break;

            case UnitAIState.Attack:
                UpdateAttack();
                break;

            case UnitAIState.Return:
                UpdateReturn();
                break;
        }
    }

    public void SetHomeCell(GridCell cell)
    {
        homeCell = cell;

        if (currentState == UnitAIState.Return &&
            homeCell != null)
        {
            MoveTo(
                homeCell.WorldPosition,
                homeArrivalDistance,
                true);
        }
    }

    private void UpdateIdle()
    {
        if (Time.time < nextScanTime)
            return;

        nextScanTime = Time.time + scanInterval;

        TryFindNextTarget();
    }

    private void UpdateChase()
    {
        if (currentTarget == null)
        {
            StartReturn();
            return;
        }

        float distanceToTarget = HorizontalDistance(
            transform.position,
            currentTarget.transform.position);

        if (distanceToTarget <= unit.AttackRange)
        {
            ChangeState(UnitAIState.Attack);
            return;
        }

        float stoppingDistance = Mathf.Max(
            0.1f,
            unit.AttackRange * 0.9f);

        MoveTo(
            currentTarget.transform.position,
            stoppingDistance);
    }

    private void UpdateAttack()
    {
        if (currentTarget == null)
        {
            if (!TryFindNextTarget())
            {
                StartReturn();
            }

            return;
        }

        float targetDistance = HorizontalDistance(
            transform.position,
            currentTarget.transform.position);

        float attackExitDistance =
            unit.AttackRange + attackExitPadding;

        if (targetDistance > attackExitDistance)
        {
            // 현재 위치에서 바로 때릴 수 있는 다른 적을 먼저 찾는다.
            EnemyEntity replacement =
                FindTargetInAttackRange();

            if (replacement != null)
            {
                currentTarget = replacement;
            }
            else
            {
                ChangeState(UnitAIState.Chase);
                return;
            }
        }

        // StopAgent는 Attack 상태 진입 시 한 번만 실행한다.
        SmoothLookAtTarget();

        unit.TryAttack(currentTarget);
    }

    private void UpdateReturn()
    {
        // 귀환 중에도 적을 계속 찾는다.
        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + scanInterval;

            if (TryFindNextTarget())
            {
                return;
            }
        }

        if (!agent.isOnNavMesh)
            return;

        if (!agent.hasPath)
        {
            MoveTo(
                homeCell.WorldPosition,
                homeArrivalDistance,
                true);

            return;
        }

        if (agent.pathPending)
            return;

        float arrivalDistance = Mathf.Max(
            homeArrivalDistance,
            agent.stoppingDistance);

        if (agent.remainingDistance <= arrivalDistance)
        {
            // Idle 상태 진입 시 경로를 한 번만 제거한다.
            ChangeState(UnitAIState.Idle);
        }
    }

    private void ValidateTarget()
    {
        if (IsValidCombatTarget(currentTarget))
            return;

        currentTarget = null;

        if (currentState != UnitAIState.Chase &&
            currentState != UnitAIState.Attack)
        {
            return;
        }

        if (!TryFindNextTarget())
        {
            StartReturn();
        }
    }

    private bool TryFindNextTarget()
    {
        EnemyEntity nextTarget =
            FindTargetInDetectionRange();

        if (nextTarget == null)
            return false;

        currentTarget = nextTarget;

        float distance = HorizontalDistance(
            transform.position,
            currentTarget.transform.position);

        if (distance <= unit.AttackRange)
        {
            ChangeState(UnitAIState.Attack);
        }
        else
        {
            ChangeState(UnitAIState.Chase);
        }

        return true;
    }

    private EnemyEntity FindTargetInDetectionRange()
    {
        return FindNearestTarget(
            homeCell.WorldPosition,
            detectionRange);
    }

    private EnemyEntity FindTargetInAttackRange()
    {
        return FindNearestTarget(
            transform.position,
            unit.AttackRange);
    }

    private EnemyEntity FindNearestTarget(
        Vector3 center,
        float radius)
    {
        Collider[] hits = Physics.OverlapSphere(
            center,
            radius,
            enemyLayer);

        EnemyEntity nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            EnemyEntity enemy =
                hit.GetComponentInParent<EnemyEntity>();

            if (!IsValidCombatTarget(enemy))
                continue;

            float distance = HorizontalDistance(
                transform.position,
                enemy.transform.position);

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearestEnemy = enemy;
        }

        return nearestEnemy;
    }

    private bool IsValidCombatTarget(EnemyEntity target)
    {
        if (target == null ||
            !target.gameObject.activeInHierarchy ||
            target.CurrentHealth <= 0)
        {
            return false;
        }

        float distanceFromHome = HorizontalDistance(
            homeCell.WorldPosition,
            target.transform.position);

        return distanceFromHome <= leashRange;
    }

    private void StartReturn()
    {
        ChangeState(UnitAIState.Return);
    }

    private void MoveTo(
        Vector3 destination,
        float stoppingDistance,
        bool forceUpdate = false)
    {
        if (!agent.isOnNavMesh)
            return;

        bool destinationChanged =
            HorizontalDistance(
                lastDestination,
                destination) >= destinationUpdateDistance;

        if (!forceUpdate)
        {
            if (Time.time < nextRepathTime)
                return;

            if (agent.hasPath && !destinationChanged)
                return;
        }

        nextRepathTime = Time.time + repathInterval;
        lastDestination = destination;

        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(destination);
    }

    private void StopAgent(bool clearPath)
    {
        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = true;

        // Idle 진입 때만 기존 경로를 완전히 제거한다.
        if (clearPath && agent.hasPath)
        {
            agent.ResetPath();
        }
    }

    private void SmoothLookAtTarget()
    {
        if (currentTarget == null)
            return;

        Vector3 direction =
            currentTarget.transform.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    private void ChangeState(UnitAIState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (currentState)
        {
            case UnitAIState.Idle:
                currentTarget = null;

                agent.updateRotation = true;

                // 귀환 완료 시 한 번만 ResetPath
                StopAgent(true);

                nextScanTime =
                    Time.time + scanInterval;
                break;

            case UnitAIState.Chase:
                agent.updateRotation = true;
                nextRepathTime = 0f;

                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }
                break;

            case UnitAIState.Attack:
                // 공격 중에는 직접 부드럽게 회전한다.
                agent.updateRotation = false;

                // 경로는 유지하고 이동만 정지한다.
                StopAgent(false);
                break;

            case UnitAIState.Return:
                currentTarget = null;
                agent.updateRotation = true;
                nextRepathTime = 0f;
                nextScanTime =
                    Time.time + scanInterval;

                MoveTo(
                    homeCell.WorldPosition,
                    homeArrivalDistance,
                    true);
                break;
        }
    }

    private static float HorizontalDistance(
        Vector3 first,
        Vector3 second)
    {
        first.y = 0f;
        second.y = 0f;

        return Vector3.Distance(first, second);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center =
            homeCell != null
                ? homeCell.WorldPosition
                : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            center,
            detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            center,
            leashRange);
    }
}