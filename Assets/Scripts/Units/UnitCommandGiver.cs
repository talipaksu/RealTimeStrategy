using System;
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

        //Oyun bitince komutların verilmemesi için GameOverHandler daki client taraflı ClientOnGameOver eventini dinlemeye başlıyoruz.
        //Oyun bitince komutların verilmemesi. scripti disable edecek bir metod çağırıyoruz
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

        //Tıkladığımız şey target mı yoksa bir konum mu, hareket mi etmeliyiz attack mı yapmalıyız
        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            //target olsa bile, dost mu düşman mı
            if (target.hasAuthority)
            {
                //Raycast aracılığı ile vurduğumuz noktaya, seçili Unitlerimizi yönlendirmek için fonksiyonu çağır
                TryMove(hit.point);

                return;
            }

            TryTarget(target);
            return;
        }
        //hiç bir Targetable objeye tıklamadığımız için sadece hareket et
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

    private void TryTarget(Targetable target)
    {
        //seçili olan her Unit için sunucuda çalışmak üzere CmdSetTarget fonksiyonunu çağır ve targetı Unit için set et
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter().CmdSetTarget(target.gameObject);
        }
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}
