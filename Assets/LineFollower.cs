using UnityEngine;
using System.Collections;

public class LineFollower : MonoBehaviour {
    public BezierCurve curve;

	// Use this for initialization
	void Start () {
        for (int i = 0; i < curve.length; i+=3)
        {
            var gObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gObj.transform.position = curve.GetPointAtDistance(i);
        }
	}

}
