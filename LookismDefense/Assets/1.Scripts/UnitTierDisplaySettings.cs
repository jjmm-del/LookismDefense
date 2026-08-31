using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitTierDisplaySettings", menuName = "Game/UI/Unit Tier Display Settings")]
public class UnitTierDisplaySettings : ScriptableObject
{
    [SerializeField] 
    private List<TierDisplayInfo> tierSettings = new()
    {
        new TierDisplayInfo
        {
            tier = UnitTier.Common,
            displayName = "흔함",
            textColor = Color.white 
        },
        new TierDisplayInfo 
        {
            tier = UnitTier.Uncommon,
            displayName = "안흔함",
            textColor = Color.green
        },
        new TierDisplayInfo 
        { tier = UnitTier.Special,
            displayName = "특별함",
            textColor = Color.dodgerBlue
        }, 
        new TierDisplayInfo 
        { tier = UnitTier.Rare,
            displayName = "희귀함",
            textColor = Color.purple
        },
        new TierDisplayInfo 
        { tier = UnitTier.Legendary,
            displayName = "전설적인",
            textColor = Color.orange
        }, 
        new TierDisplayInfo 
        { tier = UnitTier.Hidden,
            displayName = "히든조합",
            textColor = Color.crimson
        }, 
        new TierDisplayInfo 
        { tier = UnitTier.Changed,
            displayName = "변화된",
            textColor = Color.deepPink
        },
        new TierDisplayInfo 
        { tier = UnitTier.Transcendence,
            displayName = "초월함",
            textColor = Color.aquamarine
        },
        new TierDisplayInfo 
        { tier = UnitTier.Immortal,
            displayName = "불멸의",
            textColor = Color.firebrick
        }, 
        new TierDisplayInfo 
        { tier = UnitTier.Eternal,
            displayName = "영원함",
            textColor = Color.mediumPurple
        },
        new TierDisplayInfo 
        { tier = UnitTier.Exclusive,
            displayName = "제한됨",
            textColor = Color.darkGoldenRod
        }
    };

    private Dictionary<UnitTier, TierDisplayInfo> tierMap;

    private void OnEnable()
    {
        BuildMap();
    }

    private void BuildMap()
    {
        tierMap = new Dictionary<UnitTier, TierDisplayInfo>();

        foreach (TierDisplayInfo setting in tierSettings)
        {
            if (setting == null)
                continue;

            if (!tierMap.ContainsKey(setting.tier))
            {
                tierMap.Add(setting.tier, setting);
            }
        }
    }

    public bool TryGet(UnitTier tier, out TierDisplayInfo info)
    {
        if (tierMap == null)
        {
            BuildMap();
        }

        return tierMap.TryGetValue(tier, out info);
    }

    public string GetDisplayName(UnitTier tier)
    {
        return TryGet(tier, out TierDisplayInfo info)
            ? info.displayName
            : tier.ToString();
    }

    public Color GetColor(UnitTier tier)
    {
        return TryGet(tier, out TierDisplayInfo info)
            ? info.textColor
            : Color.white;
    }
}
