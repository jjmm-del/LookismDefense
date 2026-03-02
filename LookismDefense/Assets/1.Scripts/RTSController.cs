using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class RTSController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask enemyLayer;

    //임시로 조합매니저 추가
    [SerializeField] private CombinationManager combinationManager;

    private List<UnitEntity> selectedUnits = new List<UnitEntity>(); // 선택된 유닛
    private EnemyEntity selectedEnemy; // 선택된 적 유닛(상태 확인용)

    private Vector2 startMousePosition;
    private bool isDragging;

    //현재 마우스가 UI 위에 있는지 확인하는 변수
    private bool isPointerOverUI = false;

    // 공격 명령 대기 상태 (A키 누르면 true)
    private bool isAttackCommandPending = false;

    private void Update()
    {
        if (EventSystem.current != null)
        {
            isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        }
    }

public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isAttackCommandPending = true;
            Debug.Log("공격모드 : 목표를 좌클릭 하세요");
            // 추가 마우스 커서의 모양을 공격 모양으로 변경하는 로직
        }
    }

    public void OnStop(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isAttackCommandPending = false;
            foreach (UnitEntity unit in selectedUnits) unit.OrderStop();
        }
    }

    public void OnHold(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            foreach (UnitEntity unit in selectedUnits) unit.OrderHold();
        }
    }
    
    public void OnSelect(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isPointerOverUI)
            {
                return;
            }
            startMousePosition = Mouse.current.position.ReadValue();
            isDragging = true;
        }
        else if (context.canceled)
        {
            if (!isDragging)
            {
                return;
            }
            isDragging = false;
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (isAttackCommandPending)
            {
                ExecuteAttackCommand(mousePosition);
                isAttackCommandPending = false;
                return;
            }
            PerformSelection(Mouse.current.position.ReadValue());
        }
    }
    
    public void OnSmartCommand(InputAction.CallbackContext context)
    {
        if (context.performed && selectedUnits.Count > 0)
        {
            if (isPointerOverUI)
            {
                return;
            }
            isAttackCommandPending = false;
            ExecuteSmartCommand();
        }
    }
    
    private void PerformSelection(Vector2 endMousePosition)
    {
        if (Vector2.Distance(startMousePosition, endMousePosition) < 10f)
        {
            selectSingle();
        }
        else
        {
            SelectMultipleUnits(startMousePosition, endMousePosition);
        }
    }

    private void selectSingle()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, unitLayer))
        {
            UnitEntity unit = hit.collider.GetComponent<UnitEntity>();
			if(unit != null)
            {
                ClearSelection(); // 기존 선택 해제
				selectedUnits.Add(unit);
                unit.SetSelected(true);
            	Debug.Log($"{unit.Data.EntityName}선택됨");
                UIManager.Instance.ShowUnitInfo(unit.Data);
            }
        }
        else if (Physics.Raycast(ray, out hit, 100f, enemyLayer))
        {
            EnemyEntity enemy = hit.collider.GetComponent<EnemyEntity>();
            if (enemy != null)
            {
                ClearSelection();
                selectedEnemy = enemy;
                enemy.SetSelected(true);
                Debug.Log($"적 유닛{enemy.Data.EntityName} 선택됨");
                UIManager.Instance.ShowEnemyInfo(enemy.Data, enemy.CurrentHealth);
            }
        }
        else
        {
            ClearSelection();
            UIManager.Instance.HideInfoPanel();
        }
    }

    private void SelectMultipleUnits(Vector2 start, Vector2 end)
    {
        ClearSelection();
        
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
                unit.SetSelected(true);
                
                //(선택 사항) 시각적 피드백: 유닛 발 밑에 선택 원 등을 켜주는 메서드 호출
                //Unit.OnSelected();
            }
        }
        Debug.Log($"드래그로 {selectedUnits.Count}개의 유닛이 선택되었습니다.");
        // [추가] 선택된 유닛 수에 따른 UI 갱신 분기
        if (selectedUnits.Count == 1)
        {
            //드래그로 1마리만 잡혔다면, 단일 유닛 클릭과 동일하게 정보창 띄우기
            UIManager.Instance.ShowUnitInfo(selectedUnits[0].Data);
        }
        else if (selectedUnits.Count > 1)
        {
            // 여러 마리가 잡혔다면 , 다중 선택(초상화 모음) 창 띄우기
            UIManager.Instance.ShowMultiUnitInfo(selectedUnits,SelectSingleUnitFromUI);
        }
        
    }

    public void SelectSingleUnitFromUI(UnitEntity unit)
    {
        ClearSelection();
        selectedUnits.Add(unit);
        unit.SetSelected(true);
        UIManager.Instance.ShowUnitInfo(unit.Data);
    }

    private void ExecuteAttackCommand(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, enemyLayer))
        {
            EnemyEntity enemy = hit.collider.GetComponent<EnemyEntity>();
            if (enemy != null)
            {
                foreach (UnitEntity unit in selectedUnits) unit.OrderAttackTarget(enemy);
                Debug.Log("타겟 공격 명령");
            }
        }
        else if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            foreach (UnitEntity unit in selectedUnits) unit.OrderAttackMove(hit.point);
            Debug.Log("어택땅");
        }
    }
    private void ExecuteSmartCommand()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        // 적 우클릭 -> 공격
        if (Physics.Raycast(ray, out hit, 100f, enemyLayer))
        {
            EnemyEntity enemy = hit.collider.GetComponent<EnemyEntity>();
            if (enemy != null)
            {
                foreach (UnitEntity unit in selectedUnits)
                {
                    unit.OrderAttackTarget(enemy);
                }
            }
        }
        else if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            foreach (UnitEntity unit in selectedUnits)
            {
                unit.OrderMove(hit.point);
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

    private void ClearSelection()
    {
        // 리스트를 비우기 전에, 현재 담겨있는 모든 유닛의 원을 꺼줍니다.
        foreach (UnitEntity unit in selectedUnits)
        {
            if (unit != null)
            {
                unit.SetSelected(false);
            }
        }
        selectedUnits.Clear();
        
        if (selectedEnemy != null)
        {
            selectedEnemy.SetSelected(false);
        }

        selectedEnemy = null;
        
        //(추가)UI 닫기 등 처리
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideInfoPanel();
        }
    }
    
}
