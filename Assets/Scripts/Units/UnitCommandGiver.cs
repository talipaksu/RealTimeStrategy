using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    //Seçili olan Unitlere erişmek için UnitSelectionHandler sınıfının bir referansını oluştur
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Camera mainCamera;


    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

        //Raycast aracılığı ile vurduğumuz noktaya, seçili Unitlerimizi yönlendirmek için fonksiyonu çağır
        TryMove(hit.point);
    }

    private void TryMove(Vector3 point)
    {
        //seçili olan her Unit için sunucuda çalışmak üzere CmdMove fonksiyonunu çağır
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().CmdMove(point);
        }
    }
}
