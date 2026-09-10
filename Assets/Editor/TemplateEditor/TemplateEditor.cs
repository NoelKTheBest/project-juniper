/*
 * By the end of the week I should have all basic operations finished.
 * Adding transitions, behaviours, adding the new state machine to the template and saving it to json
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
using AnimatorUtilities;
using TemplateUtilities;

public enum ChangesType
{
    states,
    stateMachines,
    all,
}

public class TemplateEditor : EditorWindow, IPointerEnterHandler, IPointerExitHandler
{
    // I'm sepcifying two paths here since, I'm not sure which one to pick and both seem pretty valid options
    string gamePath;
    string appPath;

    [MenuItem("Window/Template Editor")]
    public static void ShowWindow()
    {
        TemplateEditor window = EditorWindow.GetWindow<TemplateEditor>(false);
        window.minSize = new Vector2(600.0f, 300.0f);
        window.wantsMouseMove = true;
        window.Show();

        EditorWindow.FocusWindowIfItsOpen<TemplateEditor>();
    }

    //---Option Variables---
    bool templateSelected = false;
    bool stateMachineSelected = false;
    bool stateSelected = false;
    bool createState = false;
    bool createStateMachine = false;
    bool createTransition = false;
    bool optionSelected = false;
    bool useGameFolder = false;
    bool changeTemplateName = false;
    bool changeStateMachineName = false;
    bool changeStateName = false;
    bool changeMotion = false;
    bool deleteStateMachine = false;
    bool deleteState = false;

    int templateIndex = 0;
    string stateName = "";
    string stateMotion = "";
    string stateMachineName = "";
    string genericInput = "";
    string chosenState;
    string currentStateMachine;
    string referredState;
    string stateMachineToDelete;

    // Path to the template file or folder to save the file
    string templatePath;

    //---Properties---
    [SerializeField]
    State newState;
    [SerializeField]
    StateMachine newStateMachine;
    [SerializeField]
    State state;
    [SerializeField]
    StateMachine stateMachine;
    [SerializeField]
    Template template;
    [SerializeField]
    List<string> dirtyStateMachines = new List<string>();
    List<string> dirtyStates = new List<string>();
    
    //---Errors---
    bool nameError = false;

    //---Other---
    string tab = "    "; 
    int tabCount = 0;
    Vector2 scroll;

    Action menuOption;

    private void OnEnable()
    {
        gamePath = Application.dataPath + "/Templates/Data";
        appPath = Application.persistentDataPath + "/TemplateData";
        dirtyStates.Clear();
        dirtyStateMachines.Clear();
        createStateMachine = false;
        createTransition = false;
        optionSelected = false;
    }
    
    // Use a Scrollview
    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (!templateSelected && !stateMachineSelected/* || template != null*/)
        {
            //------------Display directory Content-------------
            useGameFolder = GUILayout.Toggle(useGameFolder, "Use Game Folder");

            if (useGameFolder) DisplayFiles(gamePath);
            else DisplayFiles(appPath);

            EditorGUILayout.LabelField(" ");

            //if (template != null) Debug.Log(template.name);

            //------------Template Name--------------------------
            if (template != null && template.name != null) EditorGUILayout.LabelField("Current Template: " + template.name);

            //------------Create, Edit, Delete Buttons-----------
            if (template == null && GUILayout.Button("Create Template"))
            {
                ResetErrors();
                ResetVariables();
                template = new Template();
                //template.name = "";
                //templateSelected = true;
            }
            
            if (template != null)
            {
                if (template.name == "")
                {
                    InputField("New Template Name", "Add name", "Name must have more than 0 characters");
                }
            }

            EditorGUILayout.LabelField(" ");

            if (template != null && GUILayout.Button("Edit"))
            {
                templateSelected = true;
                ResetVariables();
            }
            if (template != null && GUILayout.Button("Deselect"))
            {
                template = null;
                templateSelected = false;
            }
        }
        else if (templateSelected && !stateMachineSelected)
        {
            //-----------------Template Display-------------------
            if (GUILayout.Button("Back")) templateSelected = false;
            if (GUILayout.Button(template.name, LabelStyle()))
            {
                if (!changeTemplateName)
                {
                    changeTemplateName = true;
                }
                else
                {
                    changeTemplateName = false;
                }
            }
            
            if (changeTemplateName) template.name = EditorGUILayout.TextField(template.name);

            EditorGUILayout.LabelField(" ");

            if (template != null) DisplayTemplate();

            if (GUILayout.Button("Save Template"))
            {
                if (useGameFolder) SaveTemplate(gamePath);
                else SaveTemplate(appPath);
            }

            if (GUILayout.Button("Create Controller")) {
                Debug.Log("Controller Created");
                // AnimatorUtilities.
                AnimatorControllerUtility.CreateAnimatorController(gamePath, "Hello World Again!");
            }
        }
        else if (stateMachineSelected)
        {
            if (GUILayout.Button("Back"))
            {
                stateMachineSelected = false;
                stateSelected = false;

                // Reset State Machine Menus
                createState = false;
                createStateMachine = false;
                createTransition = false;
                optionSelected = false;
                ResetErrors();
                ResetVariables();
            }

            if (GUILayout.Button(stateMachine.stateMachineName, LabelStyle()))
            {
                if (!changeStateMachineName)
                {
                    changeStateMachineName = true;
                }
                else
                {
                    changeStateMachineName = false;
                }
            }

            if (changeStateMachineName) stateMachine.stateMachineName = EditorGUILayout.TextField(stateMachine.stateMachineName);
            //EditorGUILayout.LabelField(currentStateMachine);

            EditorGUILayout.LabelField(" ");
            
            AddOptions();
        }
        EditorGUILayout.EndScrollView();
    }

    #region Displays
    void DisplayFiles(string path)
    {
        int i = 0;
        string[] files = Directory.GetFileSystemEntries(path);

        foreach (string file in files)
        {
            i++;
            string[] s = file.Split('/', '\\');

            string fileName = s[s.Length - 1];
            if (!fileName.Contains(".meta"))
            {
                fileName = fileName.Split('.')[0];
                if (GUILayout.Button(fileName, LabelStyle()))
                {
                    if (template != null)
                    {
                        if (templateIndex != i)
                        {
                            template = JSONTemplateUtilities.FromJSON(path + "/" + fileName + ".json");
                            templateIndex = i;
                            //templateSelected = true;
                        }
                        else if (templateIndex == i)
                        {
                            template = null;
                            templateIndex = 0;
                        }
                    }
                    else
                    {
                        template = JSONTemplateUtilities.FromJSON(path + "/" + fileName + ".json");
                        templateIndex = i;
                    }
                }
            }
        }
    }

    void DisplayTemplate()
    {
        tabCount = 0;
        TraverseTemplate(template.rootStateMachine);
    }

    void TraverseTemplate(StateMachine sm)
    {
        string dirtyString = "";
        if (dirtyStateMachines.Contains(sm.stateMachineName)) dirtyString = "*";
        
        if (GUILayout.Button(Tab() + sm.stateMachineName + dirtyString, LabelStyle()))
        {
            stateMachineSelected = true;
            currentStateMachine = sm.stateMachineName;
            stateMachine = sm;
        }
        tabCount++;
        foreach (StateMachine stateMachine in sm.subStateMachines)
        {
            if (stateMachine.stateMachineName == stateMachineToDelete)
            {
                sm.subStateMachines.Remove(stateMachine);
                dirtyStateMachines.Add(sm.stateMachineName);
                break;
            }
            TraverseTemplate(stateMachine);
            tabCount--;
        }
    }

    string Tab()
    {
        string s = "";

        for (int i = 0; i < tabCount; i++)
        {
            s = s + tab;
        }

        return s;
    }

    GUIStyle LabelStyle()
    {
        var style = new GUIStyle();
        var b = style.border;
        var p = style.padding;
        
        // Remove Border
        b.left = 0;
        b.right = 0;
        b.top = 0;
        b.bottom = 0;

        // Add Padding
        p.left = 5;
        p.right = 0;
        p.top = 3;
        p.bottom = 3;

        // Add hover style
        //GUI.skin.button.onHover.textColor = new Color(150, 150, 150);
        style.onHover.textColor = new Color(150, 150, 150);

        return style;
    }

    GUIStyle WarningStyle()
    {
        var style = new GUIStyle();

        style.normal.textColor = Color.red;

        return style;
    }

    void InputField(string labelName, string buttonText, string errorText)
    {
        // Create local variable to store results of TextArea before saving to genericInput
        //string input = "";

        EditorGUILayout.LabelField(labelName);
        genericInput = EditorGUILayout.TextArea(genericInput);

        if (GUILayout.Button(buttonText))
        {
            if (genericInput != "")
            {
                nameError = false;
                template.name = genericInput;
            }
            else
            {
                nameError = true;
            }
        }

        if (nameError) EditorGUILayout.LabelField(errorText);
    }
    #endregion

    #region Menus
    bool AddOption(bool optionVisibility, string optionText, Action action)
    {
        if (!optionVisibility && !optionSelected)
        {
            if (GUILayout.Button(optionText))
            {
                optionVisibility = true;
                optionSelected = true;
            }
        }
        else if (optionVisibility)
        {
            if (GUILayout.Button("Cancel"))
            {
                optionVisibility = false;
                optionSelected = false;
                ResetErrors();
                ResetVariables();
            }

            menuOption = action;
            menuOption();
        }

        return optionVisibility;
    }

    void AddOptions()
    {
        if (!stateSelected)
        {
            DisplayStates();

            createState = AddOption(createState, "Add State", AddStateMenu);
            createStateMachine = AddOption(createStateMachine, "Add State Machine", AddStateMachineMenu);
            createTransition = AddOption(createTransition, "Add Transition", AddTransitionMenu);
            deleteStateMachine = AddOption(deleteStateMachine, "Delete", DeleteStateMachine);
        }
        else
        {
            StateMenu();
        }
    }

    void DisplayStates()
    {
        foreach (State s in stateMachine.states)
        {
            string dirtyString = "";
            if (dirtyStates.Contains(s.stateName)) dirtyString = "*";

            if (GUILayout.Button(s.stateName + dirtyString, LabelStyle()))
            {
                stateSelected = true;
                referredState = s.stateName;
                state = s;
            }
        }
    }

    void StateMenu()
    {
        //EditorGUILayout.LabelField("----------------------------");

        if (GUILayout.Button("Back"))
        {
            stateSelected = false;
        }

        //------------------Change State Name----------------------
        if (GUILayout.Button(state.stateName, LabelStyle()))
        {
            if (!changeStateName)
            {
                changeStateName = true;
            }
            else
            {
                changeStateName = false;
            }
        }

        /// BUG: Right here the field state.stateName does not get updated automatically in the Unity Editor
        if (changeStateName) state.stateName = EditorGUILayout.TextField(state.stateName);
        
        EditorGUILayout.LabelField(" ");

        //-------------------Change Motion-------------------------
        if (!changeMotion && !optionSelected)
        {
            if (GUILayout.Button("Motion"))
            {
                changeMotion = true;
                optionSelected = true;
            }
        }
        else if (changeMotion)
        {
            EditorGUILayout.LabelField("New Motion");
            genericInput = EditorGUILayout.TextArea(genericInput);

            if (GUILayout.Button("Apply"))
            {
                if (genericInput != "")
                {
                    nameError = false;
                    state.stateMotion = genericInput;
                }
                else
                {
                    nameError = true;
                }
            }

            if (GUILayout.Button("Cancel"))
            {
                changeMotion = false;
                optionSelected = false;
            }

            if (nameError) EditorGUILayout.LabelField("Motion name must have more than 0 characters");
            //InputField("New Motion", "Apply", "Motion name must have more than 0 characters");
        }

        //------------------Edit transitions-----------------------
        if (!createTransition && !optionSelected)
        {
            if (GUILayout.Button("Transitions"))
            {
                createTransition = true;
                optionSelected = true;
            }
        }
        else if (createTransition)
        {
            if (GUILayout.Button("Back"))
            {
                createTransition = false;
                optionSelected = false;
                ResetErrors();
                ResetVariables();
            }

            AddTransitionMenu();
        }

        //--------------------Delete the state---------------------
        if (!deleteState && !optionSelected)
        {
            if (GUILayout.Button("Delete"))
            {
                deleteState = true;
                optionSelected = true;
            }
        }
        else if (deleteState)
        {
            if (GUILayout.Button("Cancel"))
            {
                deleteState = false;
                optionSelected = false;
            }
            
            EditorGUILayout.LabelField("Are you sure you want to delete this state?", WarningStyle());
            
            if (GUILayout.Button("Confirm"))
            {
                //Debug.Log("Delete " + referredState);
                optionSelected = false;
                deleteState = false;
                stateSelected = false;
                stateMachine.states.Remove(state);
                dirtyStateMachines.Add(currentStateMachine);
            }
        }
    }

    void AddStateMenu()
    {
        EditorGUILayout.LabelField("Name: ");
        stateName = EditorGUILayout.TextArea(stateName);
        EditorGUILayout.LabelField("Motion: ");
        stateMotion = EditorGUILayout.TextArea(stateMotion);

        if (GUILayout.Button("Add"))
        {
            if (stateName != "")
            {
                nameError = false;
                newState = new State(stateName, stateMotion);
                referredState = stateName;
                EditTemplate(ChangesType.states);
                //createState = false;
            }
            else
            {
                nameError = true;
            }
        }

        if (nameError) EditorGUILayout.LabelField("Every state must have a name");
    }
    
    // I need an overloaded method for state machines
    void AddTransitionMenu()
    {
        // Choose State
        if (EditorGUILayout.DropdownButton(new GUIContent("Choose State"), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();
            foreach(State state in stateMachine.states)
            {
                menu.AddItem(new GUIContent(state.stateName), false, OnItemSelected, state.stateName);
            }
            menu.ShowAsContext();
        }

        // Choose Parameter
        if (EditorGUILayout.DropdownButton(new GUIContent("Choose Parameters"), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddDisabledItem(new GUIContent("isMoving"));
            //menu.AddItem(new GUIContent("item"), false, OnItemSelected);
            //menu.AddSeparator("");
            //menu.AddItem(new GUIContent("another item"), false, OnAnotherItemSelected);
            //menu.AddSeparator("");
            //menu.AddDisabledItem(new GUIContent("disabled item"));
            menu.ShowAsContext();
        }

        EditorGUILayout.LabelField("Destination State: " + chosenState);
        //EditorGUILayout.LabelField("Parameters: " + );
    }

    void AddBehaviourMenu()
    {
        // Display Behaviour Menu
    }

    void AddStateMachineMenu()
    {
        EditorGUILayout.LabelField("Name: ");
        stateMachineName = EditorGUILayout.TextArea(stateMachineName);

        if (GUILayout.Button("Add"))
        {
            if (stateMachineName != "")
            {
                nameError = false;
                newStateMachine = new StateMachine(stateMachineName);
                EditTemplate(ChangesType.stateMachines);
                stateMachineSelected = false;
            }
            else
            {
                nameError = true;
            }
        }

        if (nameError) EditorGUILayout.LabelField("Every state machine must have a name");
    }

    void DeleteStateMachine()
    {
        if (GUILayout.Button("Cancel"))
        {
            deleteStateMachine = false;
            optionSelected = false;
        }

        EditorGUILayout.LabelField("Are you sure you want to delete this state machine?", WarningStyle());

        if (GUILayout.Button("Confirm"))
        {
            optionSelected = false;
            deleteStateMachine = false;
            stateMachineSelected = false;
            stateMachineToDelete = stateMachine.stateMachineName;
            //stateMachine.subStateMachines.Remove(stateMachine);
            //dirtyStateMachines.Add(currentStateMachine);
        }
    }
    #endregion

    #region Reset
    void ResetErrors()
    {
        nameError = false;
    }

    void ResetVariables()
    {
        stateName = "";
        stateMotion = "";
        stateMachineName = "";
        genericInput = "";
        templateIndex = 0;
        changeTemplateName = false;
    }
    #endregion

    #region Edit and Save Templates
    // Apply current changes to current template
    void EditTemplate(ChangesType changes /*, bool applyDeletionChanges*/)
    {
        switch (changes)
        {
            case ChangesType.states:
                stateMachine.states.Add(newState);
                dirtyStates.Add(referredState);
                newState = null;
                break;
            case ChangesType.stateMachines:
                stateMachine.subStateMachines.Add(newStateMachine);
                dirtyStateMachines.Add(currentStateMachine);
                newStateMachine = null;
                break;
            case ChangesType.all:
                stateMachine.states.Add(newState);
                stateMachine.subStateMachines.Add(newStateMachine);
                dirtyStateMachines.Add(currentStateMachine);
                newState = null;
                newStateMachine = null;
                break;
        }
    }

    // Save the current template to the current working directory
    void SaveTemplate(string path)
    {
        string filePath = path + "/" + template.name + ".json";
        if (!File.Exists(filePath)) Debug.Log("File was created at " + filePath);
        File.WriteAllText(filePath, JSONTemplateUtilities.ToJSON(template));

        dirtyStates.Clear();
        dirtyStateMachines.Clear();

        AssetDatabase.Refresh();
        // FileStream file;

        // if (File.Exists(destination)) file = File.OpenWrite(destination);
        // else file = File.Create(destination);

        // file.Close();
    }
    #endregion

    #region Other
    private void OnItemSelected(object name)
    {
        Debug.Log(name);
        chosenState = (string)name;
    }

    public static EditorWindow[] GetAllOpenEditorWindows()
    {
        return Resources.FindObjectsOfTypeAll<EditorWindow>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log(eventData);
        Debug.Log("A ver...");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Let's see...");
    }
    #endregion
}
