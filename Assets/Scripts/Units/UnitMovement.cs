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
    //Takip limit mesafemiz için bir range tutuyoruz.
    [SerializeField] private float chaseRange = 10f;


    #region Server

    public override void OnStartServer()
    {
        //oyun bittiğinde mevcutta hareket eden bir obje varsa pathini nullamak için...
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    //Sunucuda çalışır, clientta çağrılırken de warning logu atmaz consola. Eğer sadece [Server] deseydik clientlarda consola warning atardı
    //Bunu çağrımını kontrol edemediğimiz fonksiyonlarda yapıyoruz
    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        //Unitimizin bir targetı var, takip ettiği bir düşman varsa...
        if (target != null)
        {
            //eğer takip devam ediyorsa... targetımız ve unitimiz arasındaki mesafe chaseRangeimizden büyük ise
            //Vector3.Distance() da kullanılabilirdi fakat performans için pek uygun değil
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }//eğer takip devam etmiyorsa... targetimiz ve unitimiz arası mesafe yeteri kadar yakın. takibi bırak.
            //dipnot sadece takip bırakılıyor. hala hedefimiz mevcut. hedef uzaklaşırsa tekrar takibe başlanacak
            else if (agent.hasPath)
            {
                agent.ResetPath();
            }
            return;
        }
        //Eğer Unitimizin hedefi yok, sıradan bir movement gerçekleştirecek ise...
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

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }

    #endregion


}
