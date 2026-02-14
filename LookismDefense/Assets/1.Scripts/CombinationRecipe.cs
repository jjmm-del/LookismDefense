using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Ingredient
{
    public UnitData unit; //필요한 유닛
    public int count; // 필요한 개수
}
[CreateAssetMenu(fileName = "NewRecipe", menuName = "LookismDefense/CombinationRecipe")]
public class CombinationRecipe : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string recipeName;
    
    [Header("Requirements")]
    [SerializeField] private List<Ingredient> ingredients;
        
    [Header("Results")]
    [SerializeField] private UnitData resultUnit; //결과 유닛
    
    public List<Ingredient> Ingredients => ingredients;
    public UnitData ResultUnit => resultUnit;
    
   
}
