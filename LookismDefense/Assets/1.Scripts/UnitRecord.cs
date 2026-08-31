using UnityEngine;
using System;

[Serializable]
public class UnitRecord
{
    public string id;
    public UnitTier tier;

    public string characterName;
    public string title;

    public float attackDamage;
    public float attackSpeed;
    public int attackRange;
    public int maxAttackTargets;

    public string prefabKey;
    public string portraitKey;

    public bool enabled;

    public string DisplayName => string.IsNullOrEmpty(title) ? characterName : $"[{title}]{characterName}";
}
