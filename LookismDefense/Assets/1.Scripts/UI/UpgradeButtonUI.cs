using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UpgradeButtonUI : MonoBehaviour
{
    private UnitTier targetTier; //[수정] Inspector -> Setup으로 설정
    private string displayName;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private Button btn;

    private void Start()
    {
        btn.onClick.AddListener(OnClick);
        // 실시간 구독
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnResourceChanged += UpdateUI;
        }
    }
    private void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnResourceChanged -= UpdateUI;
        }
    }
    //[신규] 프리팹이 생성될 때 최초 1회 호출되는 세팅 함수
    public void Setup(TierUpgradeData data)
    {
        targetTier = data.targetTier;
        displayName = data.name;

        UpdateUI();
    }

    private void OnClick()
    {
        UpgradeManager.Instance.TryUpgradeTier(targetTier);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (UpgradeManager.Instance == null || GameManager.Instance == null) return;
        
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
            if (GameManager.Instance.GetCurrency(CurrencyType.Gold) >= currentCost)
            {
                btn.interactable = true;
                costText.color = Color.white;
            }
            else
            {
                btn.interactable = false;
                costText.color = Color.red;
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
