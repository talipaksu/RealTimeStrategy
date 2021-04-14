using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//tamamen client taraflı çalışacak
//istediğimiz buildingi seçip ekrana sürükledikten sonra servera bu pozisyona şu buildingi kurmak istediğimizi söyleyeceğiz
public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    //Butonun sunacağı buildingi ifade eder
    //oyun sahnesine koyacağımız prefab örneğinde bu özelleştirilecek. örneğin spawner building tipi için UnitSpawner verilecek
    [SerializeField] private Building building = null;
    //Buildingteki icon set edilecektir.
    [SerializeField] private Image iconImage = null;
    //Buildingteki price değeri set edilecektir.
    [SerializeField] private TMP_Text priceText = null;
    //buildinglerimizi floor üzerine kuracağımız için LayerMask adını floorMask olarak verdik
    [SerializeField] private LayerMask floorMask = new LayerMask();

    private Camera mainCamera;
    private RTSPlayer player;
    //sürükleyip konumlandırmaya çalışırken gerçek objeyi değil sadece görüntüsünü göreceğiz, o yüzden geçici bir GameObject oluşturuyoruz.
    private GameObject buildingPreviewInstance;
    //bir konuma yerleştirip yerleştiremeyeceğimizi göstermek için kırmızı ya da yeşil uyarı rengi çıkartmalıyız
    private Renderer buildingRendererInstance;



    private void Start()
    {
        mainCamera = Camera.main;

        //oyun boyunca icon ve price değişmeyeceği için burada set ediyoruz.
        iconImage.sprite = building.GetIcon();
        priceText.text = building.GetPrice().ToString();
    }

    private void Update()
    {
        if (player == null)
        {
            //connection üzerinden bu connectiona ait RTSPlayer nesnemize erişiyoruz. 
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        if (buildingPreviewInstance == null) { return; }

        UpdateBuildingPreview();
    }

    //mouse a tıkladığımızda previewInstance ımızı oluşturacak metod
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();

        buildingPreviewInstance.SetActive(false);

    }

    //mousedan parmağımızı çektiğimizde tetiklenecek ve buildingi yerleştirecek metod
    public void OnPointerUp(PointerEventData eventData)
    {
        if (buildingRendererInstance == null) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        //floor layermaskında ray, sonsuz uzaklık içerisinde bir cisme çarptıysa
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            //place building
            player.CmdTryPlaceBuilding(building.GetId(), hit.point);
        }

        Destroy(buildingPreviewInstance);
    }

    //previewInstance sürüklenirken tetiklenecek method
    private void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        //eğer hiç bir yere çarpmadıysa ray, fonksiyondan çık
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }

        buildingPreviewInstance.transform.position = hit.point;

        if (!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }
    }

}
