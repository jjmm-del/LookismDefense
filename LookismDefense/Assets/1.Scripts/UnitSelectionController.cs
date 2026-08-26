using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class UnitSelectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    
    [Header("Layer")]
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Selection")]
    [SerializeField] private float dragThreshold = 10f;
    
    private List<UnitEntity> selectedUnits = new();  // 선택된 유닛
    private EnemyEntity selectedEnemy; // 선택된 적 유닛(상태 확인용)

    private Vector2 startMousePosition;
    private bool isDragging;


    public IReadOnlyList<UnitEntity> SelectedUnits => selectedUnits;
    public EnemyEntity SelectedEnemy => selectedEnemy;
    
    public UnitEntity PrimarySelectedUnit => selectedUnits.Count > 0 ? selectedUnits[0] : null;

    public event Action OnSelectionChanged;
    
    //현재 마우스가 UI 위에 있는지 확인하는 변수
    
    

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (EntityRegistry.Instance != null)
        {
            EntityRegistry.Instance.OnUnitUnregistered += HandleUnitRemoved;
            EntityRegistry.Instance.OnEnemyUnregistered += HandleEnemyRemoved;
            
        }
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (IsPointerOverUI())
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
            
            Vector2 endMousePosition = Mouse.current.position.ReadValue();
            
            PerformSelection(endMousePosition);
        }
    }
    
    
    private void PerformSelection(Vector2 endMousePosition)
    {
        float dragDistance = Vector2.Distance(startMousePosition, endMousePosition);
        if (dragDistance  < dragThreshold)
        {
            selectSingle(endMousePosition);
        }
        else
        {
            SelectMultipleUnits(startMousePosition, endMousePosition);
        }
    }

    private void selectSingle(Vector2 mousePosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        
        //아군
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, unitLayer))
        {
            UnitEntity unit = hit.collider.GetComponentInParent<UnitEntity>();
			if(unit != null)
            {
                ClearSelection(false); // 기존 선택 해제
				selectedUnits.Add(unit);
                unit.SetSelected(true);
            	Debug.Log($"{unit.DisplayName}선택됨");
                UIManager.Instance?.ShowUnitInfo(unit);
                OnSelectionChanged?.Invoke();
                return;
            }
        }
        
        //적
        if (Physics.Raycast(ray, out hit, 100f, enemyLayer))
        {
            EnemyEntity enemy = hit.collider.GetComponentInParent<EnemyEntity>();
            
            if (enemy != null)
            {
                ClearSelection(false);
                selectedEnemy = enemy;
                enemy.SetSelected(true);
                Debug.Log($"적 유닛{enemy.Data.EntityName} 선택됨");
                UIManager.Instance?.ShowEnemyInfo(enemy);
                OnSelectionChanged?.Invoke();
                return;
            }
        }
        
        ClearSelection();
    }

    private void SelectMultipleUnits(Vector2 start, Vector2 end)
    {
        ClearSelection(false);
        
        //1. 드래그 박스의 크기와 위치 계산(어느 방향으로 드래그하든 사각형이 정상적으로 만들어 지도록 함)
        float minX = Mathf.Min(start.x, end.x);
        float minY = Mathf.Min(start.y, end.y);
        float width = Mathf.Abs(end.x - start.x);
        float height = Mathf.Abs(end.y - start.y);
        
        Rect selectionRect = new Rect(minX, minY, width, height);
        
        //2. 씬에 있는 모든 유닛 리스트를 가져옴
        //최적화 팁: 실제 게임에서는 매번 FindObjectsByTpe을 호출하면 느려질 수 있습니다.
        //GameManager 등에서 '전체 유닛 리스트'를 미리 관리하고 그걸 가져오는 것이 좋습니다.
        if (EntityRegistry.Instance == null)
        {
            RefreshSelectionUI();
            OnSelectionChanged?.Invoke();
            return;
        }

        IReadOnlyList<UnitEntity> allUnits = EntityRegistry.Instance.PlayerUnits;
        foreach (UnitEntity unit in allUnits)
        {
            //3. 유닛들의 월드 좌표를 화면 좌표로 전환
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
            
            //4. 화면 밖(카메라 뒤쪽)에 있는 유닛은 제외(z값이 음수면 카메라 뒤임)
            if (screenPosition.z < 0) continue;
            
            //5. 드래그 박스 안에 유닛의 화면 좌표가 포함되는지 확인
            //screenPosition은 Vector3지만 Rect.Contains는 x,y만 체크하므로 자동 형변환됨
            if (!selectionRect.Contains(screenPosition))
                continue;
            
            selectedUnits.Add(unit);
            unit.SetSelected(true);
        }

        RefreshSelectionUI();
        OnSelectionChanged?.Invoke();

    }

    public void SelectSingleUnitFromUI(UnitEntity unit)
    {
        if (unit == null)
            return;
        
        ClearSelection(false);
        selectedUnits.Add(unit);
        unit.SetSelected(true);
        UIManager.Instance?.ShowPanel<UnitInfoPanelUI>(panel => panel.SetData(unit));
        OnSelectionChanged?.Invoke();
    }
    
	

    private void ClearSelection(bool notify = true)
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
            selectedEnemy = null;
        }
        
        //(추가)UI 닫기 등 처리
        UIManager.Instance?.CloseCurrentPanel();

        if (notify)
        {
            OnSelectionChanged?.Invoke();
        }
    }

    private void RefreshSelectionUI()
    {
        if (selectedUnits.Count == 1)
        {
            UIManager.Instance?.ShowUnitInfo(selectedUnits[0]);
        }
        else if (selectedUnits.Count > 1)
        {
            UIManager.Instance?.ShowMultiUnitInfo(selectedUnits, SelectSingleUnitFromUI);
        }
        else
        {
            UIManager.Instance?.CloseCurrentPanel();
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    
    
    private void HandleUnitRemoved(UnitEntity unit)
    {
        if (unit == null)
            return;

        if (selectedUnits.Remove(unit))
        {
            RefreshSelectionUI();
            OnSelectionChanged?.Invoke();
        }
    }

    private void HandleEnemyRemoved(EnemyEntity enemy)
    {
        if (enemy == null)
            return;
        
        if (selectedEnemy != enemy)
            return;

        selectedEnemy = null;
        
        UIManager.Instance?.CloseCurrentPanel();

        OnSelectionChanged?.Invoke();
    }
    // 드래그 박스를 화면에 그리기 위한 변수 (기존 변수 활용)
    private void OnGUI()
    {
        if (!isDragging)
            return;
        
        Vector2 current = Mouse.current.position.ReadValue();
        // GUI 좌표계는 Y축이 반대이므로 변환 필요
        Vector2 guiStart = new (startMousePosition.x, Screen.height - startMousePosition.y);
        Vector2 guiCurrent = new (current.x, Screen.height - current.y);

        Rect rect = new (
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
        
        GUI.color = Color.white;
    }
    private void OnDestroy()
    {
        if (EntityRegistry.Instance != null)
        {
            EntityRegistry.Instance.OnUnitUnregistered -= HandleUnitRemoved;
            EntityRegistry.Instance.OnEnemyUnregistered -= HandleEnemyRemoved;
            
        }
    }
}
