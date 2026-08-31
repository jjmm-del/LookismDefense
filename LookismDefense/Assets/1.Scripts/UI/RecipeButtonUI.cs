using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeButtonUI : MonoBehaviour
{
   // [SerializeField] private TextMeshProUGUI resultNameText;
    [SerializeField] private Button combineButton;
    [SerializeField] private Image resultImage;

    private TooltipTrigger tooltipTrigger;
    private CombinationRecord myRecipe;
    
    
    public void Setup(CombinationRecord recipe)
    {
        myRecipe = recipe;
        
        if (!TryGetComponent(out tooltipTrigger))
        {
            tooltipTrigger = gameObject.AddComponent<TooltipTrigger>();
        }
        
        combineButton.onClick.RemoveAllListeners();
        
        if (recipe == null || GameDatabase.Instance == null ||
            !GameDatabase.Instance.TryGetUnit(recipe.resultUnitId, out UnitRecord resultUnit))
        {
            combineButton.interactable = false;
            
            if(resultImage!= null)
            {
                resultImage.sprite = null;
                resultImage.gameObject.SetActive(false);
            }
            
            return;
        }

        combineButton.interactable = true;

        if (resultImage != null)
        {
            Sprite portrait = UnitAssetProvider.LoadPortrait((resultUnit));
            resultImage.sprite = portrait;
            resultImage.gameObject.SetActive(portrait != null);
        }

        StringBuilder tooltipText = new StringBuilder();
        tooltipText.AppendLine(
            $"<b><color=orange>" +
            $"{resultUnit.DisplayName}조합법" +
            $"</color></b>"
        );
        //tooltipText.AppendLine();
        
        foreach (RecipeIngredientRecord ingredient in recipe.ingredients)
        {
            string ingredientName = ingredient.unitId;

            if (GameDatabase.Instance.TryGetUnit(ingredientName, out UnitRecord ingredientUnit))
            {
                ingredientName = ingredientUnit.DisplayName;
            }

            tooltipText.AppendLine($"{ingredientName} x {ingredient.count}");
        }

        tooltipTrigger.content = tooltipText.ToString();
        
        combineButton.onClick.AddListener(() =>
        {
            CombinationManager.Instance?.TryCombine(myRecipe);
        });
    }

}
