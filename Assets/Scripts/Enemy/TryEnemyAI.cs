using System;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class TryEnemyAI : MonoBehaviour
    {
        public Transform target;
        public NavMeshAgent agent;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        private void Update()
        {
            Debug.Log(target.name);
            agent.SetDestination(target.position);
        }


    }
}
