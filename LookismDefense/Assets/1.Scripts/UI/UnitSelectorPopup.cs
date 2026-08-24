using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class UnitSelectorPopup : UIPopup
{
    [SerializeField] private TextMeshProUGUI titleText; // "흔함 선택권"같은 제목
    [SerializeField] private Transform contentArea; //버튼들이 생길 부모 객체
    [SerializeField] private GameObject unitButtonPrefab; // 유닛 아이콘/ 버튼 프리팹
    
    private Action<UnitData> onUnitSelected;
    
    
    //팝업 열기(외부에서 호출)
    public void SetData(string title, IReadOnlyList<UnitData> unitList, Action<UnitData> callback)
    {
        ClearButtons();

        if (titleText != null)
        {
            titleText.text = title;
        }
        onUnitSelected = callback;
        
        if (unitList == null)
            return;
        
        //2. 목록에 있는 유닛만큼 버튼 생성
        foreach (UnitData unit in unitList)
        {
            if (unit == null)
                return; 
            CreateUnitButton(unit);
        }
    }
    
    
 

    private void CreateUnitButton(UnitData unit)
    {
        GameObject buttonObject = Instantiate(unitButtonPrefab, contentArea);
        
        TextMeshProUGUI unitNameText = buttonObject.GetComponent<TextMeshProUGUI>();
        if (unitNameText != null)
        {
            unitNameText.text = unit.EntityName;
        }
            
        //버튼 택스트/이미지 설정(프리팹 구조에 따라 수정 필요)
        Image portraitImage = buttonObject.GetComponent<Image>();
        if (portraitImage != null)
        {
            portraitImage.sprite = unit.PortraitIcon;
        }
            
        //3. 버튼 클릭 시 "이 유닛 소환해줘"라고 매니저에게 요청
        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(()=> HandleUnitSelected(unit));
        }
    }

    private void HandleUnitSelected(UnitData unit)
    {
        Action<UnitData> callback = onUnitSelected;
        Close();
        callback?.Invoke(unit);
    }
    

    public override void Hide()
    {
        onUnitSelected = null;
        ClearButtons();
        base.Hide();
    }
    

    private void ClearButtons()
    {
        if (contentArea == null)
            return;
        
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }
    }
}
