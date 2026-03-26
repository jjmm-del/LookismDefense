using UnityEngine;
using UnityEngine.UI;
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject healthBar; //BackgroundObject
    private Camera mainCamera; //카메라를 바라보게 하기 위함

    private void Start()
    {
        //메인 카메라 캐싱
        mainCamera = Camera.main;
    }
    
    //EnemyEntity가 체력이 변할 때마다 호출해야 함
    public void UpdateHealth(float current, float max)
    {
        fillImage.fillAmount = current / max;
        
        //체력이 100% 미만일 때만 켜지게 설정
        if (current < max)
        {
            healthBar.SetActive(true);
        }
        else
        {
            healthBar.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        // 체력바가 항상 카메라를 정면으로 바라보게 회전(빌보드 효과)
        if(mainCamera != null)
        {
                
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}
