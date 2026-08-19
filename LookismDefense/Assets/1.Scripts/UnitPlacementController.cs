using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class UnitPlacementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private UnitSelectionController selectionController;

    [Header("Layer")]
    [SerializeField] private LayerMask gridCellLayer;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void OnPlace(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (IsPointerOverUI())
            return;

        UnitEntity selectedUnit =
            selectionController.PrimarySelectedUnit;

        if (selectedUnit == null)
            return;

        UnitAIController selectedAI =
            selectedUnit.GetComponent<UnitAIController>();

        if (selectedAI == null)
            return;

        // 일단 전투 중 재배치 금지
        if (selectedAI.CurrentState != UnitAIState.Idle)
        {
            Debug.Log("전투 중에는 유닛을 재배치할 수 없습니다.");
            return;
        }

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            mainCamera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, gridCellLayer))
        {
            return;
        }

        GridCell targetCell =
            hit.collider.GetComponentInParent<GridCell>();

        if (targetCell == null)
            return;

        TryPlaceUnit(selectedUnit, selectedAI, targetCell);
    }

    private void TryPlaceUnit(UnitEntity selectedUnit, UnitAIController selectedAI, GridCell targetCell)
    {
        GridCell currentCell = selectedAI.HomeCell;

        if (currentCell == null)
            return;

        // 자기 자리 클릭
        if (currentCell == targetCell)
            return;

        if (!targetCell.IsOccupied)
        {
            MoveToEmptyCell(selectedUnit, selectedAI, currentCell, targetCell);
        }
        else
        {
            SwapUnits(selectedUnit, selectedAI, currentCell, targetCell);
        }
    }

    private void MoveToEmptyCell(UnitEntity selectedUnit, UnitAIController selectedAI, GridCell currentCell, GridCell targetCell)
    {
        // 기존 Cell 점유 해제
        currentCell.RemoveUnit();

        // 새 Cell 점유
        bool success =
            targetCell.TryPlaceUnit(selectedUnit.gameObject);

        if (!success)
        {
            // 실패 시 원상복구
            currentCell.TryPlaceUnit(selectedUnit.gameObject);
            return;
        }

        // AI HomeCell 갱신
        selectedAI.SetHomeCell(targetCell);

        Debug.Log(
            $"{selectedUnit.Data.EntityName} 이동 : {currentCell.Coordinate} → {targetCell.Coordinate}"
        );
    }

    private void SwapUnits(UnitEntity selectedUnit, UnitAIController selectedAI, GridCell currentCell, GridCell targetCell)
    {
        GameObject targetUnitObject = targetCell.OccupiedUnit;

        if (targetUnitObject == null)
            return;

        UnitEntity targetUnit = targetUnitObject.GetComponent<UnitEntity>();

        UnitAIController targetAI = targetUnitObject.GetComponent<UnitAIController>();

        if (targetUnit == null || targetAI == null)
            return;

        // 상대 유닛도 전투 중이면 Swap 금지
        if (targetAI.CurrentState != UnitAIState.Idle)
        {
            Debug.Log("전투 중인 유닛과는 위치를 바꿀 수 없습니다.");
            return;
        }

        // 두 Cell 점유 해제
        currentCell.RemoveUnit();
        targetCell.RemoveUnit();

        // 서로 반대 Cell에 배치
        bool selectedPlaced = targetCell.TryPlaceUnit(selectedUnit.gameObject);

        bool targetPlaced = currentCell.TryPlaceUnit(targetUnit.gameObject);

        if (!selectedPlaced || !targetPlaced)
        {
            Debug.LogError("Unit Swap 실패");

            // 혹시 하나라도 실패했으면 최대한 원복
            currentCell.RemoveUnit();
            targetCell.RemoveUnit();

            currentCell.TryPlaceUnit(selectedUnit.gameObject);
            targetCell.TryPlaceUnit(targetUnit.gameObject);

            return;
        }

        selectedAI.SetHomeCell(targetCell);
        targetAI.SetHomeCell(currentCell);

        Debug.Log($"{selectedUnit.Data.EntityName} ↔ {targetUnit.Data.EntityName} 위치 교환"
        );
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }
}