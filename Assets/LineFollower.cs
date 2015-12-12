using UnityEngine;
using System.Collections;

public class LineFollower : MonoBehaviour {
    public Utility.CatmullRomSpline spline;

    void Update()
    {
        float distance;
        Debug.DrawLine(transform.position, spline.GetClosestPoint(transform.position, out distance, 30));
        Debug.Log(distance);
    }
}
