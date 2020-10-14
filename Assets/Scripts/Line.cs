using UnityEngine;

//need this struct to figure out if the Unit.cs has passed certain boundary
public struct Line
{
    //gradient = slope of line
    //pointPerpendicularToLine = point on the line that is perpendicular to the point of the path [(the point) THAT ALSO LIES ON OUR LINE/CURVE]

    private const float verticalLineGradient = 1e5f;    //10 raised to power 5 = 100,000

    private float slope;
    private float y_intercept;  //y value, where the line meets x = 0

    private Vector2 pointOnLine1, pointOnLine2;

    private float gradientPerpendicular;

    private bool approachSide;

    public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
    {
        float dx = pointOnLine.x - pointPerpendicularToLine.x;
        float dy = pointOnLine.y - pointPerpendicularToLine.y;


        //if line is vertical, dx = 0, which makes dy/dx invalid operation
        if (dx == 0)
            gradientPerpendicular = verticalLineGradient;
        else
            gradientPerpendicular = dy / dx;

        //same logic as above
        if (gradientPerpendicular == 0)
            slope = verticalLineGradient;
        else
            //gradient of line * gradient of line perpendicular to it is -1, hence
            slope = -1 / gradientPerpendicular;

        //y = mx + c is the equation for representing a straight line on the graph
        //x & y are co ords of point on line, m is slope and c is y intercept
        //hence, c = y - mx

        y_intercept = pointOnLine.y - slope * pointOnLine.x;
        pointOnLine1 = pointOnLine;

        //you can calculate another point on the graph by:
        //new point on line = oldPoint + (x, slope * x);
        //yes it makes sense i tried it on a graph
        pointOnLine2 = pointOnLine1 + new Vector2(1, slope);

        //has this point crossed the perpendicularPoint yet?
        approachSide = false;                               //initialising because of constructor norms
        approachSide = GetSide(pointPerpendicularToLine);
    }

    private bool GetSide(Vector2 point)
    {
        return (point.x - pointOnLine1.x) * (pointOnLine2.y - pointOnLine1.y) > (point.y - pointOnLine1.y) * (pointOnLine2.x - pointOnLine1.x);
    }

    //has the given point crossed the point perpendicular to the line
    public bool HasCrossedLine(Vector2 point)
    {
        //return true if the point is no longer on the approachSide
        return GetSide(point) != approachSide;
    }

    public void DrawWithGizmos(int length)
    {
        Vector3 lineDirection = new Vector3(1, 0, slope).normalized;
        Vector3 lineCenter = new Vector3(pointOnLine1.x, 0, pointOnLine1.y) + Vector3.up;

        Gizmos.DrawLine(lineCenter - lineDirection * length / 2f, lineCenter + lineDirection * length / 2f);
    }
}
