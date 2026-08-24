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

    private void Start()
    {
        Hide();
    }

    public void ShowInfo(EnemyData data, float currentHp)
    {
        if (data == null)
        {
            HideInfo();
            return;
        }

        if (nameText != null)
        {
            nameText.text = data.EntityName;
        }

        if (typeText != null)
        {
            typeText.text = "Enemy";
        }

        if (hpText != null)
        {
            hpText.text = $"HP:{currentHp}/{data.MaxHealth:F0}";
        }

        Show();
    }

    public void HideInfo()
    {
        Hide();
    }

}
