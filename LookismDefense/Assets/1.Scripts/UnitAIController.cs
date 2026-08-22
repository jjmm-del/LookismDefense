using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

[RequireComponent(typeof(UnitEntity))]
[RequireComponent(typeof(NavMeshAgent))]
public class UnitAIController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private LayerMask enemyLayer;
    
    [Tooltip("적을 발견할 수 있는 거리")]
    [SerializeField] private float detectionRange = 5f;
    
    [Tooltip("HomeCell 에서 이 거리 이상 벗어나면 추격 중단")]
    [SerializeField] private float leashRange = 7f;
    
    [Tooltip("적 탐색 주기. 매 프레임 OverlapSphere 하지 않기 위함")]
    [SerializeField] private float scanInterval = 0.2f;

    [Header("Return Settings")]
    [SerializeField] private float homeArrivalDistance = 0.15f;

    private UnitEntity unit;
    private NavMeshAgent agent;

    private EnemyEntity currentTarget;
    private GridCell homeCell;
    
    private UnitAIState currentState = UnitAIState.Idle;

    private float nextScanTime;
    
    public UnitAIState CurrentState => currentState;
    public GridCell HomeCell => homeCell;
    public EnemyEntity CurrentTarget => currentTarget;

    private void Awake()
    {
        unit = GetComponent<UnitEntity>();
        agent = GetComponent<NavMeshAgent>();
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
    }

    private void UpdateIdle()
    {
        StopAgent();

        if (Time.time < nextScanTime)
            return;

        nextScanTime = Time.time + scanInterval;

        //Debug.Log($"[{name}] Idle 탐색 실행 / Home = {homeCell.WorldPosition}");

        currentTarget = FindTarget();

        if (currentTarget != null)
        {
            //Debug.Log($"[{name}] 새로운 타겟 발견 : {currentTarget.name}");

            ChangeState(UnitAIState.Chase);
        }
        else
        {
            //Debug.Log($"[{name}] 탐색했지만 적 없음");
        }
    }

    private void UpdateChase()
    {
        if (currentTarget == null)
        {
            StartReturn();
            return;
        }

        float distanceFromHome = Vector3.Distance(
            homeCell.WorldPosition,
            transform.position
        );

        if (distanceFromHome > leashRange)
        {
            ClearTarget();
            StartReturn();
            return;
        }

        float distanceToTarget = Vector3.Distance(
            transform.position,
            currentTarget.transform.position
        );
        if (distanceToTarget <= unit.AttackRange)
        {
            ChangeState(UnitAIState.Attack);
            return;
        }

        MoveTo(currentTarget.transform.position);
    }

    private void UpdateAttack()
    {
        if (currentTarget == null)
        {
            StartReturn();
            return;
        }
        float targetDistance = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (targetDistance > unit.AttackRange)
        {
            ChangeState(UnitAIState.Chase);
            return;
        }

        float homeDistance = Vector3.Distance(homeCell.WorldPosition, transform.position);
        if (homeDistance > leashRange)
        {
            ClearTarget();
            StartReturn();
            return;
        }

        StopAgent();
        LookAtTarget();

        unit.TryAttack(currentTarget);
    }

    private void UpdateReturn()
    {
        if (!agent.isOnNavMesh)
            return;

        if (!agent.hasPath)
        {
            MoveTo(homeCell.WorldPosition);
            return;
        }

        if (agent.pathPending)
            return;
        
        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            StopAgent();
            ChangeState(UnitAIState.Idle);
        }
    }

    private EnemyEntity FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            homeCell.WorldPosition,
            detectionRange,
            enemyLayer
        );
        //Debug.Log($"[{name}] OverlapSphere 감지 Collider 수 : {hits.Length}");
        
        EnemyEntity nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            EnemyEntity enemy = hit.GetComponentInParent<EnemyEntity>();

            if (enemy == null || enemy.CurrentHealth <= 0)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
                );
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    private void ValidateTarget()
    {
        if (currentTarget == null)
        {
            if (currentState == UnitAIState.Chase || currentState == UnitAIState.Attack)
            {
                if (!TryFindNextTarget())
                {
                    StartReturn();
                }
            }

            return;
        }

        if (!currentTarget.gameObject.activeInHierarchy || currentTarget.CurrentHealth<=0)
        {
            if (!TryFindNextTarget())
            {
                StartReturn();
            }
            
            
        } 
    }

    private void StartReturn()
    {
        ClearTarget();
        ChangeState(UnitAIState.Return);
        MoveTo(homeCell.WorldPosition);
    }

    private void ClearTarget()
    {
        currentTarget = null;
    }

    private void MoveTo(Vector3 destination)
    {
        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.stoppingDistance = 0.05f;
        agent.SetDestination(destination);
    }

    private void StopAgent()
    {
        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    private void LookAtTarget()
    {
        if (currentTarget == null)
            return;

        Vector3 direction = currentTarget.transform.position - transform.position;

        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f) 
            return;
        
        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private void ChangeState(UnitAIState newState)
    {
        if (currentState == newState)
            return;
        
        //Debug.Log($"[{name}] State : {currentState} -> {newState}");
        currentState = newState;

        switch (currentState)
        {
            case UnitAIState.Idle:
                currentTarget = null;
                nextScanTime = 0f;
                break;
            case UnitAIState.Return:
                currentTarget = null;
                break;
        }
    }

    private bool TryFindNextTarget()
    {
        EnemyEntity nextTarget = FindTarget();

        if (nextTarget == null)
            return false;

        currentTarget = nextTarget;
        ChangeState(UnitAIState.Chase);

        return true;
    }
    private void OnDrawGizmosSelected()
    {
        Vector3 center = homeCell != null ? homeCell.WorldPosition : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, leashRange);
    }
}
