using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

public class DuplicateSpecialToolEditor : EditorWindow
{
    #region Enums
    public enum NamingMethod
    {
        Numbers = 0,
        Custom = 1
    }

    public enum GroupUnder
    {
        None = 0,
        This = 1,
        Parent = 2,
        World = 3,
        NewGroup = 4
    }

    public enum TransformMode
    {
        Line = 0,
        Grid = 1,
        Circle = 2,
        Spiral = 3,
        Random = 4
    }

    public enum Orientation
    {
        X = 0,
        Y = 1,
        Z = 2
    }

    public enum DegreeMeasure
    {
        QuarterCircle = 0,
        SemiCircle = 1,
        ThreeQuarterCircle = 2,
        FullCircle = 3
    }
    #endregion

    #region Variables
    private static DuplicateSpecialToolEditor window;
    private Vector2 scrollPosition;

    // Banner
    private readonly string bannerPath = "Assets/Duplicate Special Tool/Textures/Duplicate Special Tool Banner.png";

    // Icon paths (Note: Don't 
    private readonly string numberOfCopiesIconPath = "Assets/Duplicate Special Tool/Textures/Icons/NumberOfCopiesIcon.png";
    private readonly string namingConventionsIconPath = "Assets/Duplicate Special Tool/Textures/Icons/NamingConventionIcon.png";
    private readonly string groupUnderIconPath = "Assets/Duplicate Special Tool/Textures/Icons/GroupUnderIcon.png";
    private readonly string transformIconPath = "Assets/Duplicate Special Tool/Textures/Icons/TransformIcon.png";
    private readonly string linkOnIconPath = "Assets/Duplicate Special Tool/Textures/Icons/LinkOnIcon.png";
    private readonly string linkOffIconPath = "Assets/Duplicate Special Tool/Textures/Icons/LinkOffIcon.png";

    #region Number of Duplicates
    private GameObject selectedGameObject;
    private List<GameObject> duplicatesCache;

    private int duplicateCount = 1;
    private bool overridePreviousDuplicates = false;
    private bool unpackPrefab = false;
    private readonly string selectedGameObjectTooltip = "The selected GameObject to duplicate.";
    private readonly string duplicateCountTooltip =  "Specify the number of duplicates to create from the selected GameObject.\n\n" +
                                                     "The range is from 1 to 1000.";
    private readonly string overrideDuplicatesTooltip = "When enabled, it erases the previous set of duplicates upon clicking " +
                                                        "the [Duplicate] button.";
    private readonly string unpackPrefabTooltip = "When enabled, it will instantiate clone(s) of the prefab. If you want to " +
                                                  "preserve the prefab connection to the selected prefab, uncheck this checkbox.\n\n" +
                                                  "Note: This option is disabled if the selected GameObject is not a prefab.";
    private bool isPrefab = false;

    // Section Fields
    private bool showNumberOfCopiesSection = true;
    #endregion

    #region Naming Conventions
    // Numbers Settings
    private NamingMethod namingMethodType = NamingMethod.Numbers;
    private readonly string[] namingMethodTooltips = new string[]
    {
        "Format names for new GameObject(s) via numerating through each duplicate.",
        "Customize how duplicate(s) should be named via prefixes, suffixes, numerations, etc."
    };
    private int numOfLeadingDigits = 0;
    private int countFromNumbers = 0;
    private int incrementByNumbers = 1;
    private readonly int countFromAmount = 100;
    private readonly int incrementalAmount = 10;
    private bool addSpace = true;
    private bool addParentheses = false;
    private bool addBrackets = false;
    private bool addBraces = false;
    private bool addUnderscore = false;
    private bool addHyphen = false;

    // Custom Settings
    private bool replaceFullName = false;
    private string replacementName = "New GameObject";
    private string prefixName;
    private bool numeratePrefix = false;
    private int numOfLeadingDigitsPrefix = 0;
    private int countFromPrefix = 0;
    private int incrementByPrefix = 1;
    private bool addPrefixSpace = true;
    private bool addParenthesesPrefix = false;
    private bool addBracketsPrefix = false;
    private bool addBracesPrefix = false;
    private bool addUnderscorePrefix = false;
    private bool addHyphenPrefix = false;
    private string suffixName;
    private bool numerateSuffix = false;
    private int numOfLeadingDigitsSuffix = 0;
    private int countFromSuffix = 0;
    private int incrementBySuffix = 1;
    private bool addSuffixSpace = true;
    private bool addParenthesesSuffix = false;
    private bool addBracketsSuffix = false;
    private bool addBracesSuffix = false;
    private bool addUnderscoreSuffix = false;
    private bool addHyphenSuffix = false;

    private string namingMethodTooltip = "Specify how duplicated object(s) should be named.";
    private string renameFullNameTooltip = "Enable this to set a new name for all duplicates.";
    private string replaceInNameTooltip = "The selected GameObject's name to replace.";
    private string replaceWithTooltip = "Type in a word to set a new name for all duplicates.";
    private string numOfLeadingDigitsPrefixTooltip = "Specify the number of leading digits to add before each duplicate’s name.\n\n" +
                                                     "The range is from 0 to 10.";
    private string countFromTooltip = "Specify the starting number to count from.\n\n" +
                                      "The range is from 0 to 100.";
    private string incrementByTooltip = "Specify the number to increment by.\n\n" +
                                        "The range is from 0 to 10.";
    private string prefixTooltip = "Type in a word to add before each duplicate’s name.";
    private string numeratePrefixTooltip = "Enable this to enumerate through all duplicates’ prefix names by number.";
    private string addPrefixSpaceTooltip = "Enable this to add a space between the name and the number (prefix).";
    private string numOfLeadingDigitsSuffixTooltip = "Specify the number of leading digits to add after each duplicate’s name.\n\n" +
                                                     "The range is from 0 to 10.";
    private string suffixTooltip = "Type in a word to add after each duplicate’s name.";
    private string numerateSuffixTooltip = "Enable this to enumerate through all duplicates’ suffix names by number.";
    private string addSuffixSpaceTooltip = "Enable this to add a space between the name and the number (suffix).";

    // Section Fields
    private bool showNamingConventionsSection = false;
    #endregion

    #region Group Under
    private GameObject groupParent = null;
    private GameObject groupRelative = null;
    private string newGroupName = "New Group";
    private readonly string[] groupUnderTypeTooltips = new string[]
    {
        "Groups the duplicate(s) next to the selected GameObject.",
        "Groups the duplicate(s) under the selected GameObject.",
        "Groups the duplicate(s) under the specified parent.",
        "Groups the duplicate(s) in the world.",
        "Creates a new group GameObject to group the duplicate(s) under."
    };
    private readonly string groupUnderTooltip = "The GameObject to group all duplicates under.";
    private readonly string groupThisUnderTooltip = "The GameObject to group the new group of duplicates under.";

    // Section Fields
    private bool showGroupUnderSection = false;
    private int groupUnderNum = 0;
    private GroupUnder groupUnderType { get { return (GroupUnder)groupUnderNum; } }
    #endregion

    #region Transform
    private bool showTransformSection = false;
    private TransformMode transformMode = TransformMode.Line;
    private readonly string[] transformModes = new string[] { "Line", "Grid", "Circle", "Spiral", "Random" };
    private readonly string[] transformModeTooltips = new string[]
    {
        "Set the position, rotation, and scale of all duplicates along a line.",
        "Arrange all duplicates on a structured grid.",
        "Arrange all duplicates in a circular pattern.",
        "Arrange all duplicates in a spiral pattern.",
        "Randomly set the position, rotation, and scale of all duplicates."
    };

    #region Line Mode
    #region Translate (Offset)
    private Vector3 positionProp = Vector3.zero;
    private bool isDefaultPosition;
    private bool linkPosition = false;
    private readonly string positionTooltip = "Move duplicate(s) in the desired direction at a given distance.";
    private readonly string linkPositionTooltip = "Link the axes to set uniform position values for all axes.\n" +
                                                  "Unlink the axes to set different position values for the X, Y, and Z " +
                                                  "axis properties.";
    private readonly string resetPositionTooltip = "Reset position values to their default values.\n\n" +
                                                   "Default position is (X: 0.0, Y: 0.0, Z: 0.0).";
    #endregion
    #region Rotate (Offset)
    private Vector3 rotationProp = Vector3.zero;
    private bool isDefaultRotation;
    private bool linkRotation = false;
    private readonly string rotationTooltip = "Rotate duplicate(s) around a given axis/axes.";
    private readonly string linkRotationTooltip = "Link the axes to set uniform rotation values for all axes.\n" +
                                                  "Unlink the axes to set different rotation values for the X, Y, and Z " +
                                                  "axis properties.";
    private readonly string resetRotationTooltip = "Reset rotation values to their default values.\n\n" +
                                                   "Default rotation is (X: 0.0, Y: 0.0, Z: 0.0).";
    #endregion
    #region Scale (Offset)
    private Vector3 scaleProp = Vector3.zero;
    private bool isDefaultScale;
    private bool linkScale = false;
    private readonly string scaleTooltip = "Scale duplicate(s) on a given axis/axes.";
    private readonly string linkScaleTooltip = "Link the axes to set uniform scale values for all axes.\n" +
                                               "Unlink the axes to set different scale values for the X, Y, and Z " +
                                               "axis properties.";
    private readonly string resetScaleTooltip = "Reset scale values to their default values.\n\n" +
                                                "Default scale is (X: 0.0, Y: 0.0, Z: 0.0).";
    #endregion
    #endregion

    #region Grid Mode
    private Vector3Int gridSize = Vector3Int.one;
    private Vector3Int prevGridSize = Vector3Int.one;
    private Vector3 gridSpacing = Vector3.zero;
    private bool isDefaultGridSize;
    private bool isDefaultGridSpacing;
    private bool linkGridSize = false;
    private bool linkGridSpacing = false;
    private readonly string gridSizeTooltip = "Specify the size of the grid.";
    private readonly string gridSpacingTooltip = "Specify the spacing between the duplicates.";
    private readonly string linkGridSizeTooltip = "Link the axes to set uniform grid size values for all axes.\n" +
                                                  "Unlink the axes to set different grid size values for the X, Y, and Z " +
                                                  "axis properties.";
    private readonly string resetGridSizeTooltip = "Reset grid size values to their default values.\n\n" +
                                                   "Default grid size is (X: 1.0, Y: 1.0, Z: 1.0).";
    private readonly string linkGridSpacingTooltip = "Link the axes to set uniform grid spacing values for all axes.\n" +
                                                     "Unlink the axes to set different grid spacing values for the X, Y, and Z " +
                                                     "axis properties.";
    private readonly string resetGridSpacingTooltip = "Reset grid spacing values to their default values.\n\n" +
                                                      "Default grid spacing is (X: 0.0, Y: 0.0, Z: 0.0).";
    #endregion

    #region Circle Mode
    private float radialDistance = 0f;
    private Orientation circleOrientation = Orientation.X;
    private DegreeMeasure degreeMeasure = DegreeMeasure.FullCircle;
    private readonly string[] degreeMeasureOptions = new string[]
    {
        "Quarter Circle (90°)",
        "Semicircle (180°)",
        "Three Quarter Circle (270°)",
        "Full Circle (360°)"
    };
    private bool lookAtCenter = false;
    private readonly string radiusTooltip = "Specify the distance from the center of the circular pattern.";
    private readonly string degreeMeasureTooltip = "Specify the degree measure of the circular pattern.";
    private readonly string circleOrientationTooltip = "Specify how the circular group of duplicates is aligned on a given axis.";
    private readonly string lookAtCenterTooltip = "When enabled, all duplicates will look at the center of the circular pattern.";
    #endregion

    #region Spiral Mode
    private float spiralRadius = 0f;
    private float curveAmount = 0f;
    private float spiralHeight = 0f;
    private Orientation spiralOrientation = Orientation.X;
    private readonly string spiralRadiusTooltip = "Specify the distance from the center of the spiral pattern.";
    private readonly string curveAmountTooltip = "Specify the curve amount of the spiral pattern.";
    private readonly string spiralHeightTooltip = "Specify the height of the spiral pattern.";
    private readonly string spiralOrientationTooltip = "Specify how the spiral group of duplicates is aligned on a given axis.";
    #endregion

    #region Random Mode
    #region Randomize Position?
    private bool randomizePosition = false;
    private float minDistance = 0f;
    private float maxDistance = 100f;
    private bool lockPositionX = false;
    private bool lockPositionY = false;
    private bool lockPositionZ = false;

    private readonly string randomizePositionTooltip = "Enable this to allow randomizing the position of all duplicates.";
    private readonly string distanceRangeTooltip = "Specify the minimum and maximum distance duplicates are spawned away from the selected GameObject.\n\n" +
                                                    "The range is from 0 to 1000.";
    private readonly string lockPositionXTooltip = "When enabled, it locks each duplicate’s X-position value, and it’s unaffected by randomization.";
    private readonly string lockPositionYTooltip = "When enabled, it locks each duplicate’s Y-position value, and it’s unaffected by randomization.";
    private readonly string lockPositionZTooltip = "When enabled, it locks each duplicate’s Z-position value, and it’s unaffected by randomization.";
    #endregion

    #region Randomize Rotation?
    private bool randomizeRotation = false;
    private bool lockPitch = false;
    private bool lockYaw = false;
    private bool lockRoll = false;

    private readonly string randomizeRotationTooltip = "Enable this to allow randomizing the rotation of all duplicates.";
    private readonly string lockRotationXTooltip = "When enabled, it locks each duplicate’s X-rotation value, and it’s unaffected by randomization.";
    private readonly string lockRotationYTooltip = "When enabled, it locks each duplicate’s Y-rotation value, and it’s unaffected by randomization.";
    private readonly string lockRotationZTooltip = "When enabled, it locks each duplicate’s Z-rotation value, and it’s unaffected by randomization.";
    #endregion

    #region Randomize Scale?
    private bool randomizeScale = false;
    private float minScale = 0f;
    private float maxScale = 10f;
    private bool lockScaleX = false;
    private bool lockScaleY = false;
    private bool lockScaleZ = false;
    private float absoluteMaxScaleValue = 10f;

    private readonly string randomizeScaleTooltip = "Enable this to allow randomizing the scale of all duplicates.";
    private readonly string scaleRangeTooltip = "Specify the minimum and maximum scale of all duplicates.\n\n" +
                                                "The range is from 0 to 10.";
    private readonly string lockScaleXTooltip = "When enabled, it locks each duplicate’s X-scale value, and it’s unaffected by randomization.";
    private readonly string lockScaleYTooltip = "When enabled, it locks each duplicate’s Y-scale value, and it’s unaffected by randomization.";
    private readonly string lockScaleZTooltip = "When enabled, it locks each duplicate’s Z-scale value, and it’s unaffected by randomization.";
    #endregion
    #endregion
    #endregion

    #region Information
    private readonly string expandSectionsTooltip = "Click this button to expand all sections and show its contents.";
    private readonly string collapseSectionsTooltip = "Click this button to collapse all sections and hide its contents.";
    private readonly string documentationTooltip = "Click this button to see the official documentation on how to use " +
                                                   "the Duplicate Special Tool.";
    #endregion

    #region Button(s) Footer
    private readonly string duplicateTooltip = "Duplicate multiple instances of the selected gameObject.";
    private readonly string closeTooltip = "Close the editor window.";
    #endregion

    private string templateName = "";
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
        window = GetWindow<DuplicateSpecialToolEditor>("Duplicate Special V1.0");
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
        // Set Selected GameObject.
        if (Selection.activeGameObject != null && selectedGameObject == null)
        {
            selectedGameObject = Selection.activeGameObject;
        }

        if (duplicatesCache == null)
        {
            duplicatesCache = new List<GameObject>();
        }

        //Rect rect;
        //float imageHeight = imageInspector.height;
        //float imageWidth = imageInspector.width;
        //float aspectRatio = imageHeight / imageWidth;
        //rect = GUILayoutUtility.GetRect(imageHeight, aspectRatio * Screen.width * 0.7f);
        //EditorGUI.DrawTextureTransparent(rect, imageInspector);

        // Set minimum window size.
        if (window == null)
        {
            window = GetWindow<DuplicateSpecialToolEditor>("Duplicate Special V1.0");
        }
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);

        if (window.docked)
        {
            window.minSize = new Vector2(540f, 200f);
            window.maxSize = new Vector2(540f, 360f);
        }
        else
        {
            window.minSize = new Vector2(540f, 480f);
            window.maxSize = new Vector2(540f, 720f);
        }

        GUIStyle footerButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            fixedHeight = 36f
        };

        #region Banner
        Texture2D banner = (Texture2D)AssetDatabase.LoadAssetAtPath(bannerPath, typeof(Texture2D));
        float imageHeight = banner.height;
        float imageWidth = banner.width;
        float aspectRatio = imageHeight / imageWidth;
        Rect bannerRect = GUILayoutUtility.GetRect(imageHeight, aspectRatio * Screen.width * 0.5f);
        EditorGUI.DrawTextureTransparent(bannerRect, banner);
        #endregion

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        #region Number of Duplicates
        DrawSection("Number of Duplicates", ref showNumberOfCopiesSection, DisplayNumberOfDuplicatesSection, numberOfCopiesIconPath, AddColor("#00E6BC") * 0.75f);
        #endregion

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        #region Naming Conventions
        DrawSection("Naming Conventions", ref showNamingConventionsSection, DisplayNamingConventionsSection, namingConventionsIconPath, AddColor("#00BEFF") * 0.75f);
        #endregion

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        #region Group Under
        DrawSection("Group Under", ref showGroupUnderSection, DisplayGroupUnderSection, groupUnderIconPath, AddColor("#B282FF") * 0.75f);
        #endregion

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        #region Transform
        DrawSection("Transform", ref showTransformSection, DisplayTransformSection, transformIconPath, AddColor("#FD6D40") * 0.75f);
        #endregion

        EditorGUIUtility.wideMode = true;
        // Toggle wide mode.
        ToggleWideMode(180f);

        #region Button(s) Footer
        GUILayout.FlexibleSpace();
        GUILayout.EndScrollView();

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        #region Other
        // Expand All Sections
        GUIContent expandSectionsContent = new GUIContent("Expand All Sections", expandSectionsTooltip);
        if (GUILayout.Button(expandSectionsContent))
        {
            ExpandAllSections();
        }
        // Collapse All Sections
        GUIContent collapseSectionsContent = new GUIContent("Collapse All Sections", collapseSectionsTooltip);
        if (GUILayout.Button(collapseSectionsContent))
        {
            CollapseAllSections();
        }
        // Documentation
        GUIContent documentationContent = new GUIContent("Documentation", documentationTooltip);
        if (GUILayout.Button(documentationContent))
        {
            OpenDocumentation();
        }
        #endregion

        #region Information
        // Display Duplicate Special information.
        string objectName = selectedGameObject != null ? selectedGameObject.name : "N/A";
        if (namingMethodType == NamingMethod.Numbers)
        {
            templateName = GetNumericalTemplateName(countFromNumbers);
        }
        else
        {
            templateName = GetCustomTemplateName(countFromPrefix, countFromSuffix);
        }
        EditorGUILayout.HelpBox($"Selected GameObject: {objectName}\n" +
                                $"Is Prefab?: {isPrefab}\n" +
                                $"No. of copies: {duplicateCount}\n" +
                                $"Name Template: {templateName}\n" +
                                $"Group Under: {GetGroupUnderName()}", MessageType.Info);

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
        #endregion

        DrawLine(GetColorFromHexString("#aaaaaa"), 1, 4f);

        GUILayout.BeginHorizontal();
        GUI.enabled = selectedGameObject != null;
        #region Duplicate
        // [Duplicate] Button
        GUI.backgroundColor = GUI.enabled ? AddColor("#00ffae") : Color.gray;
        GUIContent duplicateButtonContent = new GUIContent("✓ Duplicate", duplicateTooltip);
        if (GUILayout.Button(duplicateButtonContent, footerButtonStyle))
        {
            DuplicateObjects();
        }
        GUI.backgroundColor = Color.white;
        #endregion
        GUI.enabled = true;
        #region Close
        // [Close] Button
        GUI.backgroundColor = GUI.enabled ? AddColor("#FF004C") : Color.gray;
        GUIContent closeButtonContent = new GUIContent("╳  Close", closeTooltip);
        if (GUILayout.Button(closeButtonContent, footerButtonStyle))
        {
            // Clear cache.
            duplicatesCache.Clear();
            // Close editor window.
            Close();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        #endregion
        #endregion
    }

    #region Duplicate Special Tool Method(s)
    private string GetCustomTemplateName(int gameObjectPrefixNum, int gameObjectSuffixNum)
    {
        string selectedName = selectedGameObject != null ? selectedGameObject.name : string.Empty;
        string numericalPrefix = numeratePrefix ? gameObjectPrefixNum.ToString() : string.Empty;
        string prefixSpace = addPrefixSpace ? " " : string.Empty;
        string spaceAfterPrefix = addPrefixSpace && !string.IsNullOrWhiteSpace(prefixName) ? " " : string.Empty;
        string numericalSuffix = numerateSuffix ? gameObjectSuffixNum.ToString() : string.Empty;
        string suffixSpace = addSuffixSpace ? " " : string.Empty;
        string spaceBeforeSuffix = addSuffixSpace && !string.IsNullOrWhiteSpace(suffixName) ? " " : string.Empty;

        #region Prefix
        // Add leading zeros "0" before the starting number (Prefix).
        for (int i = 0; i < numOfLeadingDigitsPrefix; i++)
        {
            numericalPrefix = numericalPrefix.Insert(0, "0");
        }

        if (numeratePrefix)
        {
            if (addParenthesesPrefix) { numericalPrefix = $"{spaceAfterPrefix}({numericalPrefix}){prefixSpace}"; }                    // Parentheses ()
            else if (addBracketsPrefix) { numericalPrefix = $"{spaceAfterPrefix}[{numericalPrefix}]{prefixSpace}"; }                  // Brackets []
            else if (addBracesPrefix) { numericalPrefix = $"{spaceAfterPrefix}{{{numericalPrefix}}}{prefixSpace}"; }                  // Braces {}
            else if (addUnderscorePrefix) { numericalPrefix = $"{spaceAfterPrefix}{numericalPrefix}{prefixSpace}_{prefixSpace}"; }    // Underscore _
            else if (addHyphenPrefix) { numericalPrefix = $"{spaceAfterPrefix}{numericalPrefix}{prefixSpace}-{prefixSpace}"; }        // Hypen -
            else { numericalPrefix = $"{spaceAfterPrefix}{numericalPrefix}{prefixSpace}"; }
        }
        else
        {
            numericalPrefix = $"{spaceAfterPrefix}";
        }
        #endregion

        #region Suffix
        // Add leading zeros "0" before the starting number (Suffix).
        for (int i = 0; i < numOfLeadingDigitsSuffix; i++)
        {
            numericalSuffix = numericalSuffix.Insert(0, "0");
        }

        if (numerateSuffix)
        {
            if (addParenthesesSuffix) { numericalSuffix = $"{spaceBeforeSuffix}({numericalSuffix})"; }                    // Parentheses ()
            else if (addBracketsSuffix) { numericalSuffix = $"{spaceBeforeSuffix}[{numericalSuffix}]"; }                  // Brackets []
            else if (addBracesSuffix) { numericalSuffix = $"{spaceBeforeSuffix}{{{numericalSuffix}}}"; }                  // Braces {}
            else if (addUnderscoreSuffix) { numericalSuffix = $"{spaceBeforeSuffix}_{suffixSpace}{numericalSuffix}"; }    // Underscore _
            else if (addHyphenSuffix) { numericalSuffix = $"{spaceBeforeSuffix}-{suffixSpace}{numericalSuffix}"; }        // Hypen -
            else { numericalSuffix = $"{spaceBeforeSuffix}{numericalSuffix}"; }
        }
        else
        {
            numericalSuffix = "";
        }
        #endregion

        selectedName = replaceFullName ? replacementName : selectedName;
        templateName = $"{prefixName}{numericalPrefix}{selectedName}{spaceBeforeSuffix}{suffixName}{numericalSuffix}";

        return templateName;
    }

    private string GetNumericalTemplateName(int gameObjectNum)
    {
        string space = addSpace ? " " : string.Empty;
        string numericalSuffix = gameObjectNum.ToString();
        string selectedName = selectedGameObject != null ? selectedGameObject.name : string.Empty;

        // Add leading zeros "0" before the starting number.
        for (int i = 0; i < numOfLeadingDigits; i++)
        {
            numericalSuffix = numericalSuffix.Insert(0, "0");
        }

        if (addParentheses) { numericalSuffix = $"{space}({numericalSuffix})"; }            // Parentheses ()
        else if (addBrackets) { numericalSuffix = $"{space}[{numericalSuffix}]"; }          // Brackets []
        else if (addBraces) { numericalSuffix = $"{space}{{{numericalSuffix}}}"; }          // Braces {}
        else if (addUnderscore) { numericalSuffix = $"{space}_{space}{numericalSuffix}"; }  // Underscore _
        else if (addHyphen) { numericalSuffix = $"{space}-{space}{numericalSuffix}"; }      // Hypen -
        else { numericalSuffix = $"{space}{numericalSuffix}"; }

        templateName = $"{selectedName}{numericalSuffix}";

        return templateName;
    }

    private string GetGroupUnderName()
    {
        string name = "N/A";

        switch (groupUnderType)
        {
            case GroupUnder.None:
                if (selectedGameObject == null)
                    break;
                name = selectedGameObject.transform.parent != null ? selectedGameObject.transform.parent.name : name;
                break;
            case GroupUnder.This:
                name = selectedGameObject != null ? selectedGameObject.name : name;
                break;
            case GroupUnder.Parent:
                name = groupParent != null ? groupParent.name : name;
                break;
            case GroupUnder.NewGroup:
                name = newGroupName;
                break;
        }

        return name;
    }

    /// <summary>
    /// Create duplicate(s) of the selected GameObject.
    /// </summary>
    private void DuplicateObjects()
    {
        if (selectedGameObject == null)
            return;

        List<GameObject> duplicatedObjects = new List<GameObject>();

        // Destroy all duplicates in the cache if the Override Previous Duplicates option is checked.
        if (overridePreviousDuplicates)
        {
            if (duplicatesCache.Count > 0)
            {
                // Destroy all duplicates in the cache.
                foreach (GameObject duplicatedObj in duplicatesCache)
                {
                    DestroyImmediate(duplicatedObj);
                }
                // Clear cache.
                duplicatesCache.Clear();
            }
        }
        // Otherwise, just clear the cache.
        else
        {
            // Clear cache.
            duplicatesCache.Clear();
        }

        if (isPrefab && !unpackPrefab)
        {
            for (int i = 0; i < duplicateCount; i++)
            {
                string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selectedGameObject);
                GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
                GameObject duplicatedPrefab = PrefabUtility.InstantiatePrefab(go) as GameObject;
                Undo.RegisterCreatedObjectUndo(duplicatedPrefab, $"Duplicate {duplicateCount} Prefab Instances");

                // Prevent method from adding redundant prefab(s) to list.
                if (duplicatedObjects.Contains(duplicatedPrefab))
                    continue;
                // Add newly duplicated prefab to the list.
                duplicatedObjects.Add(duplicatedPrefab);
            }
        }
        else
        {
            for (int i = 0; i < duplicateCount; i++)
            {
                GameObject duplicatedObj = Instantiate<GameObject>(selectedGameObject);
                Undo.RegisterCreatedObjectUndo(duplicatedObj, $"Duplicate {duplicateCount} Instances");

                // Prevent method from adding redundant object(s) to list.
                if (duplicatedObjects.Contains(duplicatedObj))
                    continue;
                // Add newly duplicated object to the list.
                duplicatedObjects.Add(duplicatedObj);
            }
        }
        GameObject newGroup = null;
        // If user chooses to group duplicate(s) under a new group, create a new group.
        if (groupUnderType == GroupUnder.NewGroup)
        {
            newGroup = Instantiate<GameObject>(selectedGameObject);
            Undo.RegisterCreatedObjectUndo(newGroup, $"Create New Group: {newGroupName}");

            if (newGroup != null)
            {
                // Set name of new group.
                newGroup.name = newGroupName;
                // Set new group under the specified parent and get the new group's transform.
                if (groupRelative != null)
                {
                    newGroup.transform.SetParent(groupRelative.transform);
                }
            }
        }

        // Add duplicates list to the cache.
        duplicatesCache.AddRange(duplicatedObjects);

        // Set the names of all duplicates.
        SetNames(duplicatedObjects);
        // Set the position of all duplicates.
        SetPositions(duplicatedObjects);
        // Set the rotation of all duplicates.
        SetRotations(duplicatedObjects);
        // Set the scale of all duplicates.
        SetScales(duplicatedObjects);

        // Group all duplicated objects under the appropriate object.
        foreach (GameObject duplicatedObj in duplicatedObjects)
        {
            Transform target = newGroup != null ? newGroup.transform : GetGroupUnderTarget();
            duplicatedObj.transform.SetParent(target);
        }
    }

    /// <summary>
    /// Get the target to group the duplicate(s) under.
    /// </summary>
    /// <returns>Group target.</returns>
    private Transform GetGroupUnderTarget()
    {
        Transform target = null;

        switch (groupUnderType)
        {
            // None
            case GroupUnder.None:
                target = selectedGameObject.transform.parent != null ? selectedGameObject.transform.parent : null;
                break;
            // This
            case GroupUnder.This:
                target = selectedGameObject.transform;
                break;
            // Parent
            case GroupUnder.Parent:
                target = groupParent != null ? groupParent.transform : null;
                break;
            // World
            case GroupUnder.World:
                target = null;
                break;
            // New Group
            case GroupUnder.NewGroup:
                break;
        }

        return target;
    }

    private void SetNames(List<GameObject> duplicatedObjects)
    {
        int index = 0;

        foreach (GameObject duplicatedObj in duplicatedObjects)
        {
            string newName = string.Empty;
            int gameObjectNum = (countFromNumbers + (index * incrementByNumbers));
            int gameObjectPrefixNum = (countFromPrefix + (index * incrementByPrefix));
            int gameObjectSuffixNum = (countFromSuffix + (index * incrementBySuffix));

            if (namingMethodType == NamingMethod.Numbers)
            {
                newName = GetNumericalTemplateName(gameObjectNum);
            }
            else
            {
                newName = GetCustomTemplateName(gameObjectPrefixNum, gameObjectSuffixNum);
            }

            if (duplicatedObj != null)
            {
                // Set duplicate's name.
                duplicatedObj.name = newName;
            }

            // Increment index.
            index++;
        }
    }


    #region Positional Method(s)
    /// <summary>
    /// Set the positions of all duplicates.
    /// </summary>
    /// <param name="duplicatedObjects">Duplicated objects.</param>
    private void SetPositions(List<GameObject> duplicatedObjects)
    {
        switch (transformMode)
        {
            case TransformMode.Line:
                DistributeDuplicatesLinearly(duplicatedObjects);
                break;
            case TransformMode.Grid:
                DistributeDuplicatesOnGrid(duplicatedObjects);
                break;
            case TransformMode.Circle:
                DistributeDuplicatesInCircle(duplicatedObjects);
                break;
            case TransformMode.Spiral:
                DistributeDuplicatesInSpiral(duplicatedObjects);
                break;
            case TransformMode.Random:
                if (randomizePosition)
                {
                    DistributeDuplicatesAtRandom(duplicatedObjects);
                }
                else
                {
                    foreach (GameObject duplicatedObj in duplicatedObjects)
                    {
                        duplicatedObj.transform.position = Vector3.zero;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Distribute all duplicate(s) linearly.
    /// </summary>
    /// <param name="duplicatedObjects">Duplicated objects.</param>
    private void DistributeDuplicatesLinearly(List<GameObject> duplicatedObjects)
    {
        int index = 1;
        Vector3 origin = selectedGameObject != null ? selectedGameObject.transform.position : Vector3.zero;

        foreach (GameObject duplicatedObj in duplicatedObjects)
        {
            Vector3 position = origin;
            duplicatedObj.transform.position = position + (positionProp * index);

            // Increment index.
            index++;
        }
    }

    /// <summary>
    /// Distribute all duplicate(s) on a grid.
    /// </summary>
    /// <param name="duplicatedObjects">Duplicated objects.</param>
    private void DistributeDuplicatesOnGrid(List<GameObject> duplicatedObjects)
    {
        int index = 0;
        Vector3 origin = selectedGameObject != null ? selectedGameObject.transform.position : Vector3.zero;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    GameObject duplicatedObj = duplicatedObjects[index++];
                    Vector3 position = origin;
                    Vector3 gridPosition = new Vector3(gridSpacing.x * (x + 1), gridSpacing.y * (y + 1), gridSpacing.z * (z + 1));
                    duplicatedObj.transform.position = position + gridPosition;
                }
            }
        }
    }

    /// <summary>
    /// Distribute all duplicate(s) around a circle.
    /// </summary>
    /// <param name="duplicatedObjects">Duplicated objects.</param>
    private void DistributeDuplicatesInCircle(List<GameObject> duplicatedObjects)
    {
        int index = 0;
        int countOffset = degreeMeasure == DegreeMeasure.FullCircle ? 0 : 1;
        Vector3 centerPoint = selectedGameObject.transform.position;
        float angleQuotient = duplicatedObjects.Count > 1 ? GetDegreeMeasure() / (duplicatedObjects.Count - countOffset) : 0f;

        // Arrange each duplicate around the center.
        foreach (GameObject duplicatedObj in duplicatedObjects)
        {
            Vector3 angle = duplicatedObj.transform.eulerAngles;
            duplicatedObj.transform.position = GetRadialVector(circleOrientation, centerPoint, radialDistance, angleQuotient * index);

            if (lookAtCenter)
            {
                switch (circleOrientation)
                {
                    case Orientation.X:
                        duplicatedObj.transform.eulerAngles = new Vector3(angle.x, 90f, angleQuotient * index);
                        break;
                    case Orientation.Y:
                        duplicatedObj.transform.eulerAngles = new Vector3(angle.x, -angleQuotient * index, -90f);
                        break;
                    case Orientation.Z:
                        duplicatedObj.transform.eulerAngles = new Vector3(angle.x, angle.y, (angleQuotient * index) - 90f);
                        break;
                }
            }

            // Increment index.
            index++;
        }
    }

    private float GetDegreeMeasure()
    {
        float arcLength = 0f;

        switch (degreeMeasure)
        {
            case DegreeMeasure.QuarterCircle:
                arcLength = 90f;
                break;
            case DegreeMeasure.SemiCircle:
                arcLength = 180f;
                break;
            case DegreeMeasure.ThreeQuarterCircle:
                arcLength = 270f;
                break;
            case DegreeMeasure.FullCircle:
                arcLength = 360f;
                break;
        }

        return arcLength;
    }

    /// <summary>
    /// Distribute all duplicate(s) around a spiral.
    /// </summary>
    private void DistributeDuplicatesInSpiral(List<GameObject> duplicatedObjects)
    {
        int index = 1;
        Vector3 origin = selectedGameObject != null ? selectedGameObject.transform.position : Vector3.zero;
        Vector3 originOffset = Vector3.zero;
        switch (spiralOrientation)
        {
            case Orientation.X:
                originOffset = new Vector3(spiralHeight, 0f, 0f);
                break;
            case Orientation.Y:
                originOffset = new Vector3(0f, spiralHeight, 0f);
                break;
            case Orientation.Z:
                originOffset = new Vector3(0f, 0f, spiralHeight);
                break;
        }
        originOffset *= ((float)index / (float)duplicatedObjects.Count);

        foreach (GameObject duplicatedObj in duplicatedObjects)
        {
            Vector3 position = origin;
            float currentRadius = ((float)index / (float)duplicatedObjects.Count) * spiralRadius;
            origin += originOffset;
            Vector3 radialVector = GetRadialVector(spiralOrientation, origin, currentRadius, curveAmount * 0.5f * index);
            duplicatedObj.transform.position = radialVector;

            // Increment index.
            index++;
        }
    }

    private void DistributeDuplicatesAtRandom(List<GameObject> duplicatedObjects)
    {
        int index = 0;
        Vector3 selectedGameObjectPosition = selectedGameObject.transform.position;

        foreach (GameObject duplicatedObj in duplicatedObjects)
        {
            // Get random X-position value.
            float randX = lockPositionX ? selectedGameObjectPosition.x : Random.Range(minDistance, maxDistance);
            if (!lockPositionX) { randX = Random.Range(0, 100) > 50 ? -randX : randX; }
            // Get random Y-position value.
            float randY = lockPositionY ? selectedGameObjectPosition.y : Random.Range(minDistance, maxDistance);
            if (!lockPositionY) { randY = Random.Range(0, 100) > 50 ? -randY : randY; }
            // Get random Z-position value.
            float randZ = lockPositionZ ? selectedGameObjectPosition.z : Random.Range(minDistance, maxDistance);
            if (!lockPositionZ) { randZ = Random.Range(0, 100) > 50 ? -randZ : randZ; }

            // Set duplicate's position.
            duplicatedObj.transform.position = selectedGameObjectPosition + new Vector3(randX, randY, randZ);

            // Increment index.
            index++;
        }
    }
    #endregion

    #region Rotational Method(s)
    /// <summary>
    /// Set the rotation of all duplicates.
    /// </summary>
    /// <param name="duplicatedObjects">Duplicated objects.</param>
    private void SetRotations(List<GameObject> duplicatedObjects)
    {
        switch (transformMode)
        {
            case TransformMode.Line:
                RotateDuplicatesLinearly(duplicatedObjects);
                break;
            case TransformMode.Random:
                if (randomizeRotation)
                {
                    RotateDuplicatesAtRandom(duplicatedObjects);
                }
                else
                {
                    foreach (GameObject duplicatedObj in duplicatedObjects)
                    {
                        duplicatedObj.transform.eulerAngles = Vector3.zero;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Rotate all duplicate(s) linearly.
    /// </summary>
    private void RotateDuplicatesLinearly(List<GameObject> duplicatedObjects)
    {
        int index = 1;
        foreach (GameObject duplicatedObj in duplicatedObjects)
        {
            Vector3 rotation = rotationProp;
            duplicatedObj.transform.eulerAngles = rotation * index;

            // Increment index.
            index++;
        }
    }

    private void RotateDuplicatesAtRandom(List<GameObject> duplicatedObjects)
    {
        Vector3 selectedGameObjectRotation = selectedGameObject.transform.eulerAngles;
        foreach (GameObject duplicatedObj in duplicatedObjects)
        {
            // Get random pitch (X) value.
            float randPitch = lockPitch ? selectedGameObjectRotation.x : Random.Range(0f, 360f);
            // Get random yaw (Y) value.
            float randYaw = lockYaw ? selectedGameObjectRotation.y : Random.Range(0f, 360f);
            // Get random roll (Z) value.
            float randRoll = lockRoll ? selectedGameObjectRotation.z : Random.Range(0f, 360f);

            // Set duplicate's rotation.
            duplicatedObj.transform.eulerAngles = new Vector3(randPitch, randYaw, randRoll);
        }
    }
    #endregion

    #region Scalar Method(s)
    /// <summary>
    /// Set the scale of all duplicates.
    /// </summary>
    /// <param name="duplicatedObjects">Duplicated objects.</param>
    private void SetScales(List<GameObject> duplicatedObjects)
    {
        switch (transformMode)
        {
            case TransformMode.Line:
                ScaleDuplicatesLinearly(duplicatedObjects);
                break;
            case TransformMode.Random:
                if (randomizeScale)
                {
                    ScaleDuplicatesAtRandom(duplicatedObjects);
                }
                else
                {
                    foreach (GameObject duplicatedObj in duplicatedObjects)
                    {
                        duplicatedObj.transform.localScale = Vector3.one;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Scale all duplicate(s) linearly.
    /// </summary>
    private void ScaleDuplicatesLinearly(List<GameObject> duplicatedObjects)
    {
        int index = 1;
        Vector3 scale = selectedGameObject != null ? selectedGameObject.transform.localScale : Vector3.one;

        foreach (GameObject duplicatedObj in duplicatedObjects)
        {
            duplicatedObj.transform.localScale = scale + (scaleProp * index);

            // Increment index.
            index++;
        }
    }

    private void ScaleDuplicatesAtRandom(List<GameObject> duplicatedObjects)
    {
        Vector3 selectedGameObjectScale = selectedGameObject.transform.localScale;

        foreach (GameObject duplicatedObj in duplicatedObjects)
        {
            // Get random X-scale value.
            float randX = lockScaleX ? selectedGameObjectScale.x : Random.Range(minScale, maxScale);
            if (!lockScaleX) { randX = Random.Range(0, 100) > 50 ? -randX : randX; }
            // Get random Y-scale value.
            float randY = lockScaleY ? selectedGameObjectScale.y : Random.Range(minScale, maxScale);
            if (!lockScaleY) { randY = Random.Range(0, 100) > 50 ? -randY : randY; }
            // Get random Z-scale value.
            float randZ = lockScaleZ ? selectedGameObjectScale.z : Random.Range(minScale, maxScale);
            if (!lockScaleZ) { randZ = Random.Range(0, 100) > 50 ? -randZ : randZ; }

            // Set duplicate's scale.
            duplicatedObj.transform.localScale = new Vector3(randX, randY, randZ);
        }
    }
    #endregion

    /// <summary>
    /// Converts an angle to a position around the center.
    /// </summary>
    /// <param name="orientation">Orientation.</param>
    /// <param name="center">Center.</param>
    /// <param name="radius">Radius.</param>
    /// <param name="angle"><Angle (in degrees)./param>
    /// <returns>The position around the radius.</returns>
    public Vector3 GetRadialVector(Orientation orientation, Vector3 center, float radius, float angle)
    {
        Vector3 pos;
        angle -= 90f;

        if (orientation == Orientation.X)
        {
            pos.x = center.x;
            pos.y = center.y + radius * -Mathf.Sin(angle * Mathf.Deg2Rad);
            pos.z = center.z + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        }
        else if (orientation == Orientation.Y)
        {
            pos.x = center.x + radius * -Mathf.Sin(angle * Mathf.Deg2Rad);
            pos.y = center.y;
            pos.z = center.z + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        }
        else
        {
            pos.x = center.x + radius * -Mathf.Sin(angle * Mathf.Deg2Rad);
            pos.y = center.y + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            pos.z = center.z;
        }
        return pos;
    }
    #endregion

    #region Sections
    /// <summary>
    /// Display the "Number of Duplicates" section.
    /// </summary>
    protected void DisplayNumberOfDuplicatesSection()
    {
        isPrefab = PrefabUtility.GetPrefabInstanceHandle(selectedGameObject) != null;
        GUIContent selectedGameObjectContent = new GUIContent("Selected GameObject:", selectedGameObjectTooltip);
        selectedGameObject = (GameObject)EditorGUILayout.ObjectField(selectedGameObjectContent, selectedGameObject, typeof(GameObject), true);
        GUI.enabled = true;
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;
        GUILayout.Space(8);

        #region Duplicate Count
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = transformMode != TransformMode.Grid;
        DrawBulletPoint("#00E6BC");
        GUIContent duplicateCountContent = new GUIContent("Duplicate Count:", duplicateCountTooltip);
        if (transformMode == TransformMode.Grid)
        {
            int clampedDuplicateCount = Mathf.Clamp(gridSize.x * gridSize.y * gridSize.z, 1, 1000);
            duplicateCount = EditorGUILayout.IntSlider(duplicateCountContent, clampedDuplicateCount, 1, 1000);
        }
        else
        {
            duplicateCount = EditorGUILayout.IntSlider(duplicateCountContent, duplicateCount, 1, 1000);
        }

        if (GUILayout.Button("-"))
        {
            duplicateCount = Mathf.Clamp(duplicateCount - 1, 1, 1000);
        }
        if (GUILayout.Button("+"))
        {
            duplicateCount = Mathf.Clamp(duplicateCount + 1, 1, 1000);
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        #endregion

        #region Override Previous Duplicate(s)?
        EditorGUILayout.BeginHorizontal();
        DrawBulletPoint("#00E6BC");
        GUIContent overrideContent = new GUIContent("Override Duplicate(s)?", overrideDuplicatesTooltip);
        overridePreviousDuplicates = EditorGUILayout.Toggle(overrideContent, overridePreviousDuplicates);
        EditorGUILayout.EndHorizontal();
        #endregion

        #region Unpack Prefab(s)?
        GUI.enabled = isPrefab;
        EditorGUILayout.BeginHorizontal();
        DrawBulletPoint("#00E6BC");
        GUIContent unpackPrefabContent = new GUIContent("Unpack Prefab(s)?", unpackPrefabTooltip);
        unpackPrefab = EditorGUILayout.Toggle(unpackPrefabContent, unpackPrefab);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        #endregion
    }

    /// <summary>
    /// Display the "Naming Conventions" section.
    /// </summary>
    protected void DisplayNamingConventionsSection()
    {
        var namingMethods = new string[] { "Numbers", "Custom" };
        GUIContent namingMethodContent = new GUIContent("Naming Method:", namingMethodTooltip);
        GUI.backgroundColor = AddColor("#00BEFF");
        namingMethodType = (NamingMethod)EditorGUILayout.Popup(namingMethodContent, (int)namingMethodType, namingMethods);
        DrawPopupInfoBox(namingMethodTooltips[(int)namingMethodType], "#00BEFF");
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;
        GUILayout.Space(8);

        switch (namingMethodType)
        {
            case NamingMethod.Numbers:
                #region Number of Leading Digits
                EditorGUILayout.BeginHorizontal();
                GUIContent numOfLeadingDigitsContent = new GUIContent("Number of leading digits:", numOfLeadingDigitsSuffixTooltip);
                DrawBulletPoint("#00BEFF");
                numOfLeadingDigits = EditorGUILayout.IntSlider(numOfLeadingDigitsContent, numOfLeadingDigits, 0, 10);
                if (GUILayout.Button("-"))
                {
                    numOfLeadingDigits = Mathf.Clamp(numOfLeadingDigits - 1, 0, 10);
                }
                if (GUILayout.Button("+"))
                {
                    numOfLeadingDigits = Mathf.Clamp(numOfLeadingDigits + 1, 0, 10);
                }
                EditorGUILayout.EndHorizontal();
                #endregion
                #region Count From
                EditorGUILayout.BeginHorizontal();
                DrawBulletPoint("#00BEFF");
                GUIContent countFromContent = new GUIContent("Count from:", countFromTooltip);
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
                GUIContent incrementByContent = new GUIContent("Increment by:", incrementByTooltip);
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

                #region Add Space?
                EditorGUILayout.BeginHorizontal();
                GUIContent addSpaceContent = new GUIContent("Add Space?", addSuffixSpaceTooltip);
                DrawBulletPoint("#00BEFF");
                addSpace = EditorGUILayout.Toggle(addSpaceContent, addSpace);
                EditorGUILayout.EndHorizontal();
                #endregion

                #region Format Number with
                GUILayout.Space(12);
                DrawFormatNumberBox(ref addParentheses, ref addBrackets, ref addBraces, ref addUnderscore, ref addHyphen);
                #endregion
                #endregion
                break;
            case NamingMethod.Custom:
                #region Replace Full Name?
                GUIContent replaceFullNameContent = new GUIContent("Replace Full Name?", renameFullNameTooltip);
                replaceFullName = GUILayout.Toggle(replaceFullName, replaceFullNameContent);
                if (replaceFullName)
                {
                    #region Replace in Name
                    GUI.enabled = false;
                    EditorGUILayout.BeginHorizontal();
                    DrawBulletPoint("#00BEFF", 1);
                    string selectedName = selectedGameObject != null ? selectedGameObject.name : string.Empty;
                    GUIContent replaceInNameContent = new GUIContent("Replace in Name:", replaceInNameTooltip);
                    EditorGUILayout.TextField(replaceInNameContent, selectedName);
                    EditorGUILayout.EndHorizontal();
                    GUI.enabled = true;
                    #endregion
                    #region Replace With
                    EditorGUILayout.BeginHorizontal();
                    DrawBulletPoint("#00BEFF", 1);
                    GUIContent replacementNameContent = new GUIContent("Replace with:", replaceWithTooltip);
                    replacementName = EditorGUILayout.TextField(replacementNameContent, replacementName);
                    EditorGUILayout.EndHorizontal();
                    #endregion
                }
                #endregion

                DrawLine(GetColorFromHexString("#555555"), 1, 12f);

                #region Prefix
                EditorGUILayout.BeginHorizontal();
                DrawBulletPoint("#00BEFF");
                GUIContent prefixContent = new GUIContent("Prefix:", prefixTooltip);
                prefixName = EditorGUILayout.TextField(prefixContent, prefixName);
                EditorGUILayout.EndHorizontal();
                #endregion

                #region Add Space?
                EditorGUILayout.BeginHorizontal();
                GUIContent addPrefixSpaceContent = new GUIContent("Add Space?", addPrefixSpaceTooltip);
                DrawBulletPoint("#00BEFF");
                addPrefixSpace = EditorGUILayout.Toggle(addPrefixSpaceContent, addPrefixSpace);
                EditorGUILayout.EndHorizontal();
                #endregion

                #region Numerate Prefix
                GUIContent numeratePrefixContent = new GUIContent("Numerate Prefix?", numeratePrefixTooltip);
                numeratePrefix = GUILayout.Toggle(numeratePrefix, numeratePrefixContent);
                if (numeratePrefix)
                {
                    #region Number of Leading Digits
                    EditorGUILayout.BeginHorizontal();
                    GUIContent numOfLeadingDigitsPrefixContent = new GUIContent("Number of leading digits:", numOfLeadingDigitsPrefixTooltip);
                    DrawBulletPoint("#00BEFF", 1);
                    numOfLeadingDigitsPrefix = EditorGUILayout.IntSlider(numOfLeadingDigitsPrefixContent, numOfLeadingDigitsPrefix, 0, 10);
                    if (GUILayout.Button("-"))
                    {
                        numOfLeadingDigitsPrefix = Mathf.Clamp(numOfLeadingDigitsPrefix - 1, 0, 10);
                    }
                    if (GUILayout.Button("+"))
                    {
                        numOfLeadingDigitsPrefix = Mathf.Clamp(numOfLeadingDigitsPrefix + 1, 0, 10);
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion
                    #region Count From
                    EditorGUILayout.BeginHorizontal();
                    DrawBulletPoint("#00BEFF", 1);
                    GUIContent countFromPrefixContent = new GUIContent("Count from:", countFromTooltip);
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
                    GUIContent incrementByPrefixContent = new GUIContent("Increment by:", incrementByTooltip);
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

                    #region Format Number with
                    GUILayout.Space(12);
                    DrawFormatNumberBox(ref addParenthesesPrefix, ref addBracketsPrefix, ref addBracesPrefix,
                                        ref addUnderscorePrefix, ref addHyphenPrefix);
                    #endregion
                }
                #endregion

                DrawLine(GetColorFromHexString("#555555"), 1, 12f);

                #region Suffix
                EditorGUILayout.BeginHorizontal();
                DrawBulletPoint("#00BEFF");
                GUIContent suffixContent = new GUIContent("Suffix:", suffixTooltip);
                suffixName = EditorGUILayout.TextField(suffixContent, suffixName);
                EditorGUILayout.EndHorizontal();
                #endregion

                #region Add Space?
                EditorGUILayout.BeginHorizontal();
                GUIContent addSuffixSpaceContent = new GUIContent("Add Space?", addSuffixSpaceTooltip);
                DrawBulletPoint("#00BEFF");
                addSuffixSpace = EditorGUILayout.Toggle(addSuffixSpaceContent, addSuffixSpace);
                EditorGUILayout.EndHorizontal();
                #endregion

                #region Numerate Suffix
                GUIContent numerateSuffixContent = new GUIContent("Numerate Suffix?", numerateSuffixTooltip);
                numerateSuffix = GUILayout.Toggle(numerateSuffix, numerateSuffixContent);
                if (numerateSuffix)
                {
                    #region Number of Leading Digits
                    EditorGUILayout.BeginHorizontal();
                    GUIContent numOfLeadingDigitsSuffixContent = new GUIContent("Number of leading digits:", numOfLeadingDigitsSuffixTooltip);
                    DrawBulletPoint("#00BEFF", 1);
                    numOfLeadingDigitsSuffix = EditorGUILayout.IntSlider(numOfLeadingDigitsSuffixContent, numOfLeadingDigitsSuffix, 0, 10);
                    if (GUILayout.Button("-"))
                    {
                        numOfLeadingDigitsSuffix = Mathf.Clamp(numOfLeadingDigitsSuffix - 1, 0, 10);
                    }
                    if (GUILayout.Button("+"))
                    {
                        numOfLeadingDigitsSuffix = Mathf.Clamp(numOfLeadingDigitsSuffix + 1, 0, 10);
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion
                    #region Count From
                    EditorGUILayout.BeginHorizontal();
                    DrawBulletPoint("#00BEFF", 1);
                    GUIContent countFromSuffixContent = new GUIContent("Count from:", countFromTooltip);
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
                    GUIContent incrementBySuffixContent = new GUIContent("Increment by:", incrementByTooltip);
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

                    #region Format Number with
                    GUILayout.Space(12);
                    DrawFormatNumberBox(ref addParenthesesSuffix, ref addBracketsSuffix, ref addBracesSuffix,
                                        ref addUnderscoreSuffix, ref addHyphenSuffix);
                    #endregion
                }
                #endregion
                break;
        }

        DrawLine(GetColorFromHexString("#555555"), 1, 12f);

        #region Name Template
        // Displaye name template.
        templateName = string.Empty;
        if (namingMethodType == NamingMethod.Numbers)
        {
            templateName = GetNumericalTemplateName(countFromNumbers);
        }
        else
        {
            templateName = GetCustomTemplateName(countFromPrefix, countFromSuffix);
        }
        EditorGUILayout.HelpBox($"Name Template: {templateName}", MessageType.Info);
        GUI.backgroundColor = Color.white;
        #endregion
    }

    /// <summary>
    /// Display the "Group Under" section.
    /// </summary>
    protected void DisplayGroupUnderSection()
    {
        var text = new string[] { "None", "This", "Parent", "World", "New Group" };
        string groupNameTooltip = string.Empty;
        string groupName = "N/A";

        GUI.backgroundColor = Color.white;
        GUI.contentColor = Color.white;
        GUIContent groupUnderMainContent = new GUIContent("Group Duplicate(s) Under:", "Select which grouping method to group the duplicate(s) under.");
        GUIContent groupUnderContent = new GUIContent("Group Under:", groupUnderTooltip);
        GUIContent groupThisUnderContent = new GUIContent("Group This Under:", groupThisUnderTooltip);
        GUI.backgroundColor = AddColor("#B282FF");
        groupUnderNum = EditorGUILayout.Popup(groupUnderMainContent, groupUnderNum, text);
        GUI.backgroundColor = Color.white;

        DrawPopupInfoBox(groupUnderTypeTooltips[groupUnderNum], "#B282FF");
        GUILayout.Space(8);

        if (groupUnderNum == 0)
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            DrawBulletPoint("#B282FF");
            Transform parent = selectedGameObject != null && selectedGameObject.transform.parent != null ? selectedGameObject.transform.parent : null;
            GameObject parentObj = parent != null && parent.gameObject != null ? parent.gameObject : null;
            EditorGUILayout.ObjectField(groupUnderContent, parentObj, typeof(GameObject), true);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
        else if (groupUnderNum == 1)
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            DrawBulletPoint("#B282FF");
            groupName = selectedGameObject != null ? selectedGameObject.name : groupName;
            EditorGUILayout.ObjectField(groupUnderContent, selectedGameObject, typeof(GameObject), true);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
        else if (groupUnderNum == 2)
        {
            GUILayout.BeginHorizontal();
            GUIContent groupParentContent = new GUIContent("Group Parent:", "All duplicate(s) will be grouped under the group parent.");
            DrawBulletPoint("#B282FF");
            groupParent = (GameObject)EditorGUILayout.ObjectField(groupParentContent, groupParent, typeof(GameObject), true);
            groupName = groupParent != null ? groupParent.name : groupName;
            GUILayout.EndHorizontal();
        }
        else if (groupUnderNum == 3)
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            DrawBulletPoint("#B282FF");
            EditorGUILayout.ObjectField(groupUnderContent, null, typeof(GameObject), true);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
        else if (groupUnderNum == 4)
        {
            GUILayout.BeginHorizontal();
            GUIContent newGroupContent = new GUIContent("Group Name:", "The name of the new group.");
            DrawBulletPoint("#B282FF");
            newGroupName = EditorGUILayout.TextField(newGroupContent, newGroupName);
            groupName = newGroupName != string.Empty ? newGroupName : groupName;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawBulletPoint("#B282FF");
            groupRelative = (GameObject)EditorGUILayout.ObjectField(groupThisUnderContent, groupRelative, typeof(GameObject), true);
            GUILayout.EndHorizontal();
        }

        groupNameTooltip = $"Group Name: {groupName}";
        EditorGUILayout.HelpBox(groupNameTooltip, MessageType.Info);
        GUI.backgroundColor = Color.white;
    }

    /// <summary>
    /// Display the "Transform" section.
    /// </summary>
    protected void DisplayTransformSection()
    {
        // Get textures of both link icons (ON/OFF).
        var linkOnIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(linkOnIconPath, typeof(Texture2D));
        var linkOffIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(linkOffIconPath, typeof(Texture2D));

        GUI.backgroundColor = AddColor("#FD6D40");
        transformMode = (TransformMode)EditorGUILayout.Popup("Mode:", (int)transformMode, transformModes);
        DrawPopupInfoBox(transformModeTooltips[(int)transformMode], "#FD6D40");
        GUI.backgroundColor = Color.white;
        GUILayout.Space(8);

        GUILayout.BeginVertical(EditorStyles.helpBox);

        // Toggle wide mode.
        ToggleWideMode(12f);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedWidth = 24,
            fixedHeight = 24
        };

        switch (transformMode)
        {
            // Transform Mode: Line
            case TransformMode.Line:

                #region Translate
                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                Texture2D linkPositionIcon = linkPosition ? linkOnIcon : linkOffIcon;
                GUIContent positionContent = new GUIContent("Translate (Offset):", positionTooltip);
                GUIContent resetPositionContent = new GUIContent("↺", resetPositionTooltip);
                GUIContent linkPositionContent = new GUIContent(linkPositionIcon, linkPositionTooltip);
                GUILayout.Label(positionContent);
                linkPosition = GUILayout.Toggle(linkPosition, linkPositionContent, buttonStyle);

                float zPos = linkPosition ? positionProp.x : positionProp.z;
                GUIContent xPositionContent = new GUIContent("X", "X-position value");
                GUIContent yPositionContent = new GUIContent("Y", "Y-position value");
                GUIContent zPositionContent = new GUIContent("Z", "Z-position value");
                positionProp.x = DrawVectorComponent(xPositionContent, positionProp.x, "#FD6D40", true);
                positionProp.y = DrawVectorComponent(yPositionContent, linkPosition ? positionProp.x : positionProp.y, "#B1FD59", !linkPosition);
                positionProp.z = DrawVectorComponent(zPositionContent, zPos, "#7FD6FD", !linkPosition);

                isDefaultPosition = positionProp == Vector3.zero;
                GUI.backgroundColor = isDefaultPosition ? Color.white : AddColor("#70e04a");
                GUI.enabled = !isDefaultPosition;
                if (GUILayout.Button(resetPositionContent, buttonStyle))
                {
                    positionProp = Vector3.zero;
                }
                GUI.enabled = true;
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
                #endregion
                #region Rotate
                // Toggle wide mode.
                ToggleWideMode(12f);

                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                Texture2D linkRotationIcon = linkRotation ? linkOnIcon : linkOffIcon;
                GUIContent rotationContent = new GUIContent("Rotate (Offset):     ", rotationTooltip);
                GUIContent resetRotationContent = new GUIContent("↺", resetRotationTooltip);
                GUIContent linkRotationContent = new GUIContent(linkRotationIcon, linkRotationTooltip);
                GUILayout.Label(rotationContent);
                linkRotation = GUILayout.Toggle(linkRotation, linkRotationContent, buttonStyle);

                float zRot = linkRotation ? rotationProp.x : rotationProp.z;
                GUIContent xRotationContent = new GUIContent("X", "X-rotation value");
                GUIContent yRotationContent = new GUIContent("Y", "Y-rotation value");
                GUIContent zRotationContent = new GUIContent("Z", "Z-rotation value");
                rotationProp.x = DrawVectorComponent(xRotationContent, rotationProp.x, "#FD6D40", true);
                rotationProp.y = DrawVectorComponent(yRotationContent, linkRotation ? rotationProp.x : rotationProp.y, "#B1FD59", !linkRotation);
                rotationProp.z = DrawVectorComponent(zRotationContent, zRot, "#7FD6FD", !linkRotation);

                isDefaultRotation = rotationProp == Vector3.zero;
                GUI.backgroundColor = isDefaultRotation ? Color.white : AddColor("#70e04a");
                GUI.enabled = !isDefaultRotation;
                if (GUILayout.Button(resetRotationContent, buttonStyle))
                {
                    rotationProp = Vector3.zero;
                }
                GUI.enabled = true;
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
                #endregion
                #region Scale
                // Toggle wide mode.
                ToggleWideMode(12f);

                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                Texture2D linkScaleIcon = linkScale ? linkOnIcon : linkOffIcon;
                GUIContent scaleContent = new GUIContent("Scale (Offset):       ", scaleTooltip);
                GUIContent resetScaleContent = new GUIContent("↺", resetScaleTooltip);
                GUIContent linkScaleContent = new GUIContent(linkScaleIcon, linkScaleTooltip);
                GUILayout.Label(scaleContent);
                linkScale = GUILayout.Toggle(linkScale, linkScaleContent, buttonStyle);

                float zScale = linkScale ? scaleProp.x : scaleProp.z;
                GUIContent xScaleContent = new GUIContent("X", "X-scale value");
                GUIContent yScaleContent = new GUIContent("Y", "Y-scale value");
                GUIContent zScaleContent = new GUIContent("Z", "Z-scale value");
                scaleProp.x = DrawVectorComponent(xScaleContent, scaleProp.x, "#FD6D40", true);
                scaleProp.y = DrawVectorComponent(yScaleContent, linkScale ? scaleProp.x : scaleProp.y, "#B1FD59", !linkScale);
                scaleProp.z = DrawVectorComponent(zScaleContent, zScale, "#7FD6FD", !linkScale);

                isDefaultScale = scaleProp == Vector3.zero;
                GUI.backgroundColor = isDefaultScale ? Color.white : AddColor("#70e04a");
                GUI.enabled = !isDefaultScale;
                if (GUILayout.Button(resetScaleContent, buttonStyle))
                {
                    scaleProp = Vector3.zero;
                }
                GUI.enabled = true;
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
                #endregion
                break;
            // Transform Mode: Grid
            case TransformMode.Grid:
                #region Grid Size
                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                Texture2D linkGridSizeIcon = linkGridSize ? linkOnIcon : linkOffIcon;
                GUIContent gridSizeContent = new GUIContent("Grid Size:               ", gridSizeTooltip);
                GUIContent resetGridSizeContent = new GUIContent("↺", resetGridSizeTooltip);
                GUIContent linkGridSizeContent = new GUIContent(linkGridSizeIcon, linkGridSizeTooltip);
                GUILayout.Label(gridSizeContent);
                linkGridSize = GUILayout.Toggle(linkGridSize, linkGridSizeContent, buttonStyle);

                int zSize = linkGridSize ? gridSize.x : gridSize.z;
                GUIContent xSizeContent = new GUIContent("X", "X-size value");
                GUIContent ySizeContent = new GUIContent("Y", "Y-size value");
                GUIContent zSizeContent = new GUIContent("Z", "Z-size value");

                gridSize.x = DrawVectorIntComponent(xSizeContent, gridSize.x, "#FD6D40", true);
                gridSize.y = DrawVectorIntComponent(ySizeContent, linkGridSize ? gridSize.x : gridSize.y, "#B1FD59", !linkGridSize);
                gridSize.z = DrawVectorIntComponent(zSizeContent, zSize, "#7FD6FD", !linkGridSize);

                isDefaultGridSize = gridSize == Vector3Int.one;
                GUI.backgroundColor = isDefaultGridSize ? Color.white : AddColor("#70e04a");
                GUI.enabled = !isDefaultGridSize;
                if (GUILayout.Button(resetGridSizeContent, buttonStyle))
                {
                    gridSize = Vector3Int.one;
                }
                GUI.enabled = true;
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
                #endregion
                #region Grid Spacing
                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                Texture2D linkGridSpacingIcon = linkGridSpacing ? linkOnIcon : linkOffIcon;
                GUIContent gridSpacingContent = new GUIContent("Grid Spacing:        ", gridSpacingTooltip);
                GUIContent resetGridSpacingContent = new GUIContent("↺", resetGridSpacingTooltip);
                GUIContent linkGridSpacingContent = new GUIContent(linkGridSpacingIcon, linkGridSpacingTooltip);
                GUILayout.Label(gridSpacingContent);
                linkGridSpacing = GUILayout.Toggle(linkGridSpacing, linkGridSpacingContent, buttonStyle);

                float zSpacing = linkGridSpacing ? gridSpacing.x : gridSpacing.z;
                GUIContent xSpacingContent = new GUIContent("X", "X-spacing value");
                GUIContent ySpacingContent = new GUIContent("Y", "Y-spacing value");
                GUIContent zSpacingContent = new GUIContent("Z", "Z-spacing value");
                gridSpacing.x = DrawVectorComponent(xSpacingContent, gridSpacing.x, "#FD6D40", true);
                gridSpacing.y = DrawVectorComponent(ySpacingContent, linkGridSpacing ? gridSpacing.x : gridSpacing.y, "#B1FD59", !linkGridSpacing);
                gridSpacing.z = DrawVectorComponent(zSpacingContent, zSpacing, "#7FD6FD", !linkGridSpacing);

                isDefaultGridSpacing = gridSpacing == Vector3.zero;
                GUI.backgroundColor = isDefaultGridSpacing ? Color.white : AddColor("#70e04a");
                GUI.enabled = !isDefaultGridSpacing;
                if (GUILayout.Button(resetGridSpacingContent, buttonStyle))
                {
                    gridSpacing = Vector3.zero;
                }
                GUI.enabled = true;
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
                #endregion
                break;
            // Transform Mode: Circle
            case TransformMode.Circle:
                // Toggle wide mode.
                ToggleWideMode(180f);

                #region Radius
                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                GUIContent radiusContent = new GUIContent("Radius:", radiusTooltip);
                radialDistance = EditorGUILayout.Slider(radiusContent, radialDistance, 0f, 100f);
                GUILayout.EndHorizontal();
                #endregion

                #region Degree Measure
                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                GUIContent areaContent = new GUIContent("Degree Measure:", degreeMeasureTooltip);
                degreeMeasure = (DegreeMeasure)EditorGUILayout.Popup(areaContent, (int)degreeMeasure, degreeMeasureOptions);
                GUILayout.EndHorizontal();
                #endregion

                #region Orientation
                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                GUIContent orientationContent = new GUIContent("Orientation:", circleOrientationTooltip);
                circleOrientation = (Orientation)EditorGUILayout.EnumPopup(orientationContent, circleOrientation);
                GUILayout.EndHorizontal();
                #endregion

                #region Look At Center?
                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                GUIContent lookAtCenterContent = new GUIContent("Look At Center?", lookAtCenterTooltip);
                lookAtCenter = EditorGUILayout.Toggle(lookAtCenterContent, lookAtCenter);
                GUILayout.EndHorizontal();
                #endregion
                break;
            // Transform Mode: Spiral
            case TransformMode.Spiral:
                // Toggle wide mode.
                ToggleWideMode(180f);

                #region Radius
                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                GUIContent spiralRadiusContent = new GUIContent("Radius:", spiralRadiusTooltip);
                spiralRadius = EditorGUILayout.Slider(spiralRadiusContent, spiralRadius, 0f, 100f);
                GUILayout.EndHorizontal();
                #endregion

                #region Curve Amount
                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                GUIContent curveAmountContent = new GUIContent("Curve Amount:", curveAmountTooltip);
                curveAmount = EditorGUILayout.Slider(curveAmountContent, curveAmount, 0f, 100f);
                GUILayout.EndHorizontal();
                #endregion

                #region Height
                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                GUIContent spiralHeightContent = new GUIContent("Height:", spiralHeightTooltip);
                spiralHeight = EditorGUILayout.Slider(spiralHeightContent, spiralHeight, 0f, 100f);
                GUILayout.EndHorizontal();
                #endregion

                #region Orientation
                GUILayout.BeginHorizontal();
                DrawBulletPoint("#FD6D40");
                GUIContent spiralOrientationContent = new GUIContent("Orientation:", spiralOrientationTooltip);
                spiralOrientation = (Orientation)EditorGUILayout.EnumPopup(spiralOrientationContent, spiralOrientation);
                GUILayout.EndHorizontal();
                #endregion
                break;
            // Transform Mode: Random
            case TransformMode.Random:
                GUILayout.Space(12);

                // Toggle wide mode.
                ToggleWideMode(180f);

                #region Randomize Position?
                GUIContent randomizePositionContent = new GUIContent("Randomize Position?", randomizePositionTooltip);
                randomizePosition = GUILayout.Toggle(randomizePosition, randomizePositionContent);
                if (randomizePosition)
                {
                    #region Distance Range
                    GUILayout.BeginHorizontal();
                    // Calculate minimum distance & clamp it to prevent its value from going over the maximum distance.
                    minDistance = Mathf.Clamp(Mathf.Round(minDistance), 0f, maxDistance);
                    // Calculate maximum distance & clamp it to prevent its value from going under the minimum distance.
                    maxDistance = Mathf.Clamp(Mathf.Round(maxDistance), minDistance, 1000);

                    DrawBulletPoint("#FD6D40", 1);
                    GUIContent distanceRangeContent = new GUIContent("Distance Range:", distanceRangeTooltip);
                    GUILayout.Label(distanceRangeContent);
                    minDistance = EditorGUILayout.FloatField(minDistance);
                    EditorGUILayout.MinMaxSlider(ref minDistance, ref maxDistance, 0f, 1000f);
                    maxDistance = EditorGUILayout.FloatField(maxDistance);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Lock Position (X)?
                    GUILayout.BeginHorizontal();
                    DrawBulletPoint("#FD6D40", 1);
                    GUIContent lockPositionXContent = new GUIContent("Lock Position (X)?", lockPositionXTooltip);
                    lockPositionX = EditorGUILayout.Toggle(lockPositionXContent, lockPositionX);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Lock Position (Y)?
                    GUILayout.BeginHorizontal();
                    DrawBulletPoint("#FD6D40", 1);
                    GUIContent lockPositionYContent = new GUIContent("Lock Position (Y)?", lockPositionYTooltip);
                    lockPositionY = EditorGUILayout.Toggle(lockPositionYContent, lockPositionY);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Lock Position (Z)?
                    GUILayout.BeginHorizontal();
                    DrawBulletPoint("#FD6D40", 1);
                    GUIContent lockPositionZContent = new GUIContent("Lock Position (Z)?", lockPositionZTooltip);
                    lockPositionZ = EditorGUILayout.Toggle(lockPositionZContent, lockPositionZ);
                    GUILayout.EndHorizontal();
                    #endregion
                }
                #endregion

                DrawLine(GetColorFromHexString("#555555"), 1, 12f);

                #region Randomize Rotation?
                GUIContent randomizeRotationContent = new GUIContent("Randomize Rotation?", randomizeRotationTooltip);
                randomizeRotation = GUILayout.Toggle(randomizeRotation, randomizeRotationContent);
                if (randomizeRotation)
                {
                    #region Lock Pitch (X)?
                    GUILayout.BeginHorizontal();
                    DrawBulletPoint("#FD6D40", 1);
                    GUIContent lockPitchContent = new GUIContent("Lock Pitch (X)?", lockRotationXTooltip);
                    lockPitch = EditorGUILayout.Toggle(lockPitchContent, lockPitch);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Lock Yaw (Y)?
                    GUILayout.BeginHorizontal();
                    DrawBulletPoint("#FD6D40", 1);
                    GUIContent lockYawContent = new GUIContent("Lock Yaw (Y)?", lockRotationYTooltip);
                    lockYaw = EditorGUILayout.Toggle(lockYawContent, lockYaw);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Lock Roll (Z)?
                    GUILayout.BeginHorizontal();
                    DrawBulletPoint("#FD6D40", 1);
                    GUIContent lockRollContent = new GUIContent("Lock Roll (Z)?", lockRotationZTooltip);
                    lockRoll = EditorGUILayout.Toggle(lockRollContent, lockRoll);
                    GUILayout.EndHorizontal();
                    #endregion
                }
                #endregion

                DrawLine(GetColorFromHexString("#555555"), 1, 12f);

                #region Randomize Scale?
                GUIContent randomizeScaleContent = new GUIContent("Randomize Scale?", randomizeScaleTooltip);
                randomizeScale = GUILayout.Toggle(randomizeScale, randomizeScaleContent);
                if (randomizeScale)
                {
                    #region Scale Range
                    GUILayout.BeginHorizontal();
                    // Calculate minimum scale & clamp it to prevent its value from going over the maximum scale.
                    minScale = Mathf.Clamp(minScale, 0f, maxScale);
                    // Calculate maximum scale & clamp it to prevent its value from going under the minimum scale.
                    maxScale = Mathf.Clamp(maxScale, minScale, absoluteMaxScaleValue);

                    DrawBulletPoint("#FD6D40", 1);
                    GUIContent scaleRangeContent = new GUIContent("Scale Range:", scaleRangeTooltip);
                    GUILayout.Label(scaleRangeContent);
                    minScale = EditorGUILayout.FloatField(minScale);
                    EditorGUILayout.MinMaxSlider(ref minScale, ref maxScale, 0f, absoluteMaxScaleValue);
                    maxScale = EditorGUILayout.FloatField(maxScale);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Lock Scale (X)?
                    GUILayout.BeginHorizontal();
                    DrawBulletPoint("#FD6D40", 1);
                    GUIContent lockScaleXContent = new GUIContent("Lock Scale (X)?", lockScaleXTooltip);
                    lockScaleX = EditorGUILayout.Toggle(lockScaleXContent, lockScaleX);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Lock Scale (Y)?
                    GUILayout.BeginHorizontal();
                    DrawBulletPoint("#FD6D40", 1);
                    GUIContent lockScaleYContent = new GUIContent("Lock Scale (Y)?", lockScaleYTooltip);
                    lockScaleY = EditorGUILayout.Toggle(lockScaleYContent, lockScaleY);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Lock Scale (Z)?
                    GUILayout.BeginHorizontal();
                    DrawBulletPoint("#FD6D40", 1);
                    GUIContent lockScaleZContent = new GUIContent("Lock Scale (Z)?", lockScaleZTooltip);
                    lockScaleZ = EditorGUILayout.Toggle(lockScaleZContent, lockScaleZ);
                    GUILayout.EndHorizontal();
                    #endregion
                }
                #endregion

                GUILayout.Space(12);
                break;
        }
        GUILayout.EndVertical();
    }

    #region Other
    /// <summary>
    /// Expand all sections in the editor window.
    /// </summary>
    private void ExpandAllSections()
    {
        showNumberOfCopiesSection = true;
        showNamingConventionsSection = true;
        showGroupUnderSection = true;
        showTransformSection = true;
    }

    /// <summary>
    /// Collapse all sections in the editor window.
    /// </summary>
    private void CollapseAllSections()
    {
        showNumberOfCopiesSection = false;
        showNamingConventionsSection = false;
        showGroupUnderSection = false;
        showTransformSection = false;
    }

    /// <summary>
    /// Open the official documentation on how to use the Duplicate Special Tool.
    /// </summary>
    private void OpenDocumentation()
    {
        Application.OpenURL("https://docs.google.com/document/d/1OYRyDVw_OaqRfMgRfAHvz5oz9ETszxRQ8XC0xVTDda8/edit#");
    }
    #endregion
    #endregion

    #region Draw Window Element Method(s)
    private float DrawVectorComponent(GUIContent guiContent, float value, string boxColor, params bool[] conditions)
    {
        bool isEnabled = true;
        foreach (bool condition in conditions)
        {
            if (condition)
                continue;

            isEnabled = false;
            break;
        }

        GUI.backgroundColor = isEnabled ? AddColor(boxColor) * 1.1f : AddColor(boxColor) * 0.75f;
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUI.enabled = isEnabled;
        GUI.contentColor = isEnabled ? AddColor(Color.white) : Color.white;
        value = EditorGUILayout.FloatField(guiContent, value);
        GUI.contentColor = Color.white;
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;

        return value;
    }

    private int DrawVectorIntComponent(GUIContent guiContent, int value, string boxColor, params bool[] conditions)
    {
        bool isEnabled = true;
        foreach (bool condition in conditions)
        {
            if (condition)
                continue;

            isEnabled = false;
            break;
        }

        GUI.backgroundColor = isEnabled ? AddColor(boxColor) * 1.1f : AddColor(boxColor) * 0.75f;
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUI.enabled = isEnabled;
        GUI.contentColor = isEnabled ? AddColor(Color.white) : Color.white;
        value = EditorGUILayout.IntField(guiContent, Mathf.Clamp(value, 1, 10));
        GUI.contentColor = Color.white;
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;

        return value;
    }

    protected void DrawSection(string header, ref bool foldout, UnityAction ue, string iconPath, Color boxColor)
    {
        var icon = (Texture2D)AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D));
        EditorGUIUtility.SetIconSize(new Vector2(28f, 28f));

        GUI.backgroundColor = boxColor;
        GUI.contentColor = Color.white;
        GUIContent content = new GUIContent(header, icon);
        GUIStyle headerStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = 32f,
            stretchWidth = true,
            wordWrap = false,
            clipping = TextClipping.Clip
        };

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
    /// Draw "Format number with:" box w/ multiple options.
    /// </summary>
    private void DrawFormatNumberBox(ref bool addParenthesesOption, ref bool addBracketsOption, ref bool addBracesOption,
                                     ref bool addUnderscoreOption, ref bool addHyphenOption)
    {
        GUILayout.Label("Format number with:");
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        addParenthesesOption = GUILayout.Toggle(addParenthesesOption, " Parentheses \"()\"", EditorStyles.radioButton);
        if (addParenthesesOption)
        {
            addBracketsOption = false;
            addBracesOption = false;
            addUnderscoreOption = false;
            addHyphenOption = false;
        }
        addBracketsOption = GUILayout.Toggle(addBracketsOption, " Brackets \"[]\"", EditorStyles.radioButton);
        if (addBracketsOption)
        {
            addParenthesesOption = false;
            addBracesOption = false;
            addUnderscoreOption = false;
            addHyphenOption = false;
        }
        addBracesOption = GUILayout.Toggle(addBracesOption, " Braces \"{}\"", EditorStyles.radioButton);
        if (addBracesOption)
        {
            addParenthesesOption = false;
            addBracketsOption = false;
            addUnderscoreOption = false;
            addHyphenOption = false;
        }
        addUnderscoreOption = GUILayout.Toggle(addUnderscoreOption, " Underscore \"_\"", EditorStyles.radioButton);
        if (addUnderscoreOption)
        {
            addParenthesesOption = false;
            addBracketsOption = false;
            addBracesOption = false;
            addHyphenOption = false;
        }
        addHyphenOption = GUILayout.Toggle(addHyphenOption, " Hyphen \"-\"", EditorStyles.radioButton);
        if (addHyphenOption)
        {
            addParenthesesOption = false;
            addBracketsOption = false;
            addBracesOption = false;
            addUnderscoreOption = false;
        }
        EditorGUILayout.EndHorizontal();
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
    /// Draw popup information box.
    /// </summary>
    /// <param name="text">Information text.</param>
    /// <param name="boxColor">Box color string (Hexadecimal).</param>
    private void DrawPopupInfoBox(string text, string boxColor)
    {
        GUI.backgroundColor = GetColorFromHexString(boxColor);
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.Label(text);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
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
            fixedWidth = 12 + (24 * indents),
            contentOffset = new Vector2(24 * indents, 0f)
        };

        // Draw bullet point w/ the specified color.
        GUI.contentColor = GetColorFromHexString(bulletPointColor);
        GUILayout.Label("•", bulletPointStyle);
        GUI.contentColor = Color.white;
    }
    #endregion

    #region Miscellaneous
    /// <summary>
    /// Toggle wide mode for the Editor GUI.
    /// </summary>
    /// <param name="nonWideModeWidth">Minimum width (in pixels) for all labels.</param>
    private void ToggleWideMode(float labelWidth)
    {
        EditorGUIUtility.wideMode = !EditorGUIUtility.wideMode;

        // Set the minimum width (in pixels) for all fields.
        EditorGUIUtility.fieldWidth = 72;
        EditorGUIUtility.labelWidth = labelWidth;
    }

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
