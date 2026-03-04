using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DifficultyButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;

    private int myDifficultyIndex; //내가 몇 번째 난이도인지 기억할 변수
    private DifficultySelectorUI parentUI; //나를 생성해준 부모 패널
    
    //생성 될 때 데이터를 주입받는 세팅 함수
    public void Setup(DifficultyData data, int index, DifficultySelectorUI parent)
    {
        myDifficultyIndex = index;
        parentUI = parent;

        //이름 텍스트
        nameText.text = data.name;
        
        //버튼에 기능 주입
        button.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        parentUI.OnDifficultySelected(myDifficultyIndex);
    }
}
