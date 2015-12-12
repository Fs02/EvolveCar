using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Utility
{
    [CustomEditor(typeof(CatmullRomSpline))]
    public class CatmullRomSplineEditor : Editor
    {
        CatmullRomSpline spline;
        SerializedProperty resolutionProp;
        SerializedProperty closeProp;
        SerializedProperty pointsProp;
        SerializedProperty colorProp;

        private static bool showPoints = true;

        void OnEnable()
        {
            spline = (CatmullRomSpline)target;

            resolutionProp = serializedObject.FindProperty("resolution");
            closeProp = serializedObject.FindProperty("_close");
            pointsProp = serializedObject.FindProperty("points");
            colorProp = serializedObject.FindProperty("drawColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(resolutionProp);
            EditorGUILayout.PropertyField(closeProp);
            EditorGUILayout.PropertyField(colorProp);

            showPoints = EditorGUILayout.Foldout(showPoints, "Points");

            if (showPoints)
            {
                int pointCount = pointsProp.arraySize;

                for (int i = 0; i < pointCount; i++)
                {
                    DrawPointInspector(spline[i], i);
                }

                if (GUILayout.Button("Add Point"))
                {
                    GameObject pointObject = new GameObject("Point " + pointsProp.arraySize);
                    Undo.RecordObject(pointObject, "Add Point");
                    pointObject.transform.parent = spline.transform;
                    pointObject.transform.localPosition = Vector3.zero;
                    CatmullRomPoint newPoint = pointObject.AddComponent<CatmullRomPoint>();

                    newPoint.spline = spline;

                    pointsProp.InsertArrayElementAtIndex(pointsProp.arraySize);
                    pointsProp.GetArrayElementAtIndex(pointsProp.arraySize - 1).objectReferenceValue = newPoint;
                }
            }

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        void OnSceneGUI()
        {
            for (int i = 0; i < spline.pointCount; i++)
            {
                DrawPointSceneGUI(spline[i]);
            }
        }

        void DrawPointInspector(CatmullRomPoint point, int index)
        {
            SerializedObject serObj = new SerializedObject(point);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                Undo.RecordObject(point, "Remove Point");
                pointsProp.MoveArrayElement(spline.GetPointIndex(point), spline.pointCount - 1);
                pointsProp.arraySize--;
                DestroyImmediate(point.gameObject);
                return;
            }

            EditorGUILayout.ObjectField(point.gameObject, typeof(GameObject), true);

            if (index != 0 && GUILayout.Button(@"/\", GUILayout.Width(25)))
            {
                UnityEngine.Object other = pointsProp.GetArrayElementAtIndex(index - 1).objectReferenceValue;
                pointsProp.GetArrayElementAtIndex(index - 1).objectReferenceValue = point;
                pointsProp.GetArrayElementAtIndex(index).objectReferenceValue = other;
            }

            if (index != pointsProp.arraySize - 1 && GUILayout.Button(@"\/", GUILayout.Width(25)))
            {
                UnityEngine.Object other = pointsProp.GetArrayElementAtIndex(index + 1).objectReferenceValue;
                pointsProp.GetArrayElementAtIndex(index + 1).objectReferenceValue = point;
                pointsProp.GetArrayElementAtIndex(index).objectReferenceValue = other;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUI.indentLevel++;

            Vector3 newPointPos = EditorGUILayout.Vector3Field("Position : ", point.transform.localPosition);
            if (newPointPos != point.transform.localPosition)
            {
                Undo.RecordObject(point.transform, "Move Bezier Point");
                point.transform.localPosition = newPointPos;
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            if (GUI.changed)
            {
                serObj.ApplyModifiedProperties();
                EditorUtility.SetDirty(serObj.targetObject);
            }
        }

        static void DrawPointSceneGUI(CatmullRomPoint point)
        {
            Handles.Label(point.position + new Vector3(0, HandleUtility.GetHandleSize(point.position) * 0.4f, 0), point.gameObject.name);

            Handles.color = Color.green;
            Vector3 newPosition = Handles.FreeMoveHandle(point.position, point.transform.rotation, HandleUtility.GetHandleSize(point.position) * 0.1f, Vector3.zero, Handles.RectangleCap);

            if (newPosition != point.position)
            {
                Undo.RecordObject(point.transform, "Move Point");
                point.transform.position = newPosition;
            }
        }

        public static void DrawOtherPoints(CatmullRomSpline curve, CatmullRomPoint caller)
        {
            foreach (CatmullRomPoint p in curve.GetAnchorPoints())
            {
                if (p != caller) DrawPointSceneGUI(p);
            }
        }

        [MenuItem("GameObject/Create Other/CatmullRom Spline")]
        public static void CreateSpline(MenuCommand command)
        {
            GameObject curveObject = new GameObject("CatmullRomSpline");
            Undo.RecordObject(curveObject, "Undo Create Curve");
            CatmullRomSpline curve = curveObject.AddComponent<CatmullRomSpline>();

            curve.AddPointAt(Vector3.forward * 0.5f);
            curve.AddPointAt(Vector3.right * 0.5f);
            curve.AddPointAt(-Vector3.forward * 0.5f);
            curve.AddPointAt(-Vector3.right * 0.5f);

            curve.close = true;
        }
    }
}