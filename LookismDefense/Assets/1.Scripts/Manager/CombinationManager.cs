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
        
        // 생성될 위치 저장
        Vector3 targetPos = selectedUnits[0].transform.position;
        
        foreach (CombinationRecipe recipe in allRecipes)
        {
            if (CheckRecipeMatch(recipe, data1, data2))
            {
                CreateUnit(recipe.ResultUnit,targetPos);
                DestroyIngredients(selectedUnits);
                
                //조합 후 선택 목록 초기화(이미 죽은 유닛 참조하지 않도록)
                selectedUnits.Clear();
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

    private void CreateUnit(UnitData newUnitData, Vector3 spawnPosition)
    {
        if (newUnitData.Prefab == null) return;
        
        // 1. 결과 유닛 생성
        GameObject newUnit = Instantiate(newUnitData.Prefab, spawnPosition, Quaternion.identity);
        
        // 2. 데이터 주입
        UnitEntity entity = newUnit.GetComponent<UnitEntity>();
        if (entity != null)
        {
            entity.Initialize(newUnitData);
        }
        
        Debug.Log($"조합 성공!{newUnitData.EntityName}생성");
        //(선택 사항) 조합 성공 이펙트 재생 코드 추가 위치 
    }

    private void DestroyIngredients(List<UnitEntity> ingredients)
    {
        foreach (UnitEntity unit in ingredients)
        {
            Destroy(unit.gameObject);
        }
    }
}
