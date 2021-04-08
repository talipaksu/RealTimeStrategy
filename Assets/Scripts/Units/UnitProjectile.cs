using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    //verilecek hasar
    [SerializeField] private int damageToDeal = 20;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;

    //düz bir doğrultuda ilerleyecek bir nesne için herhangi bir senkronizasyon ihtiyacı yok
    void Start()
    {
        //yüzünün döndüğü yere doğru güç uyguluyoruz
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer()
    {
        //obje oluşturulduktan 5 saniye sonra DestroySelf() metodunu çağır ve kendini yoket
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    //mermi bir objeye çarptığında...
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        //düşman obje ile çarpışıp çarpışmadığını kontrol ediyoruz
        if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            //dost obje ise fonksiyondan çık
            if (networkIdentity.connectionToClient == connectionToClient) { return; }
        }

        //çarpıştığım nesnenin health componentına eriş
        if (other.TryGetComponent<Health>(out Health health))
        {
            //canını azalt
            health.DealDamage(damageToDeal);
        }

        //çarpışma sonrası mermiyi yok et
        DestroySelf();
    }

    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
