using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class CombinationRecord
{
    public string id;
    public string resultUnitId;
    public List<RecipeIngredientRecord> ingredients = new();
    public bool enabled;

    public string MainIngredientId
    {
        get
        {
            if (ingredients == null || ingredients.Count == 0)
            {
                return string.Empty;
            }

            return ingredients[0].unitId;
        }
    }
}
