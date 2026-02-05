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
        
    }

    private void SelectMultipleUnits(Vector2 start, Vector2 end)
    {
        
    }

    private void MoveSelectedUnits()
    {
        
    }
}
