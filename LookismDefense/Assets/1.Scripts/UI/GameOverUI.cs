using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        // 다시하기 버튼(현재 씬을 다시 로드)
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f; // 멈췄던 시간 다시 되돌리기
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }
        // 나가기 버튼( 게임 종료) -> 나중에는 로비로 나가기
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(() =>
            {
                Application.Quit();
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            });
        }
        
    }
}
