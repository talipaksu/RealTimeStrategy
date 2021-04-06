using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class UnitMovement : NetworkBehaviour
{
    //Unit nesnemizin NavMeshAgent componentı
    [SerializeField] private NavMeshAgent agent = null;
    //Targeter sınıfımızdaki metotlara erişebilmek için inspectordan attach ettik ve referans oluşturduk.
    [SerializeField] private Targeter targeter = null;


    #region Server

    //Sunucuda çalışır, clientta çağrılırken de warning logu atmaz consola. Eğer sadece [Server] deseydik clientlarda consola warning atardı
    //Bunu çağrımını kontrol edemediğimiz fonksiyonlarda yapıyoruz
    [ServerCallback]
    private void Update()
    {
        //aşağıdaki logic ile örneğin 3 unit birbirini sürekli itecek iken, sırayla ilk varan itmeyi bırakıyor
        //böylece sırası ile pathleri resetlenerek itişme bitiyor


        //tıklama aksiyonumuz aynı frame içine denk geldiğinde ortaya çıkan bugı engellemek için...
        if (!agent.hasPath) { return; }

        //henüz varmak istediği hedefe erişememiş. fonksiyondan çık
        if (agent.remainingDistance > agent.stoppingDistance) { return; }

        //varmak istediğimiz hedefte isek pathi temizliyoruz ki sürekli unitler birbirini iterek hedefe erişmeye çalışmasın
        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        //hedef alındıktan sonra herhangi bir yere tıklandığında targetların temizlenmesi için...
        targeter.ClearTarget();
        //hedef position, Navmesh için uygun bir konum değilse, fonksiyondan çık
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            return;
        }
        //nesneyi position konumuna yönlendir
        agent.SetDestination(hit.position);
    }

    #endregion


}
