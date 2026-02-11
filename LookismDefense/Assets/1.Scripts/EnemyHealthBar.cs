using UnityEngine;
using UnityEngine.UI;
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Transform targetCamera; //카메라를 바라보게 하기 위함

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main.transform;
        }
    }
    
    //EnemyEntity가 체력이 변할 때마다 호출해야 함
    public void UpdateHealth(float current, float max)
    {
        healthSlider.value = current / max;
    }

    private void LateUpdate()
    {
        // 체력바가 항상 카메라를 정면으로 바라보게 회전(빌보드 효과)
        transform.LookAt(transform.position + targetCamera.forward);
    }
}
