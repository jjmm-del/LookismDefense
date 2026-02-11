using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Top Info Panel")]
    [SerializeField] private TextMeshProUGUI roundTimeText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI unitCountText;
    [SerializeField] private TextMeshProUGUI waveNameText;
    
    [Header("Bottom Unit Info Panel")]
    [SerializeField] private GameObject unitInfoPanel; // 패널 전체 (켜고 끄기용)
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private Image portraitImage; // 유닛 초상화 (나중에 추가)

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    
    //--- 상단 정보 갱신 ---
    public void UpdateRoundTime(float time)
    {
        // 시간을 00:00 형식으로 표시
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        roundTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateGold(int gold)
    {
        goldText.text = $"Gold: {gold}";
    }

    public void UpdateUnitCount(int current, int max)
    {
        unitCountText.text = $"Enemy: {current}/{max}";
        
        //위험 수치면 빨간색으로 변경하는 연출 가능
        if (current >= max - 10)
        {
            unitCountText.color = Color.red;
        }
        else
        {
            unitCountText.color = Color.white;
        }
    }

    public void UpdateWaveName(string name)
    {
        waveNameText.text = name;
    }
    
    // --- 하단 유닛 정보 갱신 ---
    public void ShowUnitInfo(UnitData data)
    {
        unitInfoPanel.SetActive(true);
        nameText.text = data.EntityName;
        damageText.text = $"DMG:{data.AttackDamage}";
        attackSpeedText.text = $"ASP:{data.AttackSpeed}";
        //portraitImage.sprite = data.Icon; //아이콘이 있다면
    }

    public void ShowEnemyInfo(EnemyData data, float currentHp)
    {
        unitInfoPanel.SetActive(true);
        nameText.text = data.EntityName;
        damageText.text = "Enemy";
        attackSpeedText.text = $"HP:{currentHp}/{data.MaxHealth}";
    }

    public void HideInfoPanel()
    {
        unitInfoPanel.SetActive(false);
    }
}
