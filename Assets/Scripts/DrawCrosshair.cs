using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DrawCrosshair : MonoBehaviour
{
    [SerializeField] LineRenderer Crosshair;
    [SerializeField] Transform CurrentCrosshairPos;
    public float width;
    public int steps; // Number of segments in the circle
    public float radius;
    // Start is called before the first frame update
    void Start()
    {
        // Set up the LineRenderer properties
        Crosshair.startWidth = width;
        Crosshair.endWidth = width;
        Crosshair.loop = true; // Ensures the circle closes
        Crosshair.material = new Material(Shader.Find("Sprites/Default"));
    }

    // Update is called once per frame
    void Update()
    {
        // Draw the crosshair
        DrawCrosshairLine(Crosshair, steps, radius);
    }

    void DrawCrosshairLine(LineRenderer Crosshair, int steps, float radius)
    {
        List<Vector3> points = new List<Vector3>();
        float angleStep = 360f / steps;
        Vector3 center = new Vector3(CurrentCrosshairPos.position.x, CurrentCrosshairPos.position.y, CurrentCrosshairPos.position.z);

        for (int i = 0; i <= steps; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 point = new Vector3(Mathf.Cos(angle) * radius + center.x, Mathf.Sin(angle) * radius + center.y, center.z);
            points.Add(point);
        }

        // Update LineRenderer with points
        Crosshair.positionCount = points.Count;
        Crosshair.SetPositions(points.ToArray());
    }
}
