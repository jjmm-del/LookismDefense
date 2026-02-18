using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CombinationManager : MonoBehaviour
{
    public static CombinationManager Instance { get; private set; }
    [SerializeField] private List<CombinationRecipe> allRecipes;
    [SerializeField] private Transform CombinationZone;
    //현재 플레이어가 보유한 유닛 리스트(GameManger 등에서 관리하는 리스트 참조)
    //여기서는 예시로 로컬 리스트를 사용한다고 가정
    // private List<UnitEntity> myUnits;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    //외부(GameManager)에서 유닛 리스트를 업데이트 해줘야함)
    // public void UpdateUnitList(List<UnitEntity> units)
    // {
    //     myUnits = units;
    // }
    
    //[핵심1] 특정 유닛으 재료로 쓰는 레시피 찾기(UI 표시용)
    public List<CombinationRecipe> GetRecipesForUnit(UnitData unit)
    {
        List<CombinationRecipe> availableRecipes = new List<CombinationRecipe>();

        foreach (CombinationRecipe recipe in allRecipes)
        {
            //이 레시피의 재료 중에 내가 선택한 유닛이 포함되어 있는가?
            bool isRelated = recipe.Ingredients.Any(ing => ing.unit == unit);
            if (isRelated)
            {
                availableRecipes.Add(recipe);
            }
        }

        return availableRecipes;
    }
    
    //[핵심2] 실제로 조합 시도(UI 버튼 클릭 시 호출)
    public void TryCombine(CombinationRecipe recipe)
    {
        // 1. 재료가 충분한지 검사
        if (!HasEnoughIngredients(recipe))
        {
            Debug.Log("재료가 부족합니다!");
            return;
        }
        // 2. 재료 소조(유닛 파괴)
        ConsumeIngredients(recipe);
        
        // 3. 결과 유닛 생성
        SpawnResultUnit(recipe.ResultUnit, CombinationZone.position);
    }

    private bool HasEnoughIngredients(CombinationRecipe recipe)
    {
        List<UnitEntity> myUnits = GameManager.Instance.PlayerUnits;
        if (myUnits == null || myUnits.Count == 0)
        {
            Debug.LogError("오류: GameManager에 등록된 유닛이 0마리입니다.!");
            return false;
        }
        
        
        Dictionary<UnitData,int> unitCounts = new Dictionary<UnitData, int>();
        // 현재 필드 유닛 카운트
        foreach (UnitEntity unit in myUnits)
        {
            if (unit.Data == null)
            {
                Debug.LogWarning($"경고{unit.name}유닛의 Data가 비어있습니다.");
                continue;
            }
            
            if (!unitCounts.ContainsKey(unit.Data)) unitCounts[unit.Data] = 0;
            unitCounts[unit.Data]++;
        }
        
        //레시피 요구량과 비교
        Debug.Log($"---{recipe.ResultUnit.EntityName} 조합 시도 중 ---");
        foreach (Ingredient ingredient in recipe.Ingredients)
        {
            int currentCount = 0;
            if (unitCounts.ContainsKey(ingredient.unit))
            {
                currentCount = unitCounts[ingredient.unit];
            }
            
            //[범인 색출 Log]
            Debug.Log($"재료 검사:[필요]{ingredient.unit.EntityName}x{ingredient.count}/ [보유]{currentCount}");
            if (currentCount < ingredient.count)
            {
                Debug.LogError($"[조합 실패 원인] {ingredient.unit.EntityName}유닛이 부족 (필요: {ingredient.count} 보유:{currentCount}");
                
                //혹시 이름은 같은데 인식이 안되는 경우인지 확인
                foreach (var key in unitCounts.Keys)
                {
                    if (key.EntityName == ingredient.unit.EntityName && key != ingredient.unit)
                    {
                        Debug.LogError($"[치명적 오류] 이름이 같은 {key.EntityName} 데이터가 2개 존재합니다.");
                    }
                }

                return false; 
            }
        } 
        return true; //충분함
    }

    private void SpawnResultUnit(UnitData newUnitData, Vector3 spawnPosition)
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

    private void ConsumeIngredients(CombinationRecipe recipe)
    {
        List<UnitEntity> myUnits = GameManager.Instance.PlayerUnits;
        foreach (Ingredient ingredient in recipe.Ingredients)
        {
            int countToRemove = ingredient.count;
            
            //리스트를 뒤로 돌면서 삭제(안전한 삭제)
            for (int i = myUnits.Count - 1; i >= 0; i--)
            {
                if (countToRemove <= 0) break;
                
                if(myUnits[i].Data == ingredient.unit)
                {
                    //파괴로직
                    UnitEntity unitToRemove = myUnits[i];
                    myUnits.RemoveAt(i); //리스트에서 제거
                    Destroy(unitToRemove.gameObject); //오브젝트 파괴
                    countToRemove--;
                }
            }
        }
    }
}
