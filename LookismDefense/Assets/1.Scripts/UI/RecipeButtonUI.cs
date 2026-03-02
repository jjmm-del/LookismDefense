using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RecipeButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultNameText;
    [SerializeField] private TextMeshProUGUI ingredientsText;
    [SerializeField] private Button combineButton;
    [SerializeField] private Image resultImage;

    private CombinationRecipe myRecipe;

    public void Setup(CombinationRecipe recipe)
    {
        myRecipe = recipe;
        resultNameText.text = recipe.ResultUnit.EntityName;
        
        // 재료 텍스트 생성 (예: 박형석(1) + 이진성(1)")
        string ingredientString = "";
        foreach (Ingredient ingredient in recipe.Ingredients)
        {
            ingredientString += $"{ingredient.unit.EntityName}({ingredient.count})";
        }
        ingredientsText.text = ingredientString;
        resultImage.sprite = recipe.ResultUnit.PortraitIcon; 
        
        //버튼 클릭 시 조합 시도 연결
        combineButton.onClick.RemoveAllListeners();
        combineButton.onClick.AddListener(() =>
        {
            CombinationManager.Instance.TryCombine(myRecipe);
        });
    }

}
