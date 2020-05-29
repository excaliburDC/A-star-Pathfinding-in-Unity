using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line 
{
    const float verticalLineGradient = 1e5f;

    private float gradient;
    private float y_intercept;
    private float gradientPerpendicular;

    private Vector2 pointOnLine1;
    private Vector2 pointOnLine2;

    private bool approachSide;

    public Line(Vector2 pointOnLine,Vector2 pointPerpendicularToLine)
    {
        float dx = pointOnLine.x - pointPerpendicularToLine.x;
        float dy = pointOnLine.y - pointPerpendicularToLine.y;

        if(dx==0)
        {
            gradientPerpendicular = verticalLineGradient;
        }
        else
        {
            gradientPerpendicular = dx / dy;
        }

        if(gradientPerpendicular==0)
        {
            gradient = verticalLineGradient;
        }
        else
        {
            gradient = -1 / gradientPerpendicular;
        }

        //equation of line y=mx+c where c is the y_intercept,so after rearranging 
        //c=y-mx
        y_intercept = pointOnLine.y - gradient * pointOnLine.x;

        pointOnLine1 = pointOnLine;
        pointOnLine2 = pointOnLine + new Vector2(1, gradient);

        approachSide = false;
        approachSide = GetSide(pointPerpendicularToLine);
    }

    private bool GetSide(Vector2 p)
    {
        return (p.x - pointOnLine1.x) * (pointOnLine2.y - pointOnLine1.y) > (p.y - pointOnLine1.y) * (pointOnLine2.x - pointOnLine1.x);
    }

    public bool HasCrossedLine(Vector2 p)
    {
        return GetSide(p) != approachSide;
    }

    //called when the object is near the end of its path so as to slow down its speed while appraoching the finish line
    public float DistanceFromPoint(Vector2 p)
    {
        float yInterceptPerpendicular = p.y - gradientPerpendicular * p.x;
        float intersectX = (yInterceptPerpendicular - y_intercept) / (gradient - gradientPerpendicular);
        float intersectY = gradient * intersectX + y_intercept;

        return Vector2.Distance(p, new Vector2(intersectX, intersectY));
    }

    public void DrawWithGizmos(float length)
    {
        Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
        Vector3 lineCentre = new Vector3(pointOnLine1.x, 0, pointOnLine1.y) + Vector3.up;
        Gizmos.DrawLine(lineCentre - lineDir * length / 2f, lineCentre + lineDir * length / 2f);
    }

}
