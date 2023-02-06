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

    public enum NamingMethod
    {
        Numbers = 0,
        Letters = 1,
        Custom = 2
    }

    public enum Case
    {
        None = 0,
        LowerCase = 1,
        UpperCase = 2,
        CapitalizedCase = 3,
        AlternatingCaps = 4,
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
    private NamingMethod namingMethodType = NamingMethod.Numbers;
    #endregion

    #region Naming Conventions
    // Numbers Settings
    private int numOfDigits = 1;
    private int countFromNumbers = 0;
    private int incrementByNumbers = 0;
    private readonly int countFromAmount = 100;
    private readonly int incrementalAmount = 10;

    // Letters Settings
    private Case lettercase = Case.UpperCase;

    // Custom Settings
    private bool replaceFullName;
    private string replacementName = "New GameObject";
    private string prefixName;
    private bool numeratePrefix;
    private int countFromPrefix = 0;
    private int incrementByPrefix = 0;
    private string suffixName;
    private bool numerateSuffix;
    private int countFromSuffix = 0;
    private int incrementBySuffix = 0;

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

        GUIStyle footerButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            fixedHeight = 36f,
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
        GUIContent duplicateSpecialButtonContent = new GUIContent("★ Duplicate Special", duplicateSpecialTooltip);
        if (GUILayout.Button(duplicateSpecialButtonContent, footerButtonStyle))
        {
            DuplicateObjects();
        }
        GUI.backgroundColor = Color.white;
        // [Apply] Button
        GUI.backgroundColor = GUI.enabled ? AddColor("#00aeff") : Color.gray;
        GUIContent applyButtonContent = new GUIContent("✓ Apply", applyTooltip);
        if (GUILayout.Button(applyButtonContent, footerButtonStyle))
        {
            DuplicateObjects();
        }
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
        // [Close] Button
        GUI.backgroundColor = GUI.enabled ? AddColor("#8030ff") : Color.gray;
        GUIContent closeButtonContent = new GUIContent("╳  Close", closeTooltip);
        if (GUILayout.Button(closeButtonContent, footerButtonStyle))
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
        var letterCases = new string[] { "Lowercase", "Uppercase"};
        GUIContent namingMethodContent = new GUIContent("Naming Method:", "");
        GUI.backgroundColor = AddColor("#00BEFF");
        namingMethodType = (NamingMethod)EditorGUILayout.Popup(namingMethodContent, (int)namingMethodType, namingMethods);
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;
        GUILayout.Space(8);

        switch (namingMethodType)
        {
            case NamingMethod.Numbers:
                #region Number of Digits
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
                #endregion
                #region Count From
                EditorGUILayout.BeginHorizontal();
                DrawBulletPoint("#00BEFF");
                GUIContent countFromContent = new GUIContent("Count from:", "");
                countFromNumbers = EditorGUILayout.IntSlider(countFromContent, countFromNumbers, 0, countFromAmount);
                if (GUILayout.Button("-"))
                {
                    countFromNumbers = Mathf.Clamp(countFromNumbers - 1, 0, countFromAmount);
                }
                if (GUILayout.Button("+"))
                {
                    countFromNumbers = Mathf.Clamp(countFromNumbers + 1, 0, countFromAmount);
                }
                EditorGUILayout.EndHorizontal();
                #endregion
                #region Increment By
                EditorGUILayout.BeginHorizontal();
                DrawBulletPoint("#00BEFF");
                GUIContent incrementByContent = new GUIContent("Increment by:", "");
                incrementByNumbers = EditorGUILayout.IntSlider(incrementByContent, incrementByNumbers, 1, incrementalAmount);
                if (GUILayout.Button("-"))
                {
                    incrementByNumbers = Mathf.Clamp(incrementByNumbers - 1, 1, incrementalAmount);
                }
                if (GUILayout.Button("+"))
                {
                    incrementByNumbers = Mathf.Clamp(incrementByNumbers + 1, 1, incrementalAmount);
                }
                EditorGUILayout.EndHorizontal();
                #endregion
                break;
            case NamingMethod.Letters:
                #region Case
                EditorGUILayout.BeginHorizontal();
                GUIContent caseContent1 = new GUIContent("Case:", "");
                DrawBulletPoint("#00BEFF");
                lettercase = (Case)EditorGUILayout.EnumPopup(caseContent1, lettercase);
                EditorGUILayout.EndHorizontal();
                #endregion
                break;
            case NamingMethod.Custom:
                #region Replace Full Name?
                GUIContent replaceFullNameContent = new GUIContent("Replace Full Name?", "");
                replaceFullName = GUILayout.Toggle(replaceFullName, replaceFullNameContent);
                if (replaceFullName)
                {
                    #region Replace With
                    EditorGUILayout.BeginHorizontal();
                    DrawBulletPoint("#00BEFF", 1);
                    GUIContent replacementNameContent = new GUIContent("Replace with:", "");
                    replacementName = EditorGUILayout.TextField(replacementNameContent, replacementName);
                    EditorGUILayout.EndHorizontal();
                    #endregion
                    #region Case
                    EditorGUILayout.BeginHorizontal();
                    GUIContent caseContent2 = new GUIContent("Case:", "");
                    DrawBulletPoint("#00BEFF", 1);
                    lettercase = (Case)EditorGUILayout.EnumPopup(caseContent2, lettercase);
                    EditorGUILayout.EndHorizontal();
                    #endregion
                }
                #endregion

                DrawLine(GetColorFromHexString("#555555"), 1, 12f);

                #region Prefix
                EditorGUILayout.BeginHorizontal();
                DrawBulletPoint("#00BEFF");
                GUIContent prefixContent = new GUIContent("Prefix:", "");
                prefixName = EditorGUILayout.TextField(prefixContent, prefixName);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                //DrawBulletPoint("#00BEFF");
                //GUILayout.Label("Case:");
                //GUILayout.Toggle(pressed, "ab", "Button");
                //GUILayout.Toggle(pressed, "ab", "Button");
                //GUILayout.Toggle(pressed, "AB", "Button");
                EditorGUILayout.EndHorizontal();
                #endregion
                #region Numerate Prefix
                GUIContent numeratePrefixContent = new GUIContent("Numerate Prefix?", "");
                numeratePrefix = GUILayout.Toggle(numeratePrefix, numeratePrefixContent);
                if (numeratePrefix)
                {
                    #region Count From
                    EditorGUILayout.BeginHorizontal();
                    DrawBulletPoint("#00BEFF", 1);
                    GUIContent countFromPrefixContent = new GUIContent("Count from:", "");
                    countFromPrefix = EditorGUILayout.IntSlider(countFromPrefixContent, countFromPrefix, 0, countFromAmount);
                    if (GUILayout.Button("-"))
                    {
                        countFromPrefix = Mathf.Clamp(countFromPrefix - 1, 0, countFromAmount);
                    }
                    if (GUILayout.Button("+"))
                    {
                        countFromPrefix = Mathf.Clamp(countFromPrefix + 1, 0, countFromAmount);
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion
                    #region Increment By
                    EditorGUILayout.BeginHorizontal();
                    DrawBulletPoint("#00BEFF", 1);
                    GUIContent incrementByPrefixContent = new GUIContent("Increment by:", "");
                    incrementByPrefix = EditorGUILayout.IntSlider(incrementByPrefixContent, incrementByPrefix, 1, incrementalAmount);
                    if (GUILayout.Button("-"))
                    {
                        incrementByPrefix = Mathf.Clamp(incrementByPrefix - 1, 1, incrementalAmount);
                    }
                    if (GUILayout.Button("+"))
                    {
                        incrementByPrefix = Mathf.Clamp(incrementByPrefix + 1, 1, incrementalAmount);
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion
                }
                #endregion

                DrawLine(GetColorFromHexString("#555555"), 1, 12f);

                #region Suffix
                EditorGUILayout.BeginHorizontal();
                DrawBulletPoint("#00BEFF");
                GUIContent suffixContent = new GUIContent("Suffix:", "");
                suffixName = EditorGUILayout.TextField(suffixContent, suffixName);
                EditorGUILayout.EndHorizontal();
                #endregion
                #region Numerate Suffix
                GUIContent numerateSuffixContent = new GUIContent("Numerate Suffix?", "");
                numerateSuffix = GUILayout.Toggle(numerateSuffix, numerateSuffixContent);
                if (numerateSuffix)
                {
                    #region Count From
                    EditorGUILayout.BeginHorizontal();
                    DrawBulletPoint("#00BEFF", 1);
                    GUIContent countFromSuffixContent = new GUIContent("Count from:", "");
                    countFromSuffix = EditorGUILayout.IntSlider(countFromSuffixContent, countFromSuffix, 0, countFromAmount);
                    if (GUILayout.Button("-"))
                    {
                        countFromSuffix = Mathf.Clamp(countFromSuffix - 1, 0, countFromAmount);
                    }
                    if (GUILayout.Button("+"))
                    {
                        countFromSuffix = Mathf.Clamp(countFromSuffix + 1, 0, countFromAmount);
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion
                    #region Increment By
                    EditorGUILayout.BeginHorizontal();
                    DrawBulletPoint("#00BEFF", 1);
                    GUIContent incrementBySuffixContent = new GUIContent("Increment by:", "");
                    incrementBySuffix = EditorGUILayout.IntSlider(incrementBySuffixContent, incrementBySuffix, 1, incrementalAmount);
                    if (GUILayout.Button("-"))
                    {
                        incrementBySuffix = Mathf.Clamp(incrementBySuffix - 1, 1, incrementalAmount);
                    }
                    if (GUILayout.Button("+"))
                    {
                        incrementBySuffix = Mathf.Clamp(incrementBySuffix + 1, 1, incrementalAmount);
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion
                }
                #endregion
                break;
        }

        DrawLine(GetColorFromHexString("#555555"), 1, 12f);

        #region Name Template
        GUIStyle headerStyle = new GUIStyle(GUI.skin.box)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = 28f,
            stretchWidth = true,
            wordWrap = false,
            clipping = TextClipping.Clip
        };

        string name = $"Temp";
        EditorGUILayout.HelpBox($"Name Template: {name}", MessageType.Info);
        GUI.backgroundColor = Color.white;
        #endregion
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
        GUI.backgroundColor = AddColor("#B282FF");
        groupUnderType = EditorGUILayout.Popup(groupUnderContent, groupUnderType, text);
        GUI.backgroundColor = Color.white;
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
        GUI.backgroundColor = AddColor("#FD6D40");
        transformMode = EditorGUILayout.Popup("Mode:", transformMode, transformModes);
        GUI.backgroundColor = Color.white;
        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        DrawBulletPoint("#FD6D40");
        GUIContent positionContent = new GUIContent("Translate (Offset):", positionTooltip);
        GUIContent resetPositionContent = new GUIContent("↺", resetPositionTooltip);
        positionProp = EditorGUILayout.Vector3Field(positionContent, positionProp);
        isDefaultPosition = positionProp == Vector3.zero;

        GUI.backgroundColor = isDefaultPosition ? Color.white : AddColor("#70e04a");
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

        GUI.backgroundColor = isDefaultRotation ? Color.white : AddColor("#70e04a");
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

        GUI.backgroundColor = isDefaultScale ? Color.white : AddColor("#70e04a");
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
    protected static void DrawBulletPoint(string bulletPointColor, int indents = 0)
    {
        // GUI Style: Bullet Point
        GUIStyle bulletPointStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            stretchWidth = true,
            fixedWidth = 12 + (32 * indents),
            contentOffset = new Vector2(32 * indents, 0f)
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
