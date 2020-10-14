using UnityEngine;

public class Path
{
    public readonly Vector3[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;

    public Path(Vector3[] waypoints, Vector3 startPos, int turnDistance)
    {
        lookPoints = waypoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = makeVector2(startPos);
        for (int i = 0; i < lookPoints.Length; i++)
        {
            Vector2 currentPoint = makeVector2(lookPoints[i]);
            Vector2 directionToCurrentPoint = (currentPoint - previousPoint).normalized;

            //we don't want it to think about turning on the last waypoint
            Vector2 turnBoundaryPoint = (i == finishLineIndex) ? (currentPoint) : currentPoint - directionToCurrentPoint * turnDistance;

            //only previous point can be sent here, but if the dist between 2 points is less than turn distance, then Line.HasCrossedLine wont work
            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - directionToCurrentPoint * turnDistance);
            previousPoint = turnBoundaryPoint;
        }
    }

    private Vector2 makeVector2(Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }

    public void DrawWithGizmos()
    {
        Gizmos.color = Color.black;
        foreach (var point in lookPoints)
            Gizmos.DrawCube(point + Vector3.up, Vector3.one);

        Gizmos.color = Color.white;
        foreach (var line in turnBoundaries)
            line.DrawWithGizmos(10);
    }
}
