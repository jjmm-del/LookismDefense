using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyInfoPanelUI : UIPanel
{
    [Header("Enemy Info")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image portraitImage;

    private EnemyEntity currentEnemy;
    public void SetData(EnemyEntity enemy)
    {
        UnsubscribeCurrentEnemy();

        if (enemy == null || enemy.Data == null)
        {
            Close();
            return;
        }

        currentEnemy = enemy;
        currentEnemy.HealthChanged += HandleHealthChanged;

        EnemyData data = currentEnemy.Data;
        if (nameText != null)
        {
            nameText.text = data.EntityName;
        }

        if (typeText != null)
        {
            typeText.text = "Enemy";
        }

        HandleHealthChanged(currentEnemy.CurrentHealth, data.MaxHealth);

        Show();
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (hpText != null)
        {
            hpText.text = $"HP:{currentHealth:F0}/{maxHealth:F0}";
        }
    }

    private void UnsubscribeCurrentEnemy()
    {
        if (currentEnemy != null)
        {
            currentEnemy.HealthChanged -= HandleHealthChanged;
        }

        currentEnemy = null;
    }

    public override void Hide()
    {
        UnsubscribeCurrentEnemy();
        base.Hide();
    }
}
