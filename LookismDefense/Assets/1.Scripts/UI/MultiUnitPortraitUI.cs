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
    public void Setup(UnitEntity target, int count, Action<UnitEntity> onClickCallback)
    {
        targetUnit = target;
        onPortraitClickedCallback = onClickCallback;
        Sprite portrait = null;
        
        if (target != null && target.RuntimeData != null)
        {
            portrait = UnitAssetProvider.LoadPortrait(target.RuntimeData);
        }

        if (portrait == null && target != null && target.Data != null)
        {
            portrait = target.Data.PortraitIcon;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.gameObject.SetActive(portrait != null);
        }

        if (countText != null)
        {
            bool showCount = count > 1;
            countText.gameObject.SetActive(showCount);
            if (showCount)
            {
                countText.text = count.ToString();
            }
            
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
