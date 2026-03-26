using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MultiUnitPortraitUI : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI countText; //겹친 유닛 수
    [SerializeField] private Button portraitButton;

    private UnitEntity targetUnit;

    // [신규] 버튼이 눌렸을 때 실행할 함수를 저장할 변수
    private Action<UnitEntity> onPortraitClickedCallback;

    private void Start()
    {
        if (portraitButton != null)
        {
            portraitButton.onClick.AddListener(OnClickPortrait);
        }
    }
    public void Setup(UnitData data, int count, UnitEntity target, Action<UnitEntity> onClickCallback)
    {
        this.targetUnit = target;
        this.onPortraitClickedCallback = onClickCallback;
        if (data.PortraitIcon != null)
        {
            portraitImage.sprite = data.PortraitIcon;
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }

        //1개일 때는 숫자 숨기고, 2개 이상일 때만 표시
        if (count > 1)
        {
            countText.text = count.ToString();
            countText.gameObject.SetActive(true);
        }
        else
        {
            countText.gameObject.SetActive(false);
        }
    }

    private void OnClickPortrait()
    {
        // 싱글톤을 찾을 필요 없이, 그냥 넘겨받은 함수를 실행( null이 아니면 Invoke)
        if (targetUnit != null)
        {
            onPortraitClickedCallback?.Invoke(targetUnit);
        }
    }
}
