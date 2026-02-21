using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UnitSelectorUI : MonoBehaviour
{
    [SerializeField] private Transform contentArea; //버튼들이 생길 부모 객체
    [SerializeField] private GameObject unitButtonPrefab; // 유닛 아이콘/ 버튼 프리팹
    [SerializeField] private TextMeshProUGUI titleText; // "흔함 선택권"같은 제목
    
    //팝업 열기(외부에서 호출)
    public void OpenSelector(string title, List<UnitData> unitList)
    {
        gameObject.SetActive(true);
        titleText.text = title;
        
        // 1. 기존 버튼 청소
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }
        
        //2. 목록에 있는 유닛만큼 버튼 생성
        foreach (UnitData unit in unitList)
        {
            GameObject btnObj = Instantiate(unitButtonPrefab, contentArea);
            
            //버튼 택스트/이미지 설정(프리팹 구조에 따라 수정 필요)
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = unit.EntityName;
            //btnObj.GetComponent<Image>().sprite = unit.Portrait
            
            //3. 버튼 클릭 시 "이 유닛 소환해줘"라고 매니저에게 요청
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(()=> OnUnitSelected(unit));
        }
    }

    private void OnUnitSelected(UnitData unit)
    {
        //소환 로직 호출
        UnitSpawnManager.Instance.SpawnSelectedUnit(unit);
        
        //팝업 닫기
        CloseSelector();
    }

    private void CloseSelector()
    {
        gameObject.SetActive(false);
    }
}
