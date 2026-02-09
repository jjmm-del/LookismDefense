using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System.Collections.Generic;
public class RTSController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private LayerMask groundLayer;
    
    private List<UnitEntity> selectedUnits = new List<UnitEntity>();

    private Vector2 startMousePosition;
    private bool isDragging;

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            startMousePosition = Mouse.current.position.ReadValue();
            isDragging = true;
        }
        else if (context.canceled)
        {
            isDragging = false;
            PerformSelection(Mouse.current.position.ReadValue());
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed && selectedUnits.Count > 0)
        {
            MoveSelectedUnits();
        }
    }

    private void PerformSelection(Vector2 endMousePosition)
    {
        selectedUnits.Clear();
        if (Vector2.Distance(startMousePosition, endMousePosition) < 10f)
        {
            selectSingleUnit();
        }
        else
        {
            SelectMultipleUnits(startMousePosition, endMousePosition);
        }
    }

    private void selectSingleUnit()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, unitLayer))
        {
            UnitEntity unit = hit.collider.GetComponent<UnitEntity>();
            Debug.Log($"{unit.Data.EntityName}선택됨");
        }
    }

    private void SelectMultipleUnits(Vector2 start, Vector2 end)
    {
        //1. 드래그 박스의 크기와 위치 계산(어느 방향으로 드래그하든 사각형이 정상적으로 만들어 지도록 함)
        float minX = Mathf.Min(start.x, end.x);
        float minY = Mathf.Min(start.y, end.y);
        float width = Mathf.Max(start.x - end.x);
        float height = Mathf.Max(start.y - end.y);
        
        Rect selectionRect = new Rect(minX, minY, width, height);
        
        //2. 씬에 있는 모든 유닛 리스트를 가져옴
        //최적화 팁: 실제 게임에서는 매번 FindObjectsByTpe을 호출하면 느려질 수 있습니다.
        //GameManager 등에서 '전체 유닛 리스트'를 미리 관리하고 그걸 가져오는 것이 좋습니다.
        UnitEntity[] allUnits = FindObjectsByType<UnitEntity>(FindObjectsSortMode.None);
        foreach (UnitEntity unit in allUnits)
        {
            //3. 유닛들의 월드 좌표를 화면 좌표로 전환
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
            
            //4. 화면 밖(카메라 뒤쪽)에 있는 유닛은 제외(z값이 음수면 카메라 뒤임)
            if (screenPosition.z < 0) continue;
            
            //5. 드래그 박스 안에 유닛의 화면 좌표가 포함되는지 확인
            //screenPosition은 Vector3지만 Rect.Contains는 x,y만 체크하므로 자동 형변환됨
            if (selectionRect.Contains(screenPosition))
            {
                selectedUnits.Add(unit);
                
                //(선택 사항) 시각적 피드백: 유닛 발 밑에 선택 원 등을 켜주는 메서드 호출
                //Unit.OnSelected();
            }
        }
        Debug.Log($"드래그로 {selectedUnits.Count}개의 유닛이 선택되었습니다.");
    }

    private void MoveSelectedUnits()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            foreach (UnitEntity unit in selectedUnits)
            {
                unit.MoveTo(hit.point);
            }
        }
    }
	// 드래그 박스를 화면에 그리기 위한 변수 (기존 변수 활용)
    private void OnGUI()
    {
        if (isDragging)
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            // GUI 좌표계는 Y축이 반대이므로 변환 필요
            Vector2 guiStart = new Vector2(startMousePosition.x, Screen.height - startMousePosition.y);
            Vector2 guiCurrent = new Vector2(currentMousePosition.x, Screen.height - currentMousePosition.y);

            Rect rect = new Rect(
                Mathf.Min(guiStart.x, guiCurrent.x),
                Mathf.Min(guiStart.y, guiCurrent.y),
                Mathf.Abs(guiStart.x - guiCurrent.x),
                Mathf.Abs(guiStart.y - guiCurrent.y)
            );

            // 반투명한 박스 그리기
            GUI.color = new Color(0, 1, 0, 0.3f); // 녹색 반투명
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            
            // 테두리 (선택 사항)
            GUI.color = Color.green;
            GUI.Box(rect, ""); 
        }
	}
}
