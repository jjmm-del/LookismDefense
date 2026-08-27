using System;
using System.Collections.Generic;
using UnityEngine;

public static class CombinationRecordValidator
{
    public static bool Validate(
        IReadOnlyList<CombinationRecord> recipes,
        GameDatabase database)
    {
        if (recipes == null)
        {
            Debug.LogError(
                "[RecipeData] 조합 목록이 null입니다."
            );

            return false;
        }

        if (database == null || !database.IsReady)
        {
            Debug.LogError(
                "[RecipeData] 유닛 DB가 준비되지 않았습니다."
            );

            return false;
        }

        bool isValid = true;
        int enabledRecipeCount = 0;

        HashSet<string> recipeIds =
            new(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < recipes.Count; i++)
        {
            CombinationRecord recipe = recipes[i];

            if (recipe == null)
            {
                Debug.LogError(
                    $"[RecipeData][목록 {i}] " +
                    "CombinationRecord가 null입니다."
                );

                isValid = false;
                continue;
            }

            if (!recipe.enabled)
                continue;

            enabledRecipeCount++;

            string recipeLabel =
                string.IsNullOrWhiteSpace(recipe.id)
                    ? $"목록 {i}"
                    : recipe.id;

            if (string.IsNullOrWhiteSpace(recipe.id))
            {
                LogError(
                    recipeLabel,
                    "RecipeID가 비어 있습니다."
                );

                isValid = false;
            }
            else if (!recipeIds.Add(recipe.id))
            {
                LogError(
                    recipeLabel,
                    "중복된 RecipeID입니다."
                );

                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(
                    recipe.resultUnitId))
            {
                LogError(
                    recipeLabel,
                    "ResultUnitID가 비어 있습니다."
                );

                isValid = false;
            }
            else if (!database.TryGetUnit(
                         recipe.resultUnitId,
                         out _))
            {
                LogError(
                    recipeLabel,
                    $"결과 유닛 '{recipe.resultUnitId}'이 " +
                    "유닛 DB에 없습니다."
                );

                isValid = false;
            }

            if (recipe.ingredients == null ||
                recipe.ingredients.Count == 0)
            {
                LogError(
                    recipeLabel,
                    "재료가 하나도 없습니다."
                );

                isValid = false;
                continue;
            }

            HashSet<int> materialOrders = new();
            HashSet<string> materialUnitIds =
                new(StringComparer.OrdinalIgnoreCase);

            int mainIngredientCount = 0;

            foreach (RecipeIngredientRecord ingredient
                     in recipe.ingredients)
            {
                if (ingredient == null)
                {
                    LogError(
                        recipeLabel,
                        "null 재료가 포함되어 있습니다."
                    );

                    isValid = false;
                    continue;
                }

                if (ingredient.order <= 0)
                {
                    LogError(
                        recipeLabel,
                        $"재료 순서는 1 이상이어야 합니다: " +
                        $"{ingredient.order}"
                    );

                    isValid = false;
                }
                else
                {
                    if (!materialOrders.Add(
                            ingredient.order))
                    {
                        LogError(
                            recipeLabel,
                            $"재료 순서 {ingredient.order}가 " +
                            "중복되었습니다."
                        );

                        isValid = false;
                    }

                    if (ingredient.order == 1)
                    {
                        mainIngredientCount++;
                    }
                }

                if (string.IsNullOrWhiteSpace(
                        ingredient.unitId))
                {
                    LogError(
                        recipeLabel,
                        "재료 UnitID가 비어 있습니다."
                    );

                    isValid = false;
                }
                else
                {
                    if (!materialUnitIds.Add(
                            ingredient.unitId))
                    {
                        LogError(
                            recipeLabel,
                            $"재료 '{ingredient.unitId}'가 " +
                            "파싱 후에도 중복돼 있습니다."
                        );

                        isValid = false;
                    }

                    if (!database.TryGetUnit(
                            ingredient.unitId,
                            out _))
                    {
                        LogError(
                            recipeLabel,
                            $"재료 유닛 '{ingredient.unitId}'이 " +
                            "유닛 DB에 없습니다."
                        );

                        isValid = false;
                    }
                }

                if (ingredient.count <= 0)
                {
                    LogError(
                        recipeLabel,
                        $"재료 '{ingredient.unitId}'의 수량은 " +
                        "1 이상이어야 합니다."
                    );

                    isValid = false;
                }
            }

            if (mainIngredientCount != 1)
            {
                LogError(
                    recipeLabel,
                    "MaterialOrder=1인 메인 재료가 " +
                    $"정확히 하나여야 합니다. 현재: " +
                    $"{mainIngredientCount}개"
                );

                isValid = false;
            }
        }

        if (enabledRecipeCount == 0)
        {
            Debug.LogError(
                "[RecipeData] 활성화된 조합법이 없습니다."
            );

            isValid = false;
        }

        if (isValid)
        {
            Debug.Log(
                $"[RecipeData] 검증 성공: " +
                $"{enabledRecipeCount}개"
            );
        }
        else
        {
            Debug.LogError(
                "[RecipeData] 검증 실패. " +
                "GameDatabase에 적용하지 않습니다."
            );
        }

        return isValid;
    }

    private static void LogError(
        string recipeId,
        string message)
    {
        Debug.LogError(
            $"[RecipeData][{recipeId}] {message}"
        );
    }
}