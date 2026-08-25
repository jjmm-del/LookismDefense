using UnityEngine;
using System;
using System.Collections.Generic;

public static class UnitRecordValidator
{
    public static bool Validate(IReadOnlyList<UnitRecord> units)
    {
        if (units == null)
        {
            Debug.LogError("[UnitData] 유닛 목록이 null입니다.");
            return false;
        }

        bool isValid = true;

        Dictionary<string, int> idRows = new(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < units.Count; i++)
        {
            UnitRecord unit = units[i];

            int sheetRow = i + 2;

            if (unit == null)
            {
                LogError(sheetRow, "UnitRecord가 null입니다.");
                isValid = false;
                continue;
            }

            if (string.IsNullOrEmpty(unit.id))
            {
                LogError(sheetRow, "UnitId가 비어 있습니다.");
                isValid = false;
            }
            else if (idRows.TryGetValue(unit.id, out int previousRow))
            {
                LogError(sheetRow, $"UnitId '{unit.id}'가 중복입니다. 최초 등장 행: {previousRow}");
                isValid = false;
            }
            else
            {

                idRows.Add(unit.id, sheetRow);
            }

            if (string.IsNullOrWhiteSpace(unit.characterName))
            {
                LogError(sheetRow, $"'{unit.id}'의 이름이 비어 있습니다.");
                isValid = false;
            }

            if (!Enum.IsDefined(typeof(UnitTier), unit.tier))
            {
                LogError(sheetRow, $"'{unit.id}'의 등급이 잘못되었습니다.:{unit.tier}");
                isValid = false;
            }

            if (unit.attackRange <= 0)
            {
                LogError(sheetRow, $"'{unit.id}'의 공격 사거리는 0보다 커야합니다.");
                isValid = false;
            }

            if (unit.maxAttackTargets <= 0)
            {
                LogError(sheetRow, $"'{unit.id}'의 최대 공격 대상 수는 1 이상이어야 합니다.");
                isValid = false;
            }

            if (string.IsNullOrEmpty(unit.portraitKey))
            {
                LogWaning(sheetRow, $"'{unit.id}'의 PortraitKey가 비어 있습니다.");
            }
        }

        if (isValid)
        {
            Debug.Log($"[UnitData] 검증 성공 : {units.Count}개");
        }
        else
        {
            Debug.LogError("[UnitData] 검증 실패. GameDataBase에 적용하지 않습니다.");
        }

        return isValid;

    }

    private static void LogError(int row, string message)
    {
        Debug.LogError($"[UniData][행{row}]{message}");
    }

    private static void LogWaning(int row, string message)
    {
        Debug.LogWarning($"[UnitData][행{row}]{message}");
    }
}
