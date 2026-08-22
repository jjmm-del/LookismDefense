using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class UnitSelectorUI : MonoBehaviour
{
    [SerializeField] private Transform contentArea; //버튼들이 생길 부모 객체
    [SerializeField] private GameObject unitButtonPrefab; // 유닛 아이콘/ 버튼 프리팹
    [SerializeField] private TextMeshProUGUI titleText; // "흔함 선택권"같은 제목
    
    private Action<UnitData> onUnitSelected;
    
    //팝업 열기(외부에서 호출)
    public void OpenSelector(string title, IReadOnlyList<UnitData> unitList, Action<UnitData> selectedCallback)
    {
        gameObject.SetActive(true);
        
        titleText.text = title;
        onUnitSelected = selectedCallback;
        
        // 1. 기존 버튼 청소
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }
        
        //2. 목록에 있는 유닛만큼 버튼 생성
        foreach (UnitData unit in unitList)
        {
            GameObject buttonObject = Instantiate(unitButtonPrefab, contentArea);
            
            //버튼 택스트/이미지 설정(프리팹 구조에 따라 수정 필요)
            buttonObject.GetComponentInChildren<TextMeshProUGUI>().text = unit.EntityName;
            buttonObject.GetComponent<Image>().sprite = unit.PortraitIcon;
            
            //3. 버튼 클릭 시 "이 유닛 소환해줘"라고 매니저에게 요청
            Button button = buttonObject.GetComponent<Button>();
            UnitData capturedUnit = unit;
            button.onClick.AddListener(()=> OnUnitSelected(capturedUnit));
        }
    }

    private void OnUnitSelected(UnitData unit)
    {
        //소환 로직 호출
        onUnitSelected?.Invoke(unit);
        
        //팝업 닫기
        CloseSelector();
    }

    private void CloseSelector()
    {
        onUnitSelected = null;
        gameObject.SetActive(false);
    }
}
