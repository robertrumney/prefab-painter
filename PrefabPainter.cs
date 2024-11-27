using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrefabPainter : EditorWindow
{
    private readonly List<PrefabEntry> prefabEntries = new();
    private readonly string[] rotationAxes = new string[] { "X", "Y", "Z" };

    private Stack<GameObject> undoStack;
    private Vector3 lastPaintPosition = Vector3.positiveInfinity;
    private float paintSensitivity = 1.0f;

    private int rotationAxisIndex = 1;
    private float brushSize = 1.0f;
    private float prefabOffset = 0.5f;

    private bool randomRotation = false;
    private bool randomScale = false;
    private bool isPainting = false;

    private Vector2 scaleRange = new(1.0f, 1.0f);
    private Vector2 rotationRange = new(0, 360);

    [System.Serializable]
    public class PrefabEntry
    {
        public GameObject prefab;
        public float odds;
    }

    [MenuItem("Tools/Prefab Painter")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPainter>(false, "Prefab Painter");
    }

    private void OnEnable()
    {
        undoStack = new Stack<GameObject>();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Painting Settings", EditorStyles.boldLabel);

        if (GUILayout.Button("Add New Prefab Entry"))
        {
            prefabEntries.Add(new PrefabEntry());
        }

        for (int i = 0; i < prefabEntries.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            prefabEntries[i].prefab = EditorGUILayout.ObjectField("Prefab", prefabEntries[i].prefab, typeof(GameObject), false) as GameObject;
            prefabEntries[i].odds = EditorGUILayout.FloatField("Odds", prefabEntries[i].odds);

            if (GUILayout.Button("Remove"))
            {
                prefabEntries.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

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

        paintSensitivity = EditorGUILayout.Slider("Paint Sensitivity", paintSensitivity, 0.1f, 5.0f);

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

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPainting || prefabEntries.Count == 0) return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit) && e.type == EventType.MouseDrag && e.button == 0)
        {
            if (Vector3.Distance(lastPaintPosition, hit.point) > paintSensitivity)
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(hit.point, hit.normal, brushSize);
                PaintPrefab(hit.point + hit.normal * prefabOffset, hit.normal);
                lastPaintPosition = hit.point;
                e.Use();
            }
        }

        sceneView.Repaint();
    }

    private GameObject GetRandomPrefab()
    {
        float totalOdds = 0;
        foreach (var entry in prefabEntries)
        {
            totalOdds += entry.odds;
        }

        float randomPoint = Random.value * totalOdds;
        float currentOdds = 0;

        foreach (var entry in prefabEntries)
        {
            currentOdds += entry.odds;
            if (currentOdds >= randomPoint)
                return entry.prefab;
        }

        return null;
    }

    private void PaintPrefab(Vector3 position, Vector3 normal)
    {
        GameObject prefabToPaint = GetRandomPrefab();

        if (prefabToPaint == null)
            return;

        GameObject newPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPaint);
        newPrefab.transform.SetPositionAndRotation(position, GetRotation(normal));
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

            if (rotationAxisIndex == 0)
                randomRotationVector = new Vector3(randomAngle, 0, 0);
            else if (rotationAxisIndex == 1)
                randomRotationVector = new Vector3(0, randomAngle, 0);
            else
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
