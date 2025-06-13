using UnityEngine;
using System.Collections.Generic;

public class TrajectoryDrawer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int resolution = 30;
    public float simulationTime = 2f;
    public float timeStep = 0.1f;
    public LayerMask collisionMask;

    public void DrawTrajectory(Vector3 startPos, Vector3 startVelocity)
    {
        lineRenderer.positionCount = resolution;
        Vector3 currentPos = startPos;
        Vector3 velocity = startVelocity;
        Vector3 gravity = Physics.gravity;

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;
            Vector3 point = currentPos + velocity * t + 0.5f * gravity * t * t;

            lineRenderer.SetPosition(i, point);

            // 衝突予測（オプション）
            if (i > 0)
            {
                Vector3 prev = lineRenderer.GetPosition(i - 1);
                if (Physics.Raycast(prev, point - prev, out RaycastHit hit, (point - prev).magnitude, collisionMask))
                {
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);
                    break;
                }
            }
        }
    }

    public void HideTrajectory()
    {
        lineRenderer.positionCount = 0;
    }
}
