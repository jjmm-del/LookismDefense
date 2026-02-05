using UnityEngine;

public class UnitEntity : MonoBehaviour
{
    private UnitData _unitData;
    private float lastAttackTime;
    public UnitData Data => _unitData;
    public void Initialize(UnitData unitData)
    {
        _unitData = unitData;
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
}

