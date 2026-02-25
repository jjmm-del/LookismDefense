using UnityEngine;
using System.Collections.Generic;

public class UpgradePanelUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform contentArea; // 버튼들이 생성될 부모 
    [SerializeField] private GameObject upgradeButtonPrefab; //

    private void Start()
    {
        GenerateUpgradeButtons();
    }

    private void GenerateUpgradeButtons()
    {
        // 1. 매니저에서 업그레이드 목록 가져오기
        if (UpgradeManager.Instance == null)
        {
            return;
        }
        List<TierUpgradeData> upgrades = UpgradeManager.Instance.GetAllUpgradeData();
        
        // 2. 혹시 기존에 만들어진 버튼이 있다면 초기화(청소)
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }
        
        // 3. 목록 개수만큼 프리팹 생성 및 세팅
        foreach (TierUpgradeData upgradeData in upgrades)
        {
            // 프리팹 생성
            GameObject btnObj = Instantiate(upgradeButtonPrefab, contentArea);
            
            //프리팹에 붙어있는 스크립트 가져와서 데이터 넣어주기
            UpgradeButtonUI btnUI = btnObj.GetComponent<UpgradeButtonUI>();
            if (btnUI != null)
            {
                btnUI.Setup(upgradeData);
            }
        }
    }
}
