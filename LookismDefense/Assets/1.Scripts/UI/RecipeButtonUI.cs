using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TooltipTrigger))]
public class RecipeButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultNameText;
    [SerializeField] private Button combineButton;
    [SerializeField] private Image resultImage;

    private TooltipTrigger tooltipTrigger;
    private CombinationRecipe myRecipe;

    private void Awake()
    {
        tooltipTrigger = GetComponent<TooltipTrigger>();
    }
    public void Setup(CombinationRecipe recipe)
    {
        myRecipe = recipe;
        resultNameText.text = recipe.ResultUnit.EntityName;
        resultImage.sprite = recipe.ResultUnit.PortraitIcon; 
        
        // 재료 텍스트 생성 (예: 박형석(1) + 이진성(1)")
        string ingredientString = $"<b><color=orange>{recipe.ResultUnit.EntityName} 조합법</color></b>\n\n";
        foreach (Ingredient ingredient in recipe.Ingredients)
        {
            ingredientString += $"{ingredient.unit.EntityName} x ({ingredient.count})";
        }
        tooltipTrigger.content = ingredientString;
        
        //버튼 클릭 시 조합 시도 연결
        combineButton.onClick.RemoveAllListeners();
        combineButton.onClick.AddListener(() =>
        {
            CombinationManager.Instance.TryCombine(myRecipe);
        });
    }

}
