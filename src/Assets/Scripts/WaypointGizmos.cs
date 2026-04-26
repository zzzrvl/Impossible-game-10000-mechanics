using UnityEngine;

public class WaypointGizmos : MonoBehaviour
{
    public Color lineColor = Color.cyan;

    void OnDrawGizmos()
    {
        Gizmos.color = lineColor;
        Transform[] nodes = GetComponentsInChildren<Transform>();

        for (var i = 1; i < nodes.Length; i++)
        {
            if (i > 1)
            {
                Gizmos.DrawLine(nodes[i - 1].position, nodes[i].position);
            }

            Gizmos.DrawSphere(nodes[i].position, 0.3f);
        }
    }
}