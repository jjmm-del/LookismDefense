using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UpgradeButtonUI : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private UnitTier targetTier; //인스펙터에서 설정 (예; Common)
    [SerializeField] private string displayName = "흔함 강화"; 
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private Button btn;

    private void Start()
    {
        btn.onClick.AddListener(OnClick);
        UpdateUI();
    }
    
    //버튼이 켜질 때마다 UI 갱신
    private void OnEnable()
    {
        UpdateUI();
    }

    private void OnClick()
    {
        UpgradeManager.Instance.TryUpgradeTier(targetTier);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (UpgradeManager.Instance == null) return;
        
        //매니저에서 해당 티어의 데이터 가져오기
        var data = UpgradeManager.Instance.GetUpgradeData(targetTier);

        if (data != null)
        {
            int currentCost = data.baseCost + (data.currentLevel * data.costPerLevel);
            float currentBonus = data.currentLevel * data.damageBonusPerLevel * 100f;

            titleText.text = displayName;
            levelText.text = $"Lv.{data.currentLevel}";
            costText.text = $"{currentCost}G";
            statText.text = $"{+currentBonus:F0}%";
            
            //골드 부족 시 버튼 비활성화 등 처리 가능
            if (GameManager.Instance.GetCurrency(CurrencyType.Gold) < currentCost)
            {
                btn.interactable = false;
            }
        }
        else
        {
            //데이터 없으면 비활성화
            btn.interactable = false;
            titleText.text = "준비중";
        }
    }
}
