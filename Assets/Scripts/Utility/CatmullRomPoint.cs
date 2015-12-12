using UnityEngine;
using System;
using System.Collections;

namespace Utility
{
    /// <summary>
    /// 	- Helper class for storing and manipulating Catmull-Rom data
    /// 	- Ensures that handles are in correct relation to one another
    /// 	- Handles adding/removing self from curve point lists
    /// 	- Calls SetDirty() on curve when edited 
    /// 	- Modified version of Bezier Curve : https://www.assetstore.unity3d.com/en/#!/content/11278
    /// </summary>
    [Serializable]
    public class CatmullRomPoint : MonoBehaviour
    {
        #region PublicProperties

        /// <summary>
        ///		- Curve this point belongs to
        /// 	- Changing this value will automatically remove this point from the current curve and add it to the new one
        /// </summary>
        [SerializeField]
        private CatmullRomSpline _spline;
        public CatmullRomSpline spline
        {
            get { return _spline; }
            set
            {
                if (_spline) _spline.RemovePoint(this);
                _spline = value;
                _spline.AddPoint(this);
            }
        }

        /// <summary>
        /// 	- Shortcut to transform.position
        /// </summary>
        /// <value>
        /// 	- The point's world position
        /// </value>
        public Vector3 position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        /// <summary>
        /// 	- Shortcut to transform.localPosition
        /// </summary>
        /// <value>
        /// 	- The point's local position.
        /// </value>
        public Vector3 localPosition
        {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
        }

        #endregion

        #region PrivateVariables

        /// <summary>
        /// 	- Used to determine if this point has moved since the last frame
        /// </summary>
        private Vector3 lastPosition;

        #endregion

        #region MonoBehaviourFunctions

        void Update()
        {
            if (!_spline.dirty && transform.position != lastPosition)
            {
                _spline.SetDirty();
                lastPosition = transform.position;
            }
        }

        #endregion
    }
}