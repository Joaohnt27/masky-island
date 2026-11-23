using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomObstacleMesh))]
public class CustomObstacleMeshEditor : Editor
{
    private void OnSceneGUI()
    {
        CustomObstacleMesh obstacle =
            (CustomObstacleMesh)target;

        Vector2[] points = obstacle.Points;

        if (points == null)
            return;

        Handles.color = Color.yellow;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 worldPosition =
                obstacle.transform.TransformPoint(
                    new Vector3(
                        points[i].x,
                        0,
                        points[i].y
                    )
                );

            EditorGUI.BeginChangeCheck();

            Vector3 newWorldPosition =
                Handles.PositionHandle(
                    worldPosition,
                    Quaternion.identity
                );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(
                    obstacle,
                    "Mover ponto do collider"
                );

                Vector3 localPosition =
                    obstacle.transform.InverseTransformPoint(
                        newWorldPosition
                    );

                points[i] = new Vector2(
                    localPosition.x,
                    localPosition.z
                );

                obstacle.GenerateMesh();

                EditorUtility.SetDirty(obstacle);
            }

            Vector3 nextWorldPosition =
                obstacle.transform.TransformPoint(
                    new Vector3(
                        points[(i + 1) % points.Length].x,
                        0,
                        points[(i + 1) % points.Length].y
                    )
                );

            Handles.DrawLine(
                worldPosition,
                nextWorldPosition
            );

            Handles.Label(
                worldPosition,
                $" {i}"
            );
        }
    }
}