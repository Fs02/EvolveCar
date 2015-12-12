﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Utility
{
    /// <summary>
    /// 	- Class for describing and drawing Centripetal Catmull-rom Spline
    /// 	- Efficiently handles approximate length calculation through 'dirty' system
    /// 	- Has static functions for getting points on curves constructed by Vector3 parameters (GetPoint, GetCubicPoint, GetQuadraticPoint, and GetLinearPoint)
    /// </summary>
    [ExecuteInEditMode]
    [Serializable]
    public class CatmullRomSpline : MonoBehaviour
    {
        struct CubicPoly
        {
            public float c0, c1, c2, c3;

            public float eval(float t)
            {
                float t2 = t * t;
                float t3 = t2 * t;
                return c0 + c1 * t + c2 * t2 + c3 * t3;
            }
        };

        #region PublicVariables

        /// <summary>
        ///  	- the number of mid-points calculated for each pair of bezier points
        ///  	- used for drawing the curve in the editor
        ///  	- used for calculating the "length" variable
        /// </summary>
        public int resolution = 30;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BezierCurve"/> is dirty.
        /// </summary>
        /// <value>
        /// <c>true</c> if dirty; otherwise, <c>false</c>.
        /// </value>
        public bool dirty { get; private set; }

        /// <summary>
        /// 	- color this curve will be drawn with in the editor
        ///		- set in the editor
        /// </summary>
        public Color drawColor = Color.white;

        #endregion

        #region PublicProperties

        /// <summary>
        ///		- set in the editor
        /// 	- used to determine if the curve should be drawn as "closed" in the editor
        /// 	- used to determine if the curve's length should include the curve between the first and the last points in "points" array
        /// 	- setting this value will cause the curve to become dirty
        /// </summary>
        [SerializeField]
        private bool _close;
        public bool close
        {
            get { return _close; }
            set
            {
                if (_close == value) return;
                _close = value;
                dirty = true;
            }
        }

        /// <summary>
        ///		- set internally
        ///		- gets point corresponding to "index" in "points" array
        ///		- does not allow direct set
        /// </summary>
        /// <param name='index'>
        /// 	- the index
        /// </param>
        public CatmullRomPoint this[int index]
        {
            get { return points[index]; }
        }

        /// <summary>
        /// 	- number of points stored in 'points' variable
        ///		- set internally
        ///		- does not include "handles"
        /// </summary>
        /// <value>
        /// 	- The point count
        /// </value>
        public int pointCount
        {
            get { return points.Length; }
        }

        /// <summary>
        /// 	- The approximate length of the curve
        /// 	- recalculates if the curve is "dirty"
        /// </summary>
        private float _length;
        public float length
        {
            get
            {
                if (dirty)
                {
                    _length = 0;
                    for (int i = 0; i < points.Length - 3; i++)
                    {
                        _length += ApproximateLength(points[i], points[i + 1], points[i + 2], points[i + 3], resolution);
                    }

                    if (close)
                    {
                        _length += ApproximateLength(points[points.Length - 1], points[0], points[1], points[2], resolution);
                        _length += ApproximateLength(points[points.Length - 2], points[points.Length - 1], points[0], points[1], resolution);
                        _length += ApproximateLength(points[points.Length - 3], points[points.Length - 2], points[points.Length - 1], points[0], resolution);
                    }

                    dirty = false;
                }

                return _length;
            }
        }

        #endregion

        #region PrivateVariables

        /// <summary> 
        /// 	- Array of point objects that make up this curve
        ///		- Populated through editor
        /// </summary>
        [SerializeField]
        public CatmullRomPoint[] points = new CatmullRomPoint[0];

        #endregion

        #region UnityFunctions

        void OnDrawGizmos()
        {
            Gizmos.color = drawColor;

            if (points.Length > 3)
            {
                for (int i = 0; i < points.Length - 3; i++)
                {
                    DrawSpline(points[i], points[i + 1], points[i + 2], points[i + 3], resolution);
                }

                if (close)
                {
                    DrawSpline(points[points.Length - 1], points[0], points[1], points[2], resolution);
                    DrawSpline(points[points.Length - 2], points[points.Length - 1], points[0], points[1], resolution);
                    DrawSpline(points[points.Length - 3], points[points.Length - 2], points[points.Length - 1], points[0], resolution);
                }
            }
        }

        void Awake()
        {
            dirty = true;
        }

        #endregion

        #region PublicFunctions

        /// <summary>
        /// 	- Adds the given point to the end of the curve ("points" array)
        /// </summary>
        /// <param name='point'>
        /// 	- The point to add.
        /// </param>
        public void AddPoint(CatmullRomPoint point)
        {
            List<CatmullRomPoint> tempArray = new List<CatmullRomPoint>(points);
            tempArray.Add(point);
            points = tempArray.ToArray();
            dirty = true;
        }

        /// <summary>
        /// 	- Adds a point at position
        /// </summary>
        /// <returns>
        /// 	- The point object
        /// </returns>
        /// <param name='position'>
        /// 	- Where to add the point
        /// </param>
        public CatmullRomPoint AddPointAt(Vector3 position)
        {
            GameObject pointObject = new GameObject("Point " + pointCount);

            pointObject.transform.parent = transform;
            pointObject.transform.position = position;

            CatmullRomPoint newPoint = pointObject.AddComponent<CatmullRomPoint>();
            newPoint.spline = this;

            return newPoint;
        }

        /// <summary>
        /// 	- Removes the given point from the curve ("points" array)
        /// </summary>
        /// <param name='point'>
        /// 	- The point to remove
        /// </param>
        public void RemovePoint(CatmullRomPoint point)
        {
            List<CatmullRomPoint> tempArray = new List<CatmullRomPoint>(points);
            tempArray.Remove(point);
            points = tempArray.ToArray();
            dirty = false;
        }

        /// <summary>
        /// 	- Gets a copy of the bezier point array used to define this curve
        /// </summary>
        /// <returns>
        /// 	- The cloned array of points
        /// </returns>
        public CatmullRomPoint[] GetAnchorPoints()
        {
            return (CatmullRomPoint[])points.Clone();
        }

        /// <summary>
        /// 	- Gets the point at 't' percent along this curve
        /// </summary>
        /// <returns>
        /// 	- Returns the point at 't' percent
        /// </returns>
        /// <param name='t'>
        /// 	- Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
        /// </param>
        ///
        public Vector3 GetPointAt(float t)
        {
            if (t <= 0) return points[0].position;
            else if (t >= 1) return points[points.Length - 1].position;

            float totalPercent = 0;
            float curvePercent = 0;

            CatmullRomPoint p0 = null;
            CatmullRomPoint p1 = null;
            CatmullRomPoint p2 = null;
            CatmullRomPoint p3 = null;

            for (int i = 0; i < points.Length - 3; i++)
            {
                curvePercent = ApproximateLength(points[i], points[i + 1], points[i + 2], points[i + 3], resolution) / length;
                if (totalPercent + curvePercent > t)
                {
                    p0 = points[i];
                    p1 = points[i + 1];
                    p2 = points[i + 2];
                    p3 = points[i + 3];
                    break;
                }

                else totalPercent += curvePercent;
            }

            if (close && p1 == null)
            {
                p1 = points[points.Length - 1];
                p2 = points[0];
            }

            t -= totalPercent;

            return GetPoint(p0, p1, p2, p3, t / curvePercent);
        }

        /// <summary>
        /// 	- Get the index of the given point in this curve
        /// </summary>
        /// <returns>
        /// 	- The index, or -1 if the point is not found
        /// </returns>
        /// <param name='point'>
        /// 	- Point to search for
        /// </param>
        public int GetPointIndex(CatmullRomPoint point)
        {
            int result = -1;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] == point)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 	- Sets this curve to 'dirty'
        /// 	- Forces the curve to recalculate its length
        /// </summary>
        public void SetDirty()
        {
            dirty = true;
        }

        #endregion

        #region PublicStaticFunctions

        /// <summary>
        /// 	- Draws the curve in the Editor
        /// </summary>
        /// <param name='p1'>
        /// 	- The bezier point at the beginning of the curve
        /// </param>
        /// <param name='p2'>
        /// 	- The bezier point at the end of the curve
        /// </param>
        /// <param name='resolution'>
        /// 	- The number of segments along the curve to draw
        /// </param>
        public static void DrawSpline(CatmullRomPoint p0, CatmullRomPoint p1, CatmullRomPoint p2, CatmullRomPoint p3, int resolution)
        {
            int limit = resolution + 1;
            float _res = resolution;
            Vector3 lastPoint = p1.position;
            Vector3 currentPoint = Vector3.zero;

            for (int i = 1; i < limit; i++)
            {
                currentPoint = GetPoint(p0, p1, p2, p3, i / _res);
                if (i % 2 == 0) Gizmos.color = Color.blue;
                else Gizmos.color = Color.green;
                Gizmos.DrawLine(lastPoint, currentPoint);
                lastPoint = currentPoint;
            }
        }

        /// <summary>
        /// 	- Gets the point 't' percent along a curve
        /// 	- Automatically calculates for the number of relevant points
        /// </summary>
        /// <returns>
        /// 	- The point 't' percent along the curve
        /// </returns>
        /// <param name='p1'>
        /// 	- The bezier point at the beginning of the curve
        /// </param>
        /// <param name='p2'>
        /// 	- The bezier point at the end of the curve
        /// </param>
        /// <param name='t'>
        /// 	- Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
        /// </param>
        public static Vector3 GetPoint(CatmullRomPoint p0, CatmullRomPoint p1, CatmullRomPoint p2, CatmullRomPoint p3, float t)
        {
            CubicPoly px = new CubicPoly();
            CubicPoly py = new CubicPoly();
            CubicPoly pz = new CubicPoly();
            InitCentripetalCR(p0.position, p1.position, p2.position, p3.position, out px, out py, out pz);
            return new Vector3(px.eval(t), py.eval(t), pz.eval(t));
        }

        /// <summary>
        /// 	- Gets point 't' percent along a linear "curve" (line)
        /// 	- This is exactly equivalent to Vector3.Lerp
        /// </summary>
        /// <returns>
        ///		- The point 't' percent along the curve
        /// </returns>
        /// <param name='p1'>
        /// 	- The point at the beginning of the line
        /// </param>
        /// <param name='p2'>
        /// 	- The point at the end of the line
        /// </param>
        /// <param name='t'>
        /// 	- Value between 0 and 1 representing the percent along the line (0 = 0%, 1 = 100%)
        /// </param>
        public static Vector3 GetLinearPoint(Vector3 p1, Vector3 p2, float t)
        {
            return p1 + ((p2 - p1) * t);
        }

        /// <summary>
        /// 	- Gets point 't' percent along n-order curve
        /// </summary>
        /// <returns>
        /// 	- The point 't' percent along the curve
        /// </returns>
        /// <param name='t'>
        /// 	- Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
        /// </param>
        /// <param name='points'>
        /// 	- The points used to define the curve
        /// </param>
        /// 
        /*
        public static Vector3 GetPoint(float t, params Vector3[] points)
        {
            t = Mathf.Clamp01(t);

            int order = points.Length - 1;
            Vector3 point = Vector3.zero;
            Vector3 vectorToAdd;

            for (int i = 0; i < points.Length; i++)
            {
                vectorToAdd = points[points.Length - i - 1] * (BinomialCoefficient(i, order) * Mathf.Pow(t, order - i) * Mathf.Pow((1 - t), i));
                point += vectorToAdd;
            }

            return point;
        }
        */

        /// <summary>
        /// 	- Approximates the length
        /// </summary>
        /// <returns>
        /// 	- The approximate length
        /// </returns>
        /// <param name='p1'>
        /// 	- The bezier point at the start of the curve
        /// </param>
        /// <param name='p2'>
        /// 	- The bezier point at the end of the curve
        /// </param>
        /// <param name='resolution'>
        /// 	- The number of points along the curve used to create measurable segments
        /// </param>
        /// 
        public static float ApproximateLength(CatmullRomPoint p0, CatmullRomPoint p1, CatmullRomPoint p2, CatmullRomPoint p3, int resolution = 10)
        {
            float _res = resolution;
            float total = 0;
            Vector3 lastPosition = p1.position;
            Vector3 currentPosition;

            for (int i = 0; i < resolution + 1; i++)
            {
                currentPosition = GetPoint(p0, p1, p2, p3, i / _res);
                total += (currentPosition - lastPosition).magnitude;
                lastPosition = currentPosition;
            }

            return total;
        }
        #endregion

        /*
        * Compute coefficients for a cubic polynomial
        *   p(s) = c0 + c1*s + c2*s^2 + c3*s^3
        * such that
        *   p(0) = x0, p(1) = x1
        *  and
        *   p'(0) = t0, p'(1) = t1.
        */
        static void InitCubicPoly(float x0, float x1, float t0, float t1, out CubicPoly p)
        {
            p.c0 = x0;
            p.c1 = t0;
            p.c2 = -3*x0 + 3*x1 - 2*t0 - t1;
            p.c3 = 2*x0 - 2*x1 + t0 + t1;
        }

        // compute coefficients for a nonuniform Catmull-Rom spline
        static void InitNonuniformCatmullRom(float x0, float x1, float x2, float x3, float dt0, float dt1, float dt2, out CubicPoly p)
        {
            // compute tangents when parameterized in [t1,t2]
            float t1 = (x1 - x0) / dt0 - (x2 - x0) / (dt0 + dt1) + (x2 - x1) / dt1;
            float t2 = (x2 - x1) / dt1 - (x3 - x1) / (dt1 + dt2) + (x3 - x2) / dt2;

            // rescale tangents for parametrization in [0,1]
            t1 *= dt1;
            t2 *= dt1;

            InitCubicPoly(x1, x2, t1, t2, out p);
        }

        static void InitCentripetalCR(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, out CubicPoly px, out CubicPoly py, out CubicPoly pz)
        {
            float dt0 = Mathf.Pow(Vector3.SqrMagnitude(p1 - p0), 0.25f);
            float dt1 = Mathf.Pow(Vector3.SqrMagnitude(p2 - p1), 0.25f);
            float dt2 = Mathf.Pow(Vector3.SqrMagnitude(p3 - p2), 0.25f);

            // safety check for repeated points
            if (dt1 < 1e-4f)    dt1 = 1.0f;
            if (dt0 < 1e-4f)    dt0 = dt1;
            if (dt2 < 1e-4f)    dt2 = dt1;

            InitNonuniformCatmullRom(p0.x, p1.x, p2.x, p3.x, dt0, dt1, dt2, out px);
            InitNonuniformCatmullRom(p0.y, p1.y, p2.y, p3.y, dt0, dt1, dt2, out py);
            InitNonuniformCatmullRom(p0.z, p1.z, p2.z, p3.z, dt0, dt1, dt2, out pz);
        }
    }
}