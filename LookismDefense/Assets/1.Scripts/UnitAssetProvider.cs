using UnityEngine;
using System;
using System.Collections.Generic;
public static class UnitAssetProvider
{
    private static readonly Dictionary<string, GameObject> PrefabCache = new(StringComparer.Ordinal);
    
    private static readonly Dictionary<string, Sprite> PortraitCache = new(StringComparer.Ordinal);

    public static bool TryLoadPrefab(UnitRecord unit, out GameObject prefab)
    {
        prefab = null;
        if(unit == null)
            return false;

        string key = NormalizeKey(unit.prefabKey);

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError($"[{unit.id}] PrefabKey가 비어 있습니다.");
            return false;
        }

        if (PrefabCache.TryGetValue(key, out prefab))
        {
            return prefab != null;
        }

        prefab = Resources.Load<GameObject>(key);
        if (prefab == null)
        {
            Debug.LogError($"[{unit.id}]프리팹을 찾을 수 없습니다: Resources/{key}");
            return false;
        }

        PrefabCache.Add(key, prefab);
        return true;
    }

    public static Sprite LoadPortrait(UnitRecord unit)
    {
        if (unit == null)
            return null;
        string key = NormalizeKey(unit.portraitKey);
        if (string.IsNullOrEmpty(key))
            return null;

        if (PortraitCache.TryGetValue(key, out Sprite cachedSprite))
        {
            return cachedSprite;
        }
        Sprite sprite = Resources.Load<Sprite>(key);
        if (sprite == null)
        {
            Debug.LogError($"[{unit.id}] 초상화를 찾을 수 없습니다: Resources/{key}");
            return null;
        }

        PortraitCache.Add(key, sprite);
        return sprite;
    }

    private static string NormalizeKey(string key)
    {
        return string.IsNullOrWhiteSpace(key) ? string.Empty : key.Trim().Replace("\\", "/").TrimStart('/');
    }
}
