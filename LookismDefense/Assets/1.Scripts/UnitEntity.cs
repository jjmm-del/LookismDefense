using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitEntity : MonoBehaviour
{
    [SerializeField]private UnitData unitData;
    public UnitData Data => unitData;

    private NavMeshAgent agent;
    private float lastAttackTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    public void Initialize(UnitData data)
    {
        unitData = data;
        if (agent != null)
        {
            agent.speed = unitData.MoveSpeed;
        }
    }

    private void Update()
    {
        if (Time.deltaTime >= 0)
        {
            PerformAttack();
            lastAttackTime -= Time.deltaTime;
        }
    }

    private void PerformAttack()
    {
        Debug.Log("1");
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent != null)
        {
            agent.SetDestination(destination);
        }
    }
}

