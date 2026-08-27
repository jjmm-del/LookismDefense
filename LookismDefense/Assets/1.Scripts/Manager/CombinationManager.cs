using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class CombinationManager : MonoBehaviour
{
    public static CombinationManager Instance { get; private set; }
    [SerializeField] private List<CombinationRecipe> allRecipes;

    private UnitFactory unitFactory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        unitFactory = new UnitFactory();
    }

    public IReadOnlyList<CombinationRecord> GetRecipesForUnit(UnitEntity unit)
    {
        if (unit == null || string.IsNullOrWhiteSpace(unit.UnitId) || GameDatabase.Instance == null ||
            !GameDatabase.Instance.AreRecipesReady)
        {
            return Array.Empty<CombinationRecord>();
        }

        return GameDatabase.Instance.GetRecipesForMainUnit(unit.UnitId);
    }
    
    //[핵심1] 특정 유닛으 재료로 쓰는 레시피 찾기(UI 표시용)
    public List<CombinationRecipe> GetRecipesForUnit(UnitData unit)
    {
        List<CombinationRecipe> availableRecipes = new();

        foreach (CombinationRecipe recipe in allRecipes)
        {
            // 방어 코드: 재료가 하나도 세팅되지 않은 레시피는 무시
            if (recipe.Ingredients == null || recipe.Ingredients.Count == 0)
            {
                continue;
            }
            
            //이 레시피의 재료 중에 첫번째 유닛이 내가 선택한 유닛인가?
            Ingredient mainIngredient = recipe.Ingredients[0];
            
            if (mainIngredient.unit == unit)
            {
                availableRecipes.Add(recipe);
            }
        }

        return availableRecipes;
    }

    private bool TryGetIngredients(CombinationRecipe recipe, out List<UnitEntity> ingredients)
    {
        List<UnitEntity> collectedIngredients = new();

        if (EntityRegistry.Instance == null)
        {
            ingredients = collectedIngredients;
            return false;
        }

        IReadOnlyList<UnitEntity> ownedUnits = EntityRegistry.Instance.PlayerUnits;

        foreach (Ingredient requirement in recipe.Ingredients)
        {
            List<UnitEntity> matches = ownedUnits
                                       .Where(unit =>
                                           unit != null && unit.Data == requirement.unit && !collectedIngredients.Contains(unit))
                                       .Take(requirement.count)
                                       .ToList();

            if (matches.Count < requirement.count)
            {
                Debug.Log($"{requirement.unit.EntityName}부족 ({matches.Count}/{requirement.count}");
                
                ingredients = collectedIngredients;
                return false;
            }
            collectedIngredients.AddRange(matches);
        }
        ingredients = collectedIngredients;
        return true;

    }

    public void TryCombine(CombinationRecord recipe)
    {
        if (recipe == null || !recipe.enabled)
            return;

        if (GameDatabase.Instance == null)
        {
            Debug.LogError("[CombinationManager] GameDatabase가 없습니다.");
            return;
        }

        if (!GameDatabase.Instance.TryGetUnit(recipe.resultUnitId, out UnitRecord resultData))
        {
            Debug.LogError($"[CombinationManager] 결과 유닛이 없습니다.:{recipe.resultUnitId}");
            return;
        }

        if (!TryGetIngredients(recipe, out List<UnitEntity> ingredients))
        {
            Debug.Log("[CombinationManager] 재료가 부족합니다.");
            return;
        }

        if (ingredients.Count == 0)
            return;
        UnitAIController anchorAI = ingredients[0].GetComponent<UnitAIController>();
        if (anchorAI == null || anchorAI.HomeCell == null)
        {
            Debug.LogError("[CombinationManager] 결과를 배치할 HomeCell을 찾을 수 없습니다.");
            return;
        }

        GridCell resultCell = anchorAI.HomeCell;
        if (!TryCreateResult(resultData, ingredients, resultCell))
        {return;}
        
        Debug.Log($"[CombinationManager] 조합 성공: {resultData.DisplayName}");
    }

    private bool TryGetIngredients(CombinationRecord recipe, out List<UnitEntity> ingredients)
    {

        List<UnitEntity> collectedIngredients = new();

        if (EntityRegistry.Instance == null)
        {
            ingredients = collectedIngredients;
            return false;
        }
        
        IReadOnlyList<UnitEntity> ownedUnits = EntityRegistry.Instance.PlayerUnits;
        
        foreach (RecipeIngredientRecord requirement in recipe.ingredients)
        {
            List<UnitEntity> matches = ownedUnits
                                       .Where(unit =>
                                           unit != null && string.Equals(unit.UnitId,requirement.unitId, StringComparison.OrdinalIgnoreCase
                                           ) &&
                                           !collectedIngredients.Contains(unit))
                                       .Take(requirement.count)
                                       .ToList();

            if (matches.Count < requirement.count)
            {
                Debug.Log($"[CombinationManager]{GetUnitDisplayName(requirement.unitId)}부족 ({matches.Count}/{requirement.count}");
                
                ingredients = collectedIngredients;
                return false;
            }
            collectedIngredients.AddRange(matches);
        }
        ingredients = collectedIngredients;
        return true;
    }

    private bool TryCreateResult(UnitRecord resultData, List<UnitEntity> ingredients, GridCell resultCell)
    {
        UnitEntity anchorUnit = ingredients[0];

        resultCell.RemoveUnit();
        if (!unitFactory.TryCreate(resultData, resultCell, out _))
        {
            resultCell.TryPlaceUnit(anchorUnit.gameObject);
            Debug.LogError($"[CombinationManager] 결과 생성 실패 - 조합 취소");
            return false;
        }
        ConsumeIngredients(ingredients, resultCell);
        return true;
    }
    //[핵심2] 실제로 조합 시도(UI 버튼 클릭 시 호출) /  CombinationRecipe 사용
    public void TryCombine(CombinationRecipe recipe)
    {
        if (recipe == null || recipe == null)
            return;
        
        // 1. 재료가 충분한지 검사
        if (!TryGetIngredients(recipe, out List<UnitEntity> ingredients))
        {
            Debug.Log("재료가 부족합니다!");
            return;
        }

        if (ingredients.Count == 0)
            return;
        
        UnitAIController anchorAI = ingredients[0].GetComponent<UnitAIController>();

        if (anchorAI == null || anchorAI.HomeCell == null)
        {
            Debug.LogError("조합 결과를 배치할 GridCell을 찾을 수 없습니다.");
            return;
        }

        GridCell resultCell = anchorAI.HomeCell;

        if (!TryCreateResult(recipe.ResultUnit, ingredients, resultCell))
        {
            return;
        }
        Debug.Log($"조합 성공:{recipe.ResultUnit.EntityName}");
    }

    private bool TryCreateResult(UnitData resultData, List<UnitEntity> ingredients, GridCell resultCell)
    {
        UnitEntity anchorUnit = ingredients[0];

        resultCell.RemoveUnit();

        if (!unitFactory.TryCreate(resultData, resultCell, out UnitEntity resultUnit))
        {
            resultCell.TryPlaceUnit(anchorUnit.gameObject);
            Debug.LogError("조합 결과 생성 실패 - 조합 취소");
            return false;
        }

        ConsumeIngredients(ingredients, resultCell);
        return true;
    }

    private void ConsumeIngredients(List<UnitEntity> ingredients, GridCell resultCell)
    {
        foreach (UnitEntity unit in ingredients)
        {
            if (unit == null)
                continue;

            UnitAIController ai = unit.GetComponent<UnitAIController>();

            GridCell cell = ai?.HomeCell;

            if (cell != null && cell != resultCell && cell.OccupiedUnit == unit.gameObject)
            {
                cell.RemoveUnit();
            }
            EntityRegistry.Instance?.UnregisterUnit(unit);
            
            Destroy(unit.gameObject);
        }
    }

    private string GetUnitDisplayName(string unitId)
    {
        if (GameDatabase.Instance != null && GameDatabase.Instance.TryGetUnit(unitId, out UnitRecord unit))
        {
            return unit.DisplayName;
        }

        return unitId;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
