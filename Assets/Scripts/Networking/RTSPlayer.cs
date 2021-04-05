using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private List<Unit> myUnits = new List<Unit>();

    //playera ait olan unitlara erişebilmek için...
    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    #region Server
    public override void OnStartServer()
    {
        //Player nesnesi Serverda oluşturulduğu zaman, Unitteki static Eventlar dinlenilmeye başlanıyor
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
    }

    public override void OnStopServer()
    {
        //Player nesnesi Serverda kaldırıldığı zaman, Unitteki static Eventlar dinlenilmeyi bırakıyor
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
    }

    private void ServerHandleUnitSpawned(Unit unit)
    {
        //Server tarafındaki player örneğinde tutulan myUnits listesine, eğer yeni oluşturulan Unit clienttaki örneğimle ilişkili ise, unit ekleniyor
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
        myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        //Server tarafındaki player örneğinde tutulan myUnits listesine, eğer kaldırılan Unit clienttaki örneğimle ilişkili ise, unit kaldırılıyor
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
        myUnits.Remove(unit);
    }
    #endregion

    #region Client

    public override void OnStartClient()
    {
        //Player nesnesi Clientta oluşturulduğu zaman,ve sadece Clientta çalışıyorsa (Host + Client değil) Unitteki static Eventlar dinlenilmeye başlanıyor
        if (!isClientOnly) { return; }
        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
    }

    public override void OnStopClient()
    {
        //Player nesnesi Clientta kaldırıldığı zaman,ve sadece Clientta çalışıyorsa (Host + Client değil) Unitteki static Eventlar dinlenilmeyi bırakıyor
        if (!isClientOnly) { return; }
        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        //Yetkim dahilinde ise clientta bulunan myUnits listesine ilgili Unit ekleniyor
        if (!hasAuthority) { return; }
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        //Yetkim dahilinde ise clientta bulunan myUnits listesinden ilgili Unit kaldırılıyor
        if (!hasAuthority) { return; }
        myUnits.Remove(unit);
    }

    #endregion
}
