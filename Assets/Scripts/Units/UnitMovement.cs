using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class UnitMovement : NetworkBehaviour
{
    //Unit nesnemizin NavMeshAgent componentı
    [SerializeField] private NavMeshAgent agent = null;


    #region Server

    [Command]
    public void CmdMove(Vector3 position)
    {
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
