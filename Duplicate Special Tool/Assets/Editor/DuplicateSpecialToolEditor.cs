using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

public class DuplicateSpecialToolEditor : EditorWindow
{
    public enum TransformMode
    {
        TwoDimensional = 0,
        ThreeDimensional = 1
    }

    private readonly string[] transformModes = new string[] { "2D", "3D", "Line", "Grid", "Circle", "Random" };

    private GUIStyle optionLabelStyle;
    private readonly string boxHeaderColor = "#4b535e";

    private Vector2 scrollPosition;

    // Icons
    private readonly string numberOfCopiesIconPath = "Assets/Editor/Icons/NumberOfCopiesIcon.png";
    private readonly string namingConventionsIconPath = "Assets/Editor/Icons/NamingConventionIcon.png";
    private readonly string groupUnderIconPath = "Assets/Editor/Icons/GroupUnderIcon.png";
    private readonly string transformIconPath = "Assets/Editor/Icons/TransformIcon.png";

    #region Number of Copies
    // Property: Selected GameObject.
    private GameObject selectedGameObject;

    // Property: Number of copies.
    private int numOfCopies;
    private readonly string numOfCopiesTooltip = "Specify the number of copies to create from the selected GameObject.\n\n" +
                                                 "The range is from 1 to 1000.";

    // Section Fields
    private bool showNumberOfCopiesSection = false;
    private int namingMethodType = 0;
    #endregion

    #region Naming Conventions
    private int numOfDigits = 1;
    private int countFromNumbers = 0;
    private int incrementByNumbers = 0;

    // Section Fields
    private bool showNamingConventionsSection = false;
    #endregion

    #region Group Under
    private Object groupParent = null;
    private Object groupRelative = null;
    private string newGroupName = "New Group";
    private readonly string[] groupUnderTypeTooltips = new string[]
    {
        "Groups the duplicate(s) next to the selected GameObject.",
        "Groups the duplicate(s) under the selected GameObject.",
        "Groups the duplicate(s) under the specified parent.",
        "Groups the duplicate(s) in the world.",
        "Creates a new group GameObject to group the duplicate(s) under."
    };

    // Section Fields
    private bool showGroupUnderSection = false;
    private int groupUnderType = 0;
    #endregion

    #region Transform
    private int transformMode;

    // Property: Translate (Offset).
    private Vector3 positionProp = Vector3.zero;
    private bool isDefaultPosition;
    private readonly string positionTooltip = "Specify the number of copies to create from the selected GameObject." +
                                                 "The range is from 1 to 1000.";
    private readonly string resetPositionTooltip = "Reset postion values to their default values.\n\n" +
                                                   "Default position is (X: 0.0, Y: 0.0, Z: 0.0).";

    // Property: Rotate (Offset).
    private Vector3 rotationProp = Vector3.zero;
    private bool isDefaultRotation;
    private readonly string rotationTooltip = "Specify the number of copies to create from the selected GameObject." +
                                                 "The range is from 1 to 1000.";
    private readonly string resetRotationTooltip = "Reset rotation values to their default values.\n\n" +
                                                   "Default rotation is (X: 0.0, Y: 0.0, Z: 0.0).";

    // Property: Scale (Offset).
    private Vector3 scaleProp = Vector3.one;
    private bool isDefaultScale;
    private readonly string scaleTooltip = "Specify the number of copies to create from the selected GameObject." +
                                                 "The range is from 1 to 1000.";
    private readonly string resetScaleTooltip = "Reset scale values to their default values.\n\n" +
                                                "Default scale is (X: 1.0, Y: 1.0, Z: 1.0).";

    // Section Fields
    private bool showTransformSection = false;
    #endregion

    #region Button(s) Footer
    private readonly string duplicateSpecialTooltip = "Close the editor window.";
    private readonly string applyTooltip = "Apply change(s) without closing the editor window.";
    private readonly string closeTooltip = "Close the editor window.";
    #endregion

    #region Other
    private bool showPreview = false;
    private readonly string showPreviewTooltip = "Shows a preview of how the duplicated object(s) will be arranged " +
                                                 "before duplicating.";

    private bool makePrefabClones = false;
    private readonly string makePrefabClonesTooltip = "When enabled, it will instantiate clone(s) of the prefab.\n\n" +
                                                      "If you want to preserve the prefab connection to the selected prefab, " +
                                                      "uncheck this checkbox.";
    private bool isPrefab = false;
    private List<GameObject> temporaryDuplicatedGameObjects;

    // Section Fields
    private bool showOtherSection = false;
    #endregion

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

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);
        #region Header

        GUILayout.Label("Duplicate Special Tool is a special tool which provides users various options to", EditorStyles.boldLabel);
        GUILayout.Space(120f);
        #endregion

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        #region Number of Copies
        selectedGameObject = Selection.activeGameObject;
        isPrefab = PrefabUtility.GetPrefabInstanceHandle(selectedGameObject) != null;
        DrawSection("No. of Copies", ref showNumberOfCopiesSection, DisplayNumberOfCopiesSection, numberOfCopiesIconPath, AddColor("#00E6BC"));
        #endregion

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        #region Naming Conventions
        DrawSection("Naming Conventions", ref showNamingConventionsSection, DisplayNamingConventionsSection, namingConventionsIconPath, AddColor("#00BEFF"));
        #endregion

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        #region Group Under
        DrawSection("Group Under", ref showGroupUnderSection, DisplayGroupUnderSection, groupUnderIconPath, AddColor("#B282FF"));
        #endregion

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        #region Transform
        DrawSection("Transform", ref showTransformSection, DisplayTransformSection, transformIconPath, AddColor("#FD6D40"));
        #endregion

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        #region Other
        DrawSection("Other", ref showOtherSection, null, "", AddColor(boxHeaderColor));
        GUIContent showPreviewContent = new GUIContent("Show Preview", showPreviewTooltip);
        showPreview = GUILayout.Toggle(showPreview, showPreviewContent);
        if (showPreview)
        {

        }
        GUI.enabled = isPrefab;
        GUIContent makePrefabClonesContent = new GUIContent("Make Prefab Clone(s)", makePrefabClonesTooltip);
        makePrefabClones = GUILayout.Toggle(makePrefabClones, makePrefabClonesContent);
        if (makePrefabClones)
        {

        }
        GUI.enabled = true;
        #endregion

        #region Button(s) Footer
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndScrollView();

        // Display warning.
        if (selectedGameObject == null)
        {
            GUI.backgroundColor = AddColor("#ffb300");
            GUI.contentColor = AddColor("#ffb300");
            EditorGUILayout.HelpBox("No gameObject is currently selected. Please select a gameObject " +
                                    "to continue the duplication process.", MessageType.Warning);
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.white;
        }

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        //This is the important bit, we set the width to the calculated width of the content in the GUIStyle of the control
        //EditorGUILayout.LabelField(label, GUILayout.Width(GUI.skin.label.CalcSize(label).x));

        GUILayout.BeginHorizontal();
        GUI.enabled = selectedGameObject != null;
        // [Duplicate Special] Button
        GUI.backgroundColor = GUI.enabled ? AddColor("#00ffae") : Color.gray;
        GUIContent duplicateSpecialButtonContent = new GUIContent("Duplicate Special", duplicateSpecialTooltip);
        if (GUILayout.Button(duplicateSpecialButtonContent, closeButtonStyle))
        {
            DuplicateObjects();
        }
        GUI.backgroundColor = Color.white;
        // [Apply] Button
        GUI.backgroundColor = GUI.enabled ? AddColor("#00aeff") : Color.gray;
        GUIContent applyButtonContent = new GUIContent("Apply", applyTooltip);
        if (GUILayout.Button(applyButtonContent, closeButtonStyle))
        {
            DuplicateObjects();
        }
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
        // [Close] Button
        GUI.backgroundColor = GUI.enabled ? AddColor("#8030ff") : Color.gray;
        GUIContent closeButtonContent = new GUIContent("Close", closeTooltip);
        if (GUILayout.Button(closeButtonContent, closeButtonStyle))
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

    private void DuplicateObjects()
    {
        if (selectedGameObject == null)
            return;

        GameObject prefab = null;
        if (isPrefab)
        {
            Debug.Log("Prefab!");

            //// Get the Diamond component from the selected object.
            //Diamond diamond = obj.GetComponent<Diamond>();
            //if (diamond == null)
            //    continue;
            //if (diamond.DiamondColor == Diamond.DiamondColorType.None)
            //    continue;

            //// Add selected Diamond to list.
            //selectedDiamonds.AddItem(obj);

            //// Instantiate new Diamonds based on the desired color.
            //Object prefabInstance = GetDiamondPrefab(diamond);
            //prefabInstance = PrefabUtility.InstantiatePrefab(prefabInstance as GameObject);
            //GameObject newDiamond = (GameObject)prefabInstance;

            //// Set position of new Diamond to the old position.
            //Vector3 positionRef = diamond.transform.position;
            //newDiamond.transform.position = positionRef;

            //// Format new Diamond's name.
            //newDiamond.name = $"{newDiamond.name} {selectedDiamonds.Count}";

            //// Place new diamond under the diamond group parent in the Hierarchy.
            //if (newDiamond != null)
            //{
            //    newDiamond.transform.SetParent(diamondGroupParent);
            //}
        }

        for (int i = 0; i < numOfCopies; i++)
        {    
        }

        string windowTitle = "Hello World!";
        string windowMessage = "Hello World!";
        if (EditorUtility.DisplayDialog(windowTitle, windowMessage, "OK"))
        {

        }
    }
    #endregion

    #region Sections
    /// <summary>
    /// Display the "Number of Copies" section.
    /// </summary>
    protected void DisplayNumberOfCopiesSection()
    {
        GUIContent selectedGameObjectContent = new GUIContent("Selected GameObject:", "The selected gameObject.");
        GUI.enabled = false;
        EditorGUILayout.ObjectField(selectedGameObjectContent, selectedGameObject, typeof(GameObject), true);
        GUI.enabled = true;
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;
        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        DrawBulletPoint("#00E6BC");
        GUIContent numOfCopiesContent = new GUIContent("Number of copies:", numOfCopiesTooltip);
        numOfCopies = EditorGUILayout.IntSlider(numOfCopiesContent, numOfCopies, 0, 1000);
        if (GUILayout.Button("-"))
        {
            numOfCopies = Mathf.Clamp(numOfCopies - 1, 0, 1000);
        }
        if (GUILayout.Button("+"))
        {
            numOfCopies = Mathf.Clamp(numOfCopies + 1, 0, 1000);
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Display the "Naming Conventions" section.
    /// </summary>
    protected void DisplayNamingConventionsSection()
    {
        var namingMethods = new string[] { "Numbers", "Letters", "Custom" };
        GUIContent namingMethodContent = new GUIContent("Naming Method:", "");
        namingMethodType = EditorGUILayout.Popup(namingMethodContent, namingMethodType, namingMethods);
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;
        GUILayout.Space(8);

        if (namingMethodType == 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUIContent numOfDigitsContent = new GUIContent("Number of digits:", "");
            DrawBulletPoint("#00BEFF");
            numOfDigits = EditorGUILayout.IntSlider(numOfDigitsContent, numOfDigits, 1, 10);
            if (GUILayout.Button("-"))
            {
                numOfDigits = Mathf.Clamp(numOfDigits - 1, 1, 10);
            }
            if (GUILayout.Button("+"))
            {
                numOfDigits = Mathf.Clamp(numOfDigits + 1, 1, 10);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            DrawBulletPoint("#00BEFF");
            GUIContent countFromContent = new GUIContent("Count from:", "");
            countFromNumbers = EditorGUILayout.IntSlider(countFromContent, countFromNumbers, 0, 100);
            if (GUILayout.Button("-"))
            {
                countFromNumbers = Mathf.Clamp(countFromNumbers - 1, 0, 100);
            }
            if (GUILayout.Button("+"))
            {
                countFromNumbers = Mathf.Clamp(countFromNumbers + 1, 0, 100);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            DrawBulletPoint("#00BEFF");
            GUIContent incrementByContent = new GUIContent("Increment by:", "");
            incrementByNumbers = EditorGUILayout.IntSlider(incrementByContent, incrementByNumbers, 1, 100);
            if (GUILayout.Button("-"))
            {
                incrementByNumbers = Mathf.Clamp(incrementByNumbers - 1, 1, 100);
            }
            if (GUILayout.Button("+"))
            {
                incrementByNumbers = Mathf.Clamp(incrementByNumbers + 1, 1, 100);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {

        }

        GUILayout.Space(8);

        GUIStyle headerStyle = new GUIStyle(GUI.skin.box)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 10,
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = 24f,
            stretchWidth = true,
            wordWrap = false,
            clipping = TextClipping.Clip
        };

        string name = $"Temp";
        GUILayout.Box($"Name Template: {name}", headerStyle);
    }

    /// <summary>
    /// Display the "Group Under" section.
    /// </summary>
    protected void DisplayGroupUnderSection()
    {
        var text = new string[] { "None", "This", "Parent", "World", "New Group" };
        string groupUnderTooltip = string.Empty;
        string groupName = "N/A";

        GUI.backgroundColor = Color.white;
        GUI.contentColor = Color.white;
        GUIContent groupUnderContent = new GUIContent("Group Duplicate(s) Under:", "Select which grouping method to group the duplicate(s) under.");
        groupUnderType = EditorGUILayout.Popup(groupUnderContent, groupUnderType, text);
        GUILayout.Space(8);

        if (groupUnderType == 1)
        {
            groupName = selectedGameObject != null ? selectedGameObject.name : groupName;
        }
        if (groupUnderType == 2)
        {
            GUILayout.BeginHorizontal();
            GUIContent groupParentContent = new GUIContent("Group Parent:", "All duplicate(s) will be grouped under the group parent.");
            DrawBulletPoint("#B282FF");
            groupParent = EditorGUILayout.ObjectField(groupParentContent, groupParent, typeof(GameObject), true);
            groupName = groupParent != null ? groupParent.name : groupName;
            GUILayout.EndHorizontal();
        }
        else if (groupUnderType == 4)
        {
            GUILayout.BeginHorizontal();
            GUIContent newGroupContent = new GUIContent("Group Name:", "The name of the new group.");
            DrawBulletPoint("#B282FF");
            newGroupName = EditorGUILayout.TextField(newGroupContent, newGroupName);
            groupName = newGroupName != string.Empty ? newGroupName : groupName;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawBulletPoint("#B282FF");
            groupRelative = EditorGUILayout.ObjectField("Group This Under:", groupRelative, typeof(GameObject), true);

            if (newGroupName == string.Empty)
            {
                GUI.backgroundColor = AddColor("#ffb300");
                GUI.contentColor = AddColor("#ffb300");
                EditorGUILayout.HelpBox("The group name cannot be empty. Please fill this field with a name.", MessageType.Warning);
                GUI.contentColor = Color.white;
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
        }
        groupUnderTooltip = groupUnderTypeTooltips[groupUnderType];
        groupUnderTooltip += $"\nGroup Name: {groupName}";
        EditorGUILayout.HelpBox(groupUnderTooltip, MessageType.Info);
        GUI.backgroundColor = Color.white;
    }

    /// <summary>
    /// Display the "Transform" section.
    /// </summary>
    protected void DisplayTransformSection()
    {
        transformMode = EditorGUILayout.Popup("Mode:", transformMode, transformModes);
        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        DrawBulletPoint("#FD6D40");
        GUIContent positionContent = new GUIContent("Translate (Offset):", positionTooltip);
        GUIContent resetPositionContent = new GUIContent("↺", resetPositionTooltip);
        positionProp = EditorGUILayout.Vector3Field(positionContent, positionProp);
        isDefaultPosition = positionProp == Vector3.zero;

        GUI.backgroundColor = isDefaultPosition ? Color.white : Color.green;
        GUI.enabled = !isDefaultPosition;
        if (GUILayout.Button(resetPositionContent))
        {
            ResetTransformProperty(ref positionProp, Vector3.zero);
        }
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        DrawBulletPoint("#FD6D40");
        GUIContent rotationContent = new GUIContent("Rotate (Offset):", rotationTooltip);
        GUIContent resetRotationContent = new GUIContent("↺", resetRotationTooltip);
        rotationProp = EditorGUILayout.Vector3Field(rotationContent, rotationProp);
        isDefaultRotation = rotationProp == Vector3.zero;

        GUI.backgroundColor = isDefaultRotation ? Color.white : Color.green;
        GUI.enabled = !isDefaultRotation;
        if (GUILayout.Button(resetRotationContent))
        {
            ResetTransformProperty(ref rotationProp, Vector3.zero);
        }
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        DrawBulletPoint("#FD6D40");
        GUIContent scaleContent = new GUIContent("Scale (Offset):", scaleTooltip);
        GUIContent resetScaleContent = new GUIContent("↺", resetScaleTooltip);
        scaleProp = EditorGUILayout.Vector3Field(scaleContent, scaleProp);
        isDefaultScale = scaleProp == Vector3.one;

        GUI.backgroundColor = isDefaultScale ? Color.white : Color.green;
        GUI.enabled = !isDefaultScale;
        if (GUILayout.Button(resetScaleContent))
        {
            ResetTransformProperty(ref scaleProp, Vector3.one);
        }
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
    }
    #endregion

    #region Draw Window Element Method(s)
    protected void DrawSection(string header, ref bool foldout, UnityAction ue, string iconPath, Color boxColor)
    {
        var icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D)) as Texture2D;
        EditorGUIUtility.SetIconSize(new Vector2(28f, 28f));

        GUIContent content = new GUIContent(header, icon);
        GUIStyle headerStyle = new GUIStyle(GUI.skin.box)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = 28f,
            stretchWidth = true,
            wordWrap = false,
            clipping = TextClipping.Clip
        };

        GUI.backgroundColor = boxColor;
        GUI.contentColor = Color.white;
        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, content, headerStyle, null);
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;
        if (foldout && ue != null)
        {
            ue.Invoke();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
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

    /// <summary>
    /// Draw bullet point: "•"
    /// </summary>
    /// <param name="bulletPointColor">Bullet point color string (Hexadecimal).</param>
    protected static void DrawBulletPoint(string bulletPointColor)
    {
        // GUI Style: Bullet Point
        GUIStyle bulletPointStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            stretchWidth = true,
            fixedWidth = 12
        };

        // Draw bullet point w/ the specified color.
        GUI.contentColor = GetColorFromHexString(bulletPointColor);
        GUILayout.Label("•", bulletPointStyle);
        GUI.contentColor = Color.white;
    }
    #endregion

    #region Miscellaneous

    /// <summary>
    /// Get color from hex string.
    /// </summary>
    /// <param name="hexColor">Hex color string.</param>
    /// <returns>New color.</returns>
    protected static Color GetColorFromHexString(string hexColor)
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
    protected static Color AddColor(Color color)
    {
        color += color;
        return color;
    }

    /// <summary>
    /// Add color to existing color.
    /// </summary>
    /// <param name="hexColor">Hex color string.</param>
    /// <returns>New color.</returns>
    protected static Color AddColor(string hexColor)
    {
        Color color = Color.white;
        ColorUtility.TryParseHtmlString(hexColor, out color);
        color += color;

        return color;
    }
    #endregion

}
