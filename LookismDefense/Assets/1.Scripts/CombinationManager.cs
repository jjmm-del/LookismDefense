using UnityEngine;
using System.Collections.Generic;
public class CombinationManager : MonoBehaviour
{
    [SerializeField] private List<CombinationRecipe> allRecipes;

    public void TryCombine(List<UnitEntity> selectedUnits)
    {
        if (selectedUnits.Count != 2)
        {
            Debug.Log("조합하려면 유닛 2개를 선택해야합니다.");
            return;
        }

        UnitData data1 = selectedUnits[0].Data;
        UnitData data2 = selectedUnits[1].Data;

        foreach (CombinationRecipe recipe in allRecipes)
        {
            if (CheckRecipeMatch(recipe, data1, data2))
            {
                CreateUnit(recipe.ResultUnit);
                DestroyIngredients(selectedUnits);
                return;
            }
        }
        
        Debug.Log("유효한 조합법이 없습니다.");
    }

    private bool CheckRecipeMatch(CombinationRecipe recipe, UnitData data1, UnitData data2)
    {
        bool match1 = (recipe.MaterialUnitA == data1 && recipe.MaterialUnitB == data2);
        bool match2 = (recipe.MaterialUnitA == data2 && recipe.MaterialUnitB == data1);
        
        return match1||match2;
    }

    private void CreateUnit(UnitData newUnitData)
    {
        Debug.Log($"조합 성공!{newUnitData.EntityName}생성");
    }

    private void DestroyIngredients(List<UnitEntity> ingredients)
    {
        foreach (UnitEntity unit in ingredients)
        {
            Destroy(unit.gameObject);
        }
    }
}
