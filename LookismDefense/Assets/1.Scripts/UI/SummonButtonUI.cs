using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SummonButtonUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CurrencyType targetCurrency; // 이 버튼이 사용하는 재화
    [SerializeField] private string displayName = "랜덤 소환";
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button actionButton;
    private void Start()
    {
        //버튼 클릭 시 실행할 함수 연결
        actionButton.onClick.AddListener(OnClick);
        
        //초기 UI 갱신
        UpdateUI();
        
        //(선택) UI매니저나 게임 매니저 이벤트에 UpdateUI 등록하면 좋음
        
    }

    private void Update()
    {
        //매 프레임 검사하지 않고 이벤트 방식으로 하는게 좋지만,
        //편의상 여기서 갱신하거나, 패널이 열릴 때(OnEnable) 갱신하면 됩니다.
        UpdateUI(); //임시
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        //1. 현재 재화 개수 가져오기
        int amount = GameManager.Instance.GetCurrency(targetCurrency);
        
        //2. 텍스트 갱신
        titleText.text = displayName;
        countText.text = $"{amount}";
        
        //3. 개수가 0이면 버튼 비활성화(클릭 못하게)
        if (amount > 0)
        {
            actionButton.interactable = true;
            countText.color = Color.black;
        }
        else
        {
            actionButton.interactable = false;
            countText.color = Color.red;
        }
    }

    private void OnClick()
    {
        //UnitSpawnManager에게 요청
        //어떤 재화냐에 따라 다른 소환 로직 호출
        UnitSpawnManager.Instance.TrySummon(targetCurrency);
        
        //사용 후 UI 갱신
        UpdateUI();
    }
}
