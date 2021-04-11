using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    //nesnenin yokedildiğinde olacakları yönetmek için healthtan bir referans oluşturuyoruz
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;

    #region Server

    public override void OnStartServer()
    {
        //healtta tetiklenen serverOnDie ı dinlemeye başla
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        //Bu artık UnitBase de yapılıyor
        //NetworkServer.Destroy(this.gameObject);
    }


    [Command]
    private void CmdSpawnUnit()
    {
        //sunucuda bir Unit örneği oluştur
        GameObject unitInstance = Instantiate(unitPrefab, unitSpawnPoint.position, unitSpawnPoint.rotation);
        //sunucuda oluşturmuş olduğum Unit örneğini, NetworkBehaviour un sağladığı connectionToClient connection bilgisi ile kullanıcı ile ilişkilendirerek
        //tüm clientlarda oluştur.
        NetworkServer.Spawn(unitInstance, connectionToClient);
    }

    #endregion

    #region Client

    public void OnPointerClick(PointerEventData eventData)
    {
        //eğer left mouse button ile UnitSpawner a basıldı ise gerekli aksiyonları al
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        //clientta çalıştığı için başka playerların nesnesine müdahale etmemek adına Authority kontrolü yap
        if (!hasAuthority) { return; }

        //Client, sunucuya "benim için unit oluştur" diyor
        CmdSpawnUnit();
    }

    #endregion

}
