using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NavMeshWaypoints : MonoBehaviour
{
    public GameObject waypointsHandler;

    [Tooltip("Введите номера детей (нумерация с 1) через пробел, например: 0 2 1")]
    public string orderString = "";

    private readonly List<Transform> _waypoints = new();
    private NavMeshAgent _agent;
    private int _destPoint;

    [Header("Status")] [SerializeField] private string currentTargetName; // Поле только для визуализации

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;

        InitializeWaypointsByIndices();

        if (NavMesh.SamplePosition(transform.position, out var hit, 1.0f, NavMesh.AllAreas))
        {
            _agent.Warp(hit.position);
        }

        GoToNextPoint();
    }

    private void InitializeWaypointsByIndices()
    {
        if (waypointsHandler == null || string.IsNullOrEmpty(orderString))
        {
            return;
        }

        var indexStrings = orderString.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        var totalChildren = waypointsHandler.transform.childCount;

        foreach (var str in indexStrings)
        {
            if (int.TryParse(str, out var index))
            {
                if (1 <= index && index <= totalChildren)
                {
                    _waypoints.Add(waypointsHandler.transform.GetChild(index - 1));
                }
                else
                {
                    Debug.LogWarning($"Index {index} out of range! Object has only {totalChildren} children.");
                }
            }
            else
            {
                Debug.LogWarning($"'{str}' is not a number!");
            }
        }
    }

    private void Update()
    {
        if (_waypoints.Count == 0 || !_agent.isOnNavMesh)
        {
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            GoToNextPoint();
        }
    }

    private void GoToNextPoint()
    {
        if (_waypoints.Count == 0)
        {
            return;
        }

        _agent.destination = _waypoints[_destPoint].position;

        currentTargetName = $"Index: {_destPoint + 1} | Name: {_waypoints[_destPoint].name}";
        _destPoint = (_destPoint + 1) % _waypoints.Count;
    }
}