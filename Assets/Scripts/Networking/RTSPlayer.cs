using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class RTSPlayer : NetworkBehaviour
{
    //Oyun genelindeki building çeşitlerini tutmak amaçlı bir dizi oluşturuyoruz.
    //Boyutunu 0 verdik ama Unityde Inspectordan listeye build eklemesini yaptık Player objemiz üzerinden
    [SerializeField] private Building[] buildings = new Building[0];

    //resources sunucuda tutulacak, oyuncular tarafından hacklenmesini istemeyiz
    //sunucu değeri değiştirince clientlara, value değişti der ve ClientHandleResourcesUpdated metodu clientlarda çalışır
    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;
    public event Action<int> ClientOnResourcesUpdated;

    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

    public int GetResources()
    {
        return this.resources;
    }

    [Server]
    public void SetResources(int newResources)
    {
        this.resources = newResources;
    }

    //playera ait olan unitlara erişebilmek için...
    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    //playera ait olan buildinglara erişebilmek için...
    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }



    #region Server
    public override void OnStartServer()
    {
        //Player nesnesi Serverda oluşturulduğu zaman, Unitteki static Eventlar dinlenilmeye başlanıyor
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;

        //Player nesnesi Serverda oluşturulduğu zaman, Buildingdeki static Eventlar dinlenilmeye başlanıyor
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;
    }

    public override void OnStopServer()
    {
        //Player nesnesi Serverda kaldırıldığı zaman, Unitteki static Eventlar dinlenilmeyi bırakıyor
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;

        //Player nesnesi Serverda kaldırıldığı zaman, Buildingdeki static Eventlar dinlenilmeyi bırakıyor
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }

    //Buildingi inşa edecek fonksiyondur. BuildingButton scriptinden clientımız sunucuya "buraya building kur" diyecek ve bu fonksiyonu çağıracak
    //bütün objeyi networkte göndermek yerine sadece id sini göndererek sunucudan ilgili buildingi inşa etmesini istiyoruz
    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 point)
    {
        Building buildingToPlace = null;

        //tüm building çeşitlerimizi tutan listemizi dön
        foreach (Building building in buildings)
        {
            //oluşturmak istediğimiz building bu listede ise onu al ve looptan çık
            if (building.GetId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }

        //eğer bulamadıysak fonksiyondan çık.
        if (buildingToPlace == null) { return; }

        //istenen konumda istenen buildingimizi oluştur
        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);

        //clientımızla ilişkilendir
        NetworkServer.Spawn(buildingInstance, connectionToClient);
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

    private void ServerHandleBuildingSpawned(Building building)
    {
        //Server tarafındaki player örneğinde tutulan myBuilding listesine, eğer yeni oluşturulan building clienttaki örneğimle ilişkili ise, unit ekleniyor
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
        myBuildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        //Server tarafındaki player örneğinde tutulan myBuilding listesine, eğer kaldırılan building clienttaki örneğimle ilişkili ise, unit kaldırılıyor
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
        myBuildings.Remove(building);
    }

    #endregion

    #region Client
    //OnStartClient ı değiştirdik. Çünkü Authority'miz olmayan nesneler için bile dinlemeye başlıyorduk. Bu garip bir yaklaşım olurdu 
    public override void OnStartAuthority()
    {
        //!isClientOnly yi kaldırdık. Çünkü isClientOnly player objemiz oluşturulduğunda doğruyu gösteriyor
        //sunucu değil isek kontrolü için...
        //Bu makine sunucu olarak çalışıyorsa fonksiyondan çık
        if (NetworkServer.active) { return; }
        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;

        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStopClient()
    {
        //sunucu isek ya da yetkimiz yoksa fonksiyondan çık
        if (!isClientOnly || !hasAuthority) { return; }
        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;

        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    private void ClientHandleResourcesUpdated(int oldResources, int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        //authority checkini kaldırdık. çünkü çağrıldığı zaten bu kontrolü yapıyor
        //Yetkim dahilinde ise clientta bulunan myUnits listesine ilgili Unit ekleniyor
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        //authority checkini kaldırdık. çünkü çağrıldığı zaten bu kontrolü yapıyor
        //Yetkim dahilinde ise clientta bulunan myUnits listesinden ilgili Unit kaldırılıyor
        myUnits.Remove(unit);
    }


    private void AuthorityHandleBuildingSpawned(Building building)
    {
        //authority checkini kaldırdık. çünkü çağrıldığı zaten bu kontrolü yapıyor
        //Yetkim dahilinde ise clientta bulunan myBuildings listesine ilgili building ekleniyor
        myBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        //authority checkini kaldırdık. çünkü çağrıldığı zaten bu kontrolü yapıyor
        //Yetkim dahilinde ise clientta bulunan myBuildings listesinden ilgili building kaldırılıyor
        myBuildings.Remove(building);
    }

    #endregion
}
