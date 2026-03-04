using UnityEngine;
using UnityEngine.UI;
public class DifficultySelectorUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentArea;
    [SerializeField] private GameObject difficultyButtonPrefab;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateButtons();
    }

    private void GenerateButtons()
    {
        if (LobbyManager.Instance == null)
        {
            return;
        }
        DifficultyData[] presets = LobbyManager.Instance.DifficultyPresets;
        
        //기존에 있는 버튼들 청소
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }
        
        //GameManager에 등록된 난이도 개수만큼 버튼 생성
        for (int i = 0; i < presets.Length; i++)
        {
            GameObject btnObj = Instantiate(difficultyButtonPrefab, contentArea);
            DifficultyButtonUI btnUI = btnObj.GetComponent<DifficultyButtonUI>();

            if (btnUI != null)
            {
                btnUI.Setup(presets[i],i,this);
            }
        }
    }

    public void OnDifficultySelected(int index)
    {
        if (LobbyManager.Instance == null)
        {
            return;
        }
        LobbyManager.Instance.SelectDifficulty(index);
    }
}
