using UnityEngine;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float panBorderThickness = 10f;
    [SerializeField] private bool useEdgePanning = true;
    
    [Header("Limit Bounds(맵 한계선")]
    [SerializeField] private Vector2 panLimitMin = new Vector2(-20, -20);
    [SerializeField] private Vector2 panLimitMax = new Vector2(20, 20);
    
    [Header("Zoom Settings")]
    [SerializeField] private float scrollSpeed = 20f;
    [SerializeField] private float minY = 10f;
    [SerializeField] private float maxY = 40f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;

        // 키보드와 마우스 장치 가져오기
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        // 장치가 연결되어 있지 않으면 중지
        if (keyboard == null || mouse == null) return;

        Vector2 mousePos = mouse.position.ReadValue();

        // 1. 방향키 / WASD / 마우스 화면 끝 밀기 이동
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed || (useEdgePanning && mousePos.y >= Screen.height - panBorderThickness))
        {
            pos.z += panSpeed * Time.deltaTime;
        }
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed || (useEdgePanning && mousePos.y <= panBorderThickness))
        {
            pos.z -= panSpeed * Time.deltaTime;
        }
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed || (useEdgePanning && mousePos.x >= Screen.width - panBorderThickness))
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed || (useEdgePanning && mousePos.x <= panBorderThickness))
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        // 2. 마우스 휠 줌 (Zoom)
        float scroll = mouse.scroll.ReadValue().y;
        pos.y -= scroll * scrollSpeed * Time.deltaTime;

        // 3. 한계선 적용 (Clamp)
        pos.x = Mathf.Clamp(pos.x, panLimitMin.x, panLimitMax.x);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, panLimitMin.y, panLimitMax.y);

        transform.position = pos;
    }
    
}
