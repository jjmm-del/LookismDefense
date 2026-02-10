using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "LookismDefense/CombinationRecipe")]
public class CombinationRecipe : ScriptableObject
{
    [Header("Ingredients")]
    [SerializeField] private UnitData materialUnitA;
    [SerializeField] private UnitData materialUnitB;
    
    [Header("Results")]
    [SerializeField] private UnitData resultUnit;
    
    public UnitData MaterialUnitA => materialUnitA;
    public UnitData MaterialUnitB => materialUnitB;
    public UnitData ResultUnit => resultUnit;
}
