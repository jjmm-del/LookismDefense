using System.Collections.Generic;

public static class UnitTierParser
{
    private static readonly Dictionary<string, UnitTier>
        TierMap = new()
        {
            { "흔함", UnitTier.Common },
            { "안흔함", UnitTier.Uncommon },
            { "특별함", UnitTier.Special },
            { "희귀함", UnitTier.Rare },
            { "전설적인", UnitTier.Legendary },
            { "히든조합", UnitTier.Hidden },
            { "변화됨", UnitTier.Changed },
            { "초월함", UnitTier.Transcendence },
            { "불멸의", UnitTier.Immortal },
            { "영원함", UnitTier.Eternal },
            { "제한됨", UnitTier.Exclusive }
        };

    public static bool TryParse(
        string value,
        out UnitTier tier)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            tier = default;
            return false;
        }

        value = value.Trim();

        if (TierMap.TryGetValue(value, out tier))
        {
            return true;
        }

        return System.Enum.TryParse(
            value,
            true,
            out tier
        );
    }
}