using TMPro;
using UnityEngine;

public class UIGameHUD : UIBase
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text unitCountText;

    public void SetGold(int value)
    {
        goldText.text = value.ToString();
    }

    public void SetWave(int current, int max)
    {
        waveText.text = $"{current}/{max}";
    }

    public void SetUnitCount(int current, int max)
    {
        unitCountText.text = $"{current}/{max}";
    }
}
