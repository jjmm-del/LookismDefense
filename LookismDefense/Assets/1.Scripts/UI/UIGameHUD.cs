using TMPro;
using UnityEngine;

public class UIGameHUD : UIBase
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text unitCountText;
    [SerializeField] private TMP_Text roundTimeText;

    private void Start()
    {
        BindEvents();
        RefreshAll();
    }

    private void BindEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
            GameManager.Instance.OnEnemyCountChanged += SetEnemyCount;
        }

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.OnRoundChanged += SetWave;
            RoundManager.Instance.OnRoundTimeChanged += SetRoundTime;
        }
    }

    private void RefreshAll()
    {
        if (GameManager.Instance != null)
        {
            SetGold(GameManager.Instance.GetCurrency(CurrencyType.Gold));
        }
    }

    private void HandleCurrencyChanged(CurrencyType type, int value)
    {
        if (type != CurrencyType.Gold)
            return;

        SetGold(value);
    }
    
    
    public void SetGold(int value)
    {
        goldText.text = $"{value}G";
    }

    public void SetWave(int current, int max)
    {
        waveText.text = $"{current}/{max}";
    }

    public void SetEnemyCount(int current, int max)
    {
        unitCountText.text = $"{current}/{max}";
    }

    public void SetRoundTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        
        roundTimeText.text  = $"{minutes:00}:{seconds:00}";
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            GameManager.Instance.OnEnemyCountChanged -= SetEnemyCount;
        }

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.OnRoundChanged -= SetWave;
            RoundManager.Instance.OnRoundTimeChanged -= SetRoundTime;
        }
    }
}
