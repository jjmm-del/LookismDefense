using UnityEngine;
using UnityEngine.UI;
public class DifficultySelectorUI : MonoBehaviour
{
    [Header("Difficulty Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button crewHeadButton;
    [SerializeField] private Button gen1KingButton;
    [SerializeField] private Button gunParkButton;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //버튼을 누르면 해당 인덱스와 난이도로 게임을 시작하도록 연결합니다.
        //(GameManager 에 등록해둔 difficultyPresets 배열의 순서와 맞춥니다.
        if(easyButton!=null) easyButton.onClick.AddListener(()=>SelectDifficulty(0));
    }

    private void SelectDifficulty(int index)
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        // 1. 게임 매니저에게 해당 난이도로 게임을 시작하라고 명령
        GameManager.Instance.StartGame(index);
        
        //2. 난이도 선택 창 비활성화
        gameObject.SetActive(false);
    }
}
