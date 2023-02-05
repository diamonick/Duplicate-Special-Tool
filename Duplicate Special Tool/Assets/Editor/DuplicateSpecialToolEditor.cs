using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DuplicateSpecialToolEditor : EditorWindow
{
    private GUIStyle optionLabelStyle;

    // Property: Selected GameObject.
    private GameObject selectedGameObject;

    // Property: Number of copies.
    private int numOfCopies;
    private readonly string numOfCopiesTooltip = "Specify the number of copies to create from the selected GameObject." +
                                                 "The range is from 1 to 1000.";

    // Property: Translate (Offset).
    private Vector3 positionProp = Vector3.zero;
    private bool isDefaultPosition;
    private readonly string positionTooltip = "Specify the number of copies to create from the selected GameObject." +
                                                 "The range is from 1 to 1000.";

    // Property: Rotate (Offset).
    private Vector3 rotationProp = Vector3.zero;
    private bool isDefaultRotation;
    private readonly string rotationTooltip = "Specify the number of copies to create from the selected GameObject." +
                                                 "The range is from 1 to 1000.";

    // Property: Scale (Offset).
    private Vector3 scaleProp = Vector3.one;
    private bool isDefaultScale;
    private readonly string scaleTooltip = "Specify the number of copies to create from the selected GameObject." +
                                                 "The range is from 1 to 1000.";

    private readonly string duplicateSpecialTooltip = "Close the editor window.";
    private readonly string applyTooltip = "Apply change(s) without closing the editor window.";
    private readonly string closeTooltip = "Close the editor window.";

    private bool showPreview = false;
    private List<GameObject> temporaryDuplicatedGameObjects;

    #region Menu Item + Validation
    /// <summary>
    /// Display the Duplicate Special option in the right-click menu when a single (1) GameObject is selected.
    /// Keyboard Shortcut: ctrl-alt-D (Windows), cmd-alt-D (macOS).
    /// </summary>
    [MenuItem("GameObject/Duplicate Special", false, 10)]
    [MenuItem("Edit/Duplicate Special %&d", false, 120)]
    public static void DisplayWindow()
    {
        GetWindow<DuplicateSpecialToolEditor>("Duplicate Special");
    }

    /// <summary>
    /// Validation Function: Allow opening the Duplicate Special tool window only if one (1) gameObject is selected.
    /// </summary>
    /// <returns>TRUE if only one (1) gameObject is selected. Otherwise, FALSE.</returns>
    [MenuItem("GameObject/Duplicate Special", true)]
    private static bool DuplicateSpecialValidation() => Selection.gameObjects.Length == 1;
    #endregion

    private void OnGUI()
    {
        optionLabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            margin = new RectOffset(),
            padding = new RectOffset(),
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };

        GUIStyle closeButtonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fixedHeight = 32f,
        };

        #region Header

        GUILayout.Label("Duplicate Special Tool is a special tool which provides users various options to", EditorStyles.boldLabel);

        #endregion

        DrawLine(GetColorFromHexString("#34aeeb"), 1, 4f);

        #region Selected GameObject
        GUIContent selectedGameObjectContent = new GUIContent("Selected GameObject:", "The selected gameObject.");
        selectedGameObject = Selection.activeGameObject;
        GUI.backgroundColor = AddColor("#9bff54");
        GUI.enabled = false;
        GUILayout.Box(selectedGameObjectContent);
        EditorGUILayout.ObjectField(selectedGameObjectContent, selectedGameObject, typeof(GameObject), true);
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        #endregion

        #region Number of Copies
        GUIContent numOfCopiesContent = new GUIContent("Number of copies:", numOfCopiesTooltip);
        numOfCopies = EditorGUILayout.IntSlider(numOfCopiesContent, numOfCopies, 0, 1000);
        #endregion

        DrawLine(GetColorFromHexString("#34aeeb"), 1, 4f);

        #region Transform
        GUILayout.BeginHorizontal();
        GUIContent positionContent = new GUIContent("Translate (Offset):", positionTooltip);
        positionProp = EditorGUILayout.Vector3Field(positionContent, positionProp);
        isDefaultPosition = positionProp == Vector3.zero;

        GUI.backgroundColor = isDefaultPosition ? Color.white : Color.green;
        GUI.enabled = !isDefaultPosition;
        if (GUILayout.Button("↺"))
        {
            ResetTransformProperty(ref positionProp, Vector3.zero);
        }
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUIContent rotationContent = new GUIContent("Rotate (Offset):", rotationTooltip);
        rotationProp = EditorGUILayout.Vector3Field(rotationContent, rotationProp);
        isDefaultRotation = rotationProp == Vector3.zero;

        GUI.backgroundColor = isDefaultRotation ? Color.white : Color.green;
        GUI.enabled = !isDefaultRotation;
        if (GUILayout.Button("↺"))
        {
            ResetTransformProperty(ref rotationProp, Vector3.zero);
        }
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUIContent scaleContent = new GUIContent("Scale (Offset):", scaleTooltip);
        scaleProp = EditorGUILayout.Vector3Field(scaleContent, scaleProp);
        isDefaultScale = scaleProp == Vector3.one;

        GUI.backgroundColor = isDefaultScale ? Color.white : Color.green;
        GUI.enabled = !isDefaultScale;
        if (GUILayout.Button("↺"))
        {
            ResetTransformProperty(ref scaleProp, Vector3.one);
        }
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        #endregion

        DrawLine(GetColorFromHexString("#34aeeb"), 1, 4f);
        showPreview = GUILayout.Toggle(showPreview, "Show Preview");
        if (showPreview)
        {

        }

        #region Button(s) Footer
        GUILayout.FlexibleSpace();
        DrawLine(GetColorFromHexString("#34aeeb"), 1, 4f);
        GUILayout.BeginHorizontal();
        // [Duplicate Special] Button
        GUI.backgroundColor = AddColor("#00ffae");
        if (GUILayout.Button("Duplicate Special ★", closeButtonStyle))
        {

        }
        GUI.backgroundColor = Color.white;
        // [Apply] Button
        GUI.backgroundColor = AddColor("#00aeff");
        if (GUILayout.Button("Apply", closeButtonStyle))
        {

        }
        GUI.backgroundColor = Color.white;
        // [Close] Button
        GUI.backgroundColor = AddColor("#8030ff");
        if (GUILayout.Button("Close", closeButtonStyle))
        {
            // Close editor window.
            Close();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        #endregion


        //    EditorGUILayout.HelpBox($"Selected GameObject(s): {Selection.gameObjects.Length}", MessageType.Info);
    }

    #region Duplicate Special Tool Method(s)
    private void ResetTransformProperty(ref Vector3 v, Vector3 defaultValue)
    {
        v = defaultValue;
    }
    #endregion

    #region Miscellaneous
    /// <summary>
    /// Get color from hex string.
    /// </summary>
    /// <param name="hexColor">Hex color string.</param>
    /// <returns>New color.</returns>
    protected Color GetColorFromHexString(string hexColor)
    {
        Color color = Color.white;
        ColorUtility.TryParseHtmlString(hexColor, out color);
        return color;
    }

    /// <summary>
    /// Add color to existing color.
    /// </summary>
    /// <param name="color">Added color.</param>
    /// <returns>New color.</returns>
    protected Color AddColor(Color color)
    {
        color += color;
        return color;
    }

    /// <summary>
    /// Add color to existing color.
    /// </summary>
    /// <param name="hexColor">Hex color string.</param>
    /// <returns>New color.</returns>
    protected Color AddColor(string hexColor)
    {
        Color color = Color.white;
        ColorUtility.TryParseHtmlString(hexColor, out color);
        color += color;

        return color;
    }

    /// <summary>
    /// Draws a line in the inspector.
    /// </summary>
    /// <param name="lineColor">Line color.</param>
    /// <param name="height">Line height.</param>
    /// <param name="spacing">Spacing.</param>
    protected static void DrawLine(Color lineColor, int height, float spacing)
    {
        GUIStyle horizontalLine = new GUIStyle();
        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
        horizontalLine.margin = new RectOffset(4, 4, height, height);
        horizontalLine.fixedHeight = height;

        GUILayout.Space(spacing);

        var c = GUI.color;
        GUI.color = lineColor;
        GUILayout.Box(GUIContent.none, horizontalLine);
        GUI.color = c;

        GUILayout.Space(spacing);
    }
    #endregion

}
