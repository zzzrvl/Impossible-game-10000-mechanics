using UnityEngine;
using UnityEngine.AI;

public class NavMeshWaypoints : MonoBehaviour
{
    public Transform[] waypoints;
    private NavMeshAgent _agent;
    private int _destPoint = 0;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            _agent.Warp(hit.position);
        }

        _agent.updateRotation = false;

        GoToNextPoint();
    }

    void Update()
    {
        if (!_agent.isOnNavMesh)
        {
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            GoToNextPoint();
        }
    }

    void GoToNextPoint()
    {
        if (waypoints.Length == 0)
        {
            return;
        }

        _agent.destination = waypoints[_destPoint].position;
        _destPoint = (_destPoint + 1) % waypoints.Length;
    }
}