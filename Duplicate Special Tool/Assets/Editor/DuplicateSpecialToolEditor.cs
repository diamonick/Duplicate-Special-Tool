using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DuplicateSpecialToolEditor : EditorWindow
{
    private GUIStyle optionLabelStyle;

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

        GUILayout.Label("Select a group or groups of objects and alter the selection(s) in various ways, " +
                        "such as transform, rotate, or flip (mirror)!", EditorStyles.largeLabel);

        GUIContent numOfCopiesContent = new GUIContent("Number of copies:", numOfCopiesTooltip);
        numOfCopies = EditorGUILayout.IntSlider(numOfCopiesContent, numOfCopies, 0, 1000);

        DrawLine(GetColorFromHexString("#34aeeb"), 1, 4f);

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


        //showTool = EditorGUILayout.Foldout(showTool, "Group Transform");

        //if (showTool)
        //{
        //    showRotateTool = EditorGUILayout.Foldout(showRotateTool, "Rotate");

        //    if (showRotateTool)
        //    {
        //        freeRotation = EditorGUILayout.Toggle("Free Rotation", freeRotation);
        //        if (freeRotation)
        //        {
        //            groupRotationAngle = EditorGUILayout.Slider("Group Rotation", Mathf.Round(groupRotationAngle), 0f, 360f);
        //            if (GUILayout.Button("Apply Rotation"))
        //            {
        //                RotateGroup(groupRotationAngle);
        //            }
        //        }
        //        else
        //        {
        //            GUILayout.BeginHorizontal();
        //            if (GUILayout.Button("5°")) { RotateGroup(5f); }
        //            if (GUILayout.Button("15°")) { RotateGroup(15f); }
        //            if (GUILayout.Button("30°")) { RotateGroup(30f); }
        //            GUILayout.EndHorizontal();

        //            GUILayout.BeginHorizontal();
        //            if (GUILayout.Button("45°")) { RotateGroup(45f); }
        //            if (GUILayout.Button("60°")) { RotateGroup(60f); }
        //            if (GUILayout.Button("90°")) { RotateGroup(90f); }
        //            GUILayout.EndHorizontal();

        //            GUILayout.BeginHorizontal();
        //            if (GUILayout.Button("120°")) { RotateGroup(120f); }
        //            if (GUILayout.Button("180°")) { RotateGroup(180f); }
        //            if (GUILayout.Button("270°")) { RotateGroup(270f); }
        //            GUILayout.EndHorizontal();
        //        }
        //        rotationDirection = (Direction)EditorGUILayout.EnumPopup("Direction", rotationDirection);
        //    }

        //    showFlipTool = EditorGUILayout.Foldout(showFlipTool, "Flip");
        //    if (showFlipTool)
        //    {
        //        if (GUILayout.Button("Flip Horizontal", GUILayout.Height(16)))
        //        {
        //            FlipGroup(Flip.Horizontal);
        //        }
        //        if (GUILayout.Button("Flip Vertical", GUILayout.Height(16)))
        //        {
        //            FlipGroup(Flip.Vertical);
        //        }
        //    }

        //    EditorGUILayout.HelpBox($"Selected GameObject(s): {Selection.gameObjects.Length}", MessageType.Info);
        //}
    }

    private void ResetTransformProperty(ref Vector3 v, Vector3 defaultValue)
    {
        v = defaultValue;
    }

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
