using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SwipeArrowDrawer : MonoBehaviour
{
    public float arrowHeadLength = 0.3f;
    public float arrowHeadAngle = 20f;
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    public void DrawArrow(Vector3 start, Vector3 end)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Calculate arrowhead
        Vector3 direction = (end - start).normalized;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;

        Vector3 arrowRight = end + right * arrowHeadLength;
        Vector3 arrowLeft = end + left * arrowHeadLength;

        // Draw arrowhead using Debug.DrawLine (or add more positions to LineRenderer if you want persistent arrowhead)
        Debug.DrawLine(end, arrowRight, Color.red, 1f);
        Debug.DrawLine(end, arrowLeft, Color.red, 1f);
    }

    public void ClearArrow()
    {
        lineRenderer.positionCount = 0;
    }
}
