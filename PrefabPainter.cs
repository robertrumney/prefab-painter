using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrefabPainter : EditorWindow
{
    private GameObject prefabToPaint;
    private Stack<GameObject> undoStack;
    private float brushSize = 1.0f;
    private float prefabOffset = 0.5f; // Adjusted for better default placement
    private bool randomRotation = false;
    private Vector2 rotationRange = new Vector2(0, 360);
    private bool randomScale = false;
    private Vector2 scaleRange = new Vector2(1.0f, 1.0f);
    private bool isPainting = false;
    private int rotationAxisIndex = 1; // Default to Y-axis
    private string[] rotationAxes = new string[] { "X", "Y", "Z" };

    [MenuItem("Tools/Prefab Painter")]
    public static void ShowWindow()
    {
        GetWindow(typeof(PrefabPainter), false, "Prefab Painter");
    }

    void OnEnable()
    {
        undoStack = new Stack<GameObject>();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        GUILayout.Label("Prefab Painting Settings", EditorStyles.boldLabel);

        prefabToPaint = EditorGUILayout.ObjectField("Prefab to Paint", prefabToPaint, typeof(GameObject), false) as GameObject;

        brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.1f, 10.0f);
        prefabOffset = EditorGUILayout.FloatField("Prefab Vertical Offset", prefabOffset);

        randomRotation = EditorGUILayout.Toggle("Random Rotation", randomRotation);
        if (randomRotation)
        {
            rotationAxisIndex = EditorGUILayout.Popup("Rotation Axis", rotationAxisIndex, rotationAxes);
            rotationRange = EditorGUILayout.Vector2Field("Rotation Range (Degrees)", rotationRange);
        }

        randomScale = EditorGUILayout.Toggle("Random Scale", randomScale);
        if (randomScale)
        {
            scaleRange = EditorGUILayout.Vector2Field("Scale Range", scaleRange);
        }

        if (GUILayout.Button(isPainting ? "Stop Painting" : "Start Painting"))
        {
            isPainting = !isPainting;
        }

        if (GUILayout.Button("Undo Last Paint"))
        {
            UndoLastPaint();
        }

        if (GUILayout.Button("Clear All Painted Prefabs"))
        {
            ClearAllPaintedPrefabs();
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!isPainting || prefabToPaint == null) return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(hit.point, hit.normal, brushSize);

            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                e.Use();
                PaintPrefab(hit.point + hit.normal * prefabOffset, hit.normal);
            }
        }

        sceneView.Repaint();
    }

    private void PaintPrefab(Vector3 position, Vector3 normal)
    {
        GameObject newPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPaint);
        newPrefab.transform.position = position;
        newPrefab.transform.rotation = GetRotation(normal);
        float scale = randomScale ? Random.Range(scaleRange.x, scaleRange.y) : 1.0f;
        newPrefab.transform.localScale = new Vector3(scale, scale, scale);
        Undo.RegisterCreatedObjectUndo(newPrefab, "Paint Prefab");
        undoStack.Push(newPrefab);
    }

    private Quaternion GetRotation(Vector3 normal)
    {
        Vector3 randomRotationVector = Vector3.zero;
        if (randomRotation)
        {
            float randomAngle = Random.Range(rotationRange.x, rotationRange.y);
            if (rotationAxisIndex == 0) // X-axis
                randomRotationVector = new Vector3(randomAngle, 0, 0);
            else if (rotationAxisIndex == 1) // Y-axis
                randomRotationVector = new Vector3(0, randomAngle, 0);
            else if (rotationAxisIndex == 2) // Z-axis
                randomRotationVector = new Vector3(0, 0, randomAngle);
        }

        return Quaternion.Euler(randomRotationVector) * Quaternion.FromToRotation(Vector3.up, normal);
    }

    private void UndoLastPaint()
    {
        if (undoStack.Count > 0)
        {
            GameObject lastPaintedPrefab = undoStack.Pop();
            if (lastPaintedPrefab != null)
                Undo.DestroyObjectImmediate(lastPaintedPrefab);
        }
    }

    private void ClearAllPaintedPrefabs()
    {
        while (undoStack.Count > 0)
        {
            GameObject prefab = undoStack.Pop();
            if (prefab != null)
                Undo.DestroyObjectImmediate(prefab);
        }
    }
}
