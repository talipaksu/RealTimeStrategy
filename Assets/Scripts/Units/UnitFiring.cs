using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitFiring : NetworkBehaviour
{
    //targeter nesnemiz için referans yaratıyoruz.
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    //mermilerin oluşmaya başlayacağı konum.
    [SerializeField] private Transform projectileSpawnPoint = null;
    //Atış menzili
    [SerializeField] private float fireRange = 5f;
    //Saniyede kaç adet mermi atılacak (1/fireRate)
    [SerializeField] private float fireRate = 1f;
    //Yönünü hedefe dönme hızı
    [SerializeField] private float rotationSpeed = 20f;

    //son atılan merminin atıldığı zamanı tutmak için...
    private float lastFireTime;


    //Sunucuda çalışır, clientta çağrılırken de warning logu atmaz consola. Eğer sadece [Server] deseydik clientlarda consola warning atardı
    //Bunu çağrımını kontrol edemediğimiz fonksiyonlarda yapıyoruz
    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if (target == null) { return; }

        //Hedefimize ateş edebilecek durumda mıyız? Mesafemiz uygun mu?
        if (!CanFireAtTarget()) { return; }

        //İki konum arasında bir rotasyon oluşturur
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);

        //unitimizin yönünü hedefe döndürüyor
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        //Time.time oyun başlangıcından bu yana geçen süreyi verir.
        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            //hedef içindeki aim noktasından, merminin doğduğu noktaya bir rotasyon oluştur (hedef-kaynak)
            Quaternion projectileRotation = Quaternion.LookRotation(target.GetAimAtPoint().position - projectileSpawnPoint.position);

            GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);

            NetworkServer.Spawn(projectileInstance, connectionToClient);

            lastFireTime = Time.time;
        }
    }

    //Eğer clienttan warning logu görüyorsak, çağrım yaptığımız yerde bir hata yapmışız demektir
    [Server]
    private bool CanFireAtTarget()
    {
        //fireRange içinde olup olmadığımızı kontrol ediyoruz.
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude <= fireRange * fireRange;

    }

}
