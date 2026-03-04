using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    //싱글턴화
    public static LobbyManager Instance { get; private set; }
    
    [Header("Room Settings")]
    [SerializeField] private DifficultyData[] difficultyPresets; //로비에서 보여줄 난이도
    public DifficultyData[] DifficultyPresets => difficultyPresets;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void SelectDifficulty(int index)
    {
        SessionManager.SelectedDifficultyIndex = index;
        Debug.Log($"[{difficultyPresets[index].name}] 난이도 선택됨. (대기열에 저장)");
    }
    // 방장이 [게임 시작] 버튼을 눌렀을 때 호출될 함수
    public void StartGame()
    {
        if (SessionManager.SelectedDifficultyIndex == -1)
        {
            return;
        }
        Debug.Log("게임을 시작합니다! 메인 씬으로 이동...");
        
        // 2. 메인 게임 씬을 불러옵니다. (빌드에 추가된 씬 이름)
        SceneManager.LoadScene("MainScene");
    }
}
