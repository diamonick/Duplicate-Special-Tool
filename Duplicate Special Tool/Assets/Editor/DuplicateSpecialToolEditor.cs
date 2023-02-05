using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DuplicateSpecialToolEditor : EditorWindow
{
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
        GUILayout.Label("Select a group or groups of objects and alter the selection(s) in various ways, " +
                        "such as transform, rotate, or flip (mirror)!", EditorStyles.largeLabel);

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

}
