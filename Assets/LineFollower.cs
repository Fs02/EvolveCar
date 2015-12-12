using UnityEngine;
using System.Collections;

public class LineFollower : MonoBehaviour {
    public Utility.CatmullRomSpline spline;
    public LineRenderer line;

    void Update()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, spline.GetClosestPoint(transform.position, 30));
    }
}
