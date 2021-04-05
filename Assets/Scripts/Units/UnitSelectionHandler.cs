using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    //Aşağıdaki kodların hepsi Clientta çalışır

    //multi select alanımızı tutabilmek için referans oluşturduk
    [SerializeField] private RectTransform unitSelectionArea = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    //multi select areamızın başlangıç pozisyonu
    private Vector2 startPosition;

    //bizim playerımıza ait unitlere erişebilmek için player referansı oluşturduk
    private RTSPlayer player;
    private Camera mainCamera;
    public List<Unit> SelectedUnits { get; } = new List<Unit>();


    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (player == null)
        {
            //connection üzerinden bu connectiona ait RTSPlayer nesnemize erişiyoruz. Bunun üzerinden de sakladığımız unit listemize erişeceğiz
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        //eğer içinde bulunduğum framede mouse left clicke basarsam
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        //eğer içinde bulunduğum framede mouse left clickten elimi çekersem
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            //seçim alanını temizle ve gerekli unitleri seç
            ClearSelectionArea();
        }
        //left clicke basılı tuttuğum case
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    private void StartSelectionArea()
    {
        //seçili olan tüm Unitlerin highlightını kaldır
        foreach (Unit selectedUnit in SelectedUnits)
        {
            selectedUnit.Deselect();
        }

        //seçili unit listemi temizle
        SelectedUnits.Clear();

        //aynı zamanda disable olan multi select areamızı enable ediyoruz
        unitSelectionArea.gameObject.SetActive(true);
        //aynı zamanda multi select areamızın başlangıç noktasını tutuyoruz
        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }
    private void UpdateSelectionArea()
    {
        //multi select areamızın yükseklik ve genişliği için aşağıdaki hesaplamaları yapıyoruz
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        //multi select areamızı kaldırıyoruz
        unitSelectionArea.gameObject.SetActive(false);

        //multi select yapmadığımız yani tek bir unit seçmek istediğimiz zaman...
        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            //Tıklanan Unitleri seçmek adına bir ışın yarat. Işını yaratırken mouse un pozisyonunu ve main camerayı kullan.
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            //eğer oluşturulan ışın, layerMask (ki kendisi Unity'de Default olarak atandı) içerisinde bir nesneye çarpmadı ise fonksiyondan çık
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

            //hit değişkeni içerisinde tutulan ve ışınımızın çarptığı nesne Unit değil ise fonksiyondan çık
            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) { return; }

            //Client'ta çalıştığı için Authority kontrolü yap
            if (!unit.hasAuthority) { return; }

            //seçilen Unit i SelectedUnits listesine at
            SelectedUnits.Add(unit);

            //seçilen Unitlerin hightlightını enable et
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }

            return;
        }

        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        //Dikkat!
        //multi select areamız ScreenSpace
        //unitlerimiz WorldSpace
        foreach (Unit unit in player.GetMyUnits())
        {
            //unitimizin Screen positionunu almak için...
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            //unitimiz multi select areamız içindeyse...
            if (screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }
}
