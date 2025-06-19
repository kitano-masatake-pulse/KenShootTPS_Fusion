using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class TrajectoryDrawer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int resolution = 3000;
    public float timeStep = 0.001f;
    public LayerMask collisionMask;

    public GameObject impactMarkerPrefab;  // 半球プレハブ
    private GameObject impactMarkerInstance; // 実体
    private  float grenadeRadius = 0.001f;
    private bool impactPointFound = false;

    private void Awake()
    {
        // 初期化：インスタンス生成（1つだけ）
        if (impactMarkerPrefab != null)
        {
            impactMarkerInstance = Instantiate(impactMarkerPrefab);
            impactMarkerInstance.SetActive(false); // 最初は非表示
        }
    }



    public void SphereCastDrawTrajectory(Vector3 startPos, Vector3 velocity)
    {

        Debug.Log("SphereCastDrawTrajectory called with startPos: " + startPos + ", velocity: " + velocity);
        lineRenderer.positionCount = resolution;
        Vector3 gravity = Physics.gravity;

        impactPointFound = false;

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPos + velocity * t + 0.5f * gravity * t * t;
            lineRenderer.SetPosition(i, point);

            // 衝突予測（オプション）
            if (i > 0)
            {
                Vector3 prev = lineRenderer.GetPosition(i - 1);
                Vector3 direction = point - prev;
                float distance = direction.magnitude;

                if (Physics.SphereCast(prev, grenadeRadius, direction.normalized, out RaycastHit hit, distance, collisionMask))
                {
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);

                    if (impactMarkerInstance != null)
                    {
                        impactMarkerInstance.SetActive(true);
                        impactMarkerInstance.transform.position = hit.point;
                        impactMarkerInstance.transform.rotation = Quaternion.LookRotation(hit.normal);
                        impactPointFound = true;
                    }

                    break;
                }
            }
        }
        if (!impactPointFound && impactMarkerInstance != null)
        {
            impactMarkerInstance.SetActive(false);
        }    }



    public void HideTrajectory()
    {
        lineRenderer.positionCount = 0;
        if (impactMarkerInstance != null)
            impactMarkerInstance.SetActive(false);
    }
}
