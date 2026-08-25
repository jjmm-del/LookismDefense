using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetDataLoader : MonoBehaviour
{
    [Header("Google Sheet")]
    [SerializeField] private string spreadsheetId;
    //[SerializeField] private string apiKey;
    [SerializeField] private string sheetName = "유닛_DB";
    [SerializeField] private string cellRange = "A1:L";
    private const string ApiKeyEnvironmentVariable = "LOOKISM_SHEETS_API_KEY";
    
    [Header("Loading")]
    [SerializeField] private bool loadOnStart;

    private readonly List<UnitRecord> loadedUnits = new();

    public IReadOnlyList<UnitRecord> LoadedUnits =>
        loadedUnits;

    public bool IsLoaded { get; private set; }

    public event Action<IReadOnlyList<UnitRecord>>
        UnitsLoaded;

    private void Start()
    {
        if (loadOnStart)
        {
            Reload();
        }
    }

    [ContextMenu("Reload Unit Data")]
    public void Reload()
    {
        StopAllCoroutines();
        StartCoroutine(LoadUnitData());
    }

    private IEnumerator LoadUnitData()
    {
        string apiKey = Environment.GetEnvironmentVariable(ApiKeyEnvironmentVariable);
        IsLoaded = false;

        if (string.IsNullOrWhiteSpace(spreadsheetId))
        {
            Debug.LogError(
                "[GoogleSheet] Spreadsheet ID가 비어 있습니다."
            );
            yield break;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Debug.LogError(
                "[GoogleSheet] API Key가 비어 있습니다."
            );
            yield break;
        }

        string range =
            $"'{sheetName}'!{cellRange}";

        string escapedRange =
            UnityWebRequest.EscapeURL(range);

        string url =
            "https://sheets.googleapis.com/v4/" +
            $"spreadsheets/{spreadsheetId}/" +
            $"values/{escapedRange}" +
            "?majorDimension=ROWS" +
            "&valueRenderOption=UNFORMATTED_VALUE";

        using UnityWebRequest request =
            UnityWebRequest.Get(url);

        request.SetRequestHeader("x-goog-api-key", apiKey);

        request.timeout = 15;

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "[GoogleSheet] 다운로드 실패\n" +
                $"HTTP: {request.responseCode}\n" +
                $"Error: {request.error}\n" +
                $"Response: {request.downloadHandler.text}"
            );

            yield break;
        }

        if (!TryParseUnitData(request.downloadHandler.text, out List<UnitRecord> parsedUnits))
        {
            Debug.LogError("[GoogleSheet] 유닛 데이터 파싱 실패");

            yield break;
        }

        if (!UnitRecordValidator.Validate(parsedUnits))
        {
            Debug.LogError("[GoogleSheet] 검증 실패. GameDatabase에 적용하지 않습니다.");

            yield break;
        }

        if (GameDatabase.Instance == null)
        {
            Debug.LogError("[GoogleSheet] GameDatabase가 Scene에 없습니다.");
            yield break;
        }

        GameDatabase.Instance.SetUnits(parsedUnits);
        
        // 파싱과 검증이 모두 성공한 경우에만 교체
        loadedUnits.Clear();
        loadedUnits.AddRange(parsedUnits);

        IsLoaded = true;

        Debug.Log($"[GoogleSheet] 유닛 {loadedUnits.Count}개 검증 및 DB등록 완료");

        PrintSamples(loadedUnits);

        UnitsLoaded?.Invoke(loadedUnits);

        // if (GameDatabase.Instance.TryGetUnit("C001", out UnitRecord unit))
        // {
        //     Debug.Log($"DB조회 성공 : {unit.DisplayName}");
        // }
    }

    private bool TryParseUnitData(string json, out List<UnitRecord> result)
    {
        result = new List<UnitRecord>();

        JObject root;

        try
        {
            root = JObject.Parse(json);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[GoogleSheet] JSON 파싱 오류: " +
                $"{exception.Message}"
            );

            return false;
        }

        if (root["values"] is not JArray rows ||
            rows.Count < 2)
        {
            Debug.LogError(
                "[GoogleSheet] 헤더 또는 데이터 행이 없습니다."
            );

            return false;
        }

        if (rows[0] is not JArray headerRow)
        {
            Debug.LogError(
                "[GoogleSheet] 헤더 형식이 잘못되었습니다."
            );

            return false;
        }

        Dictionary<string, int> headers = BuildHeaderMap(headerRow);
        Debug.Log("[GoogleSheet]인식한 헤더: "+string.Join(", ",headers.Keys));
        Debug.Log($"[GoogleSheet]받은 행 개수 : {rows.Count}");

        if (!ValidateRequiredHeaders(headers))
        {
            return false;
        }

        bool parseSucceeded = true;

        for (int i = 1; i < rows.Count; i++)
        {
            if (rows[i] is not JArray row)
                continue;

            int sheetRow = i + 1;

            if (IsEmptyRow(row))
                continue;

            if (!IsEnabledRow(row, headers, sheetRow))
            {
                Debug.Log($"사용가능 행이 아니라 스킵:{sheetRow}");
                continue;
            }

            if (!TryCreateUnitRecord(
                    row,
                    headers,
                    sheetRow,
                    out UnitRecord unit))
            {
                parseSucceeded = false;
                continue;
            }

            // 사용하지 않는 행은 DB에서 제외
            if (!unit.enabled)
                continue;

            result.Add(unit);
        }

        if (result.Count == 0)
        {
            Debug.LogError(
                "[GoogleSheet] 사용할 수 있는 유닛이 없습니다."
            );

            return false;
        }

        return parseSucceeded;
    }

    private bool TryCreateUnitRecord(
        JArray row,
        Dictionary<string, int> headers,
        int sheetRow,
        out UnitRecord unit)
    {
        unit = null;

        string id =
            GetCell(row, headers, "UnitID");

        string tierText =
            GetCell(row, headers, "TierCode", "등급");

        string characterName =
            GetCell(row, headers, "이름", "캐릭터명");

        string title =
            GetCell(row, headers, "칭호");

        if (string.IsNullOrWhiteSpace(id))
        {
            LogRowError(sheetRow, "UnitID가 비어 있습니다.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(characterName))
        {
            LogRowError(sheetRow, "이름이 비어 있습니다.");
            return false;
        }

        if (!UnitTierParser.TryParse(
                tierText,
                out UnitTier tier))
        {
            LogRowError(
                sheetRow,
                $"알 수 없는 등급: '{tierText}'"
            );

            return false;
        }

        if (!TryGetFloat(
                row,
                headers,
                sheetRow,
                out float attackDamage,
                "공격력",
                "AttackDamage"))
        {
            return false;
        }

        if (!TryGetFloat(
                row,
                headers,
                sheetRow,
                out float attackSpeed,
                "공격속도",
                "AttackSpeed"))
        {
            return false;
        }

        if (!TryGetInt(
                row,
                headers,
                sheetRow,
                out int attackRange,
                "사거리",
                "AttackRange"))
        {
            return false;
        }

        if (!TryGetInt(
                row,
                headers,
                sheetRow,
                out int maxTargets,
                "최대타겟",
                "MaxAttackTargets"))
        {
            return false;
        }

        bool enabled = GetBool(
            row,
            headers,
            true,
            "사용",
            "Enabled"
        );

        unit = new UnitRecord
        {
            id = id.Trim(),
            tier = tier,

            characterName = characterName.Trim(),
            title = title.Trim(),

            attackDamage = attackDamage,
            attackSpeed = attackSpeed,
            attackRange = attackRange,
            maxAttackTargets = maxTargets,

            prefabKey = GetCell(
                row,
                headers,
                "PrefabKey"
            ).Trim(),

            portraitKey = GetCell(
                row,
                headers,
                "PortraitKey"
            ).Trim(),

            enabled = enabled
        };

        return true;
    }

    private static Dictionary<string, int> BuildHeaderMap(JArray headerRow)
    {
        Dictionary<string, int> result =
            new(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < headerRow.Count; i++)
        {
            string header =
                headerRow[i]?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(header))
                continue;

            if (!result.ContainsKey(header))
            {
                result.Add(header, i);
            }
        }

        return result;
    }

    private static bool ValidateRequiredHeaders(Dictionary<string, int> headers)
    {
        bool valid = true;

        valid &= RequireAnyHeaders(headers, "UnitID");
        valid &= RequireAnyHeaders(headers, "TierCode","등급");
        valid &= RequireAnyHeaders(headers, "이름","캐릭터명");
        valid &= RequireAnyHeaders(headers, "공격력","AttackDamage");
        valid &= RequireAnyHeaders(headers, "공격속도","AttackSpeed");
        valid &= RequireAnyHeaders(headers, "사거리","AttackRange");
        valid &= RequireAnyHeaders(headers, "최대타겟","MaxAttackTargets");
        valid &= RequireAnyHeaders(headers, "사용","Enabled");
        
        return valid;
        
    }

    private static bool RequireAnyHeaders(Dictionary<string, int> headers, params string[] names)
    {
        foreach (string name in names)
        {
            if (headers.ContainsKey(name))
            {
                return true;
            }
        }
        
        Debug.LogError("[GoogleSheet]필수 열 누락:"+ string.Join("또는",names));

        
        return false;
    }

    private static string GetCell(
        JArray row,
        Dictionary<string, int> headers,
        params string[] headerNames)
    {
        foreach (string headerName in headerNames)
        {
            if (!headers.TryGetValue(
                    headerName,
                    out int index))
            {
                continue;
            }

            if (index < 0 || index >= row.Count)
            {
                return string.Empty;
            }

            return row[index]?.ToString() ??
                   string.Empty;
        }

        return string.Empty;
    }

    private static bool TryGetFloat(
        JArray row,
        Dictionary<string, int> headers,
        int sheetRow,
        out float result,
        params string[] headerNames)
    {
        string value =
            GetCell(row, headers, headerNames);

        if (float.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out result))
        {
            return true;
        }

        LogRowError(
            sheetRow,
            $"{headerNames[0]} 숫자 변환 실패: '{value}'"
        );

        return false;
    }

    private static bool TryGetInt(
        JArray row,
        Dictionary<string, int> headers,
        int sheetRow,
        out int result,
        params string[] headerNames)
    {
        string value =
            GetCell(row, headers, headerNames);

        if (int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out result))
        {
            return true;
        }

        LogRowError(
            sheetRow,
            $"{headerNames[0]} 정수 변환 실패: '{value}'"
        );

        return false;
    }

    private static bool GetBool(
        JArray row,
        Dictionary<string, int> headers,
        bool defaultValue,
        params string[] headerNames)
    {
        string value =
            GetCell(row, headers, headerNames)
                .Trim();

        if (string.IsNullOrEmpty(value))
            return defaultValue;

        if (bool.TryParse(value, out bool boolValue))
        {
            return boolValue;
        }

        return value switch
        {
            "1" => true,
            "Y" => true,
            "YES" => true,
            "예" => true,
            "사용" => true,

            "0" => false,
            "N" => false,
            "NO" => false,
            "아니오" => false,
            "미사용" => false,

            _ => defaultValue
        };
    }

    private static bool IsEmptyRow(JArray row)
    {
        foreach (JToken cell in row)
        {
            if (!string.IsNullOrWhiteSpace(
                    cell?.ToString()))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsEnabledRow(JArray row, Dictionary<string, int> headerMap, int sheetRowNumber)
    {
        string rawValue = GetCell(row, headerMap, "사용");

        if (string.IsNullOrEmpty(rawValue))
        {
            return false;
        }

        if (bool.TryParse(rawValue.Trim(), out bool isEnabled))
        {
            return isEnabled;
        }
        Debug.LogWarning($"[GoogleSheet]{sheetRowNumber}행의 사용 값이 잘못되었습니다: '{rawValue}'(True/False만 사용)");
        
        return false;
    }
    private static void LogRowError(
        int sheetRow,
        string message)
    {
        Debug.LogError(
            $"[GoogleSheet][행 {sheetRow}] {message}"
        );
    }

    private static void PrintSamples(
        IReadOnlyList<UnitRecord> units)
    {
        int sampleCount = Mathf.Min(3, units.Count);

        for (int i = 0; i < sampleCount; i++)
        {
            UnitRecord unit = units[i];

            Debug.Log(
                $"[UnitSample] {unit.id} / " +
                $"{unit.DisplayName} / " +
                $"{unit.tier} / " +
                $"DMG {unit.attackDamage}"
            );
        }
    }
}