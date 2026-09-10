using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using TemplateUtilities;
using Newtonsoft.Json;

namespace AnimatorUtilities
{
    public enum AnimatorNodeType
    {
        Entry,
        Anystate,
        Exit,
        Parent,
    }

    public static class PreviousAnimatorController
    {
        // Place values here
    }

    struct RelativePositionBox
    {
        public Vector3 center;
        public Vector3 extents;
    }

    public static class AnimatorControllerUtility
    {
        // Might create objects and keep records of various AnimatorControllers using PreviousAnimatorController

        public static AnimatorController lastController;
        public static AnimatorStateMachine lastRootStateMachine;
        public static AnimatorState[] lastStates;
        public static AnimatorStateTransition[] lastTransitions;
        
        static Vector3 nodeSize = new Vector3(320, 58, 0);
        static Vector3 stateSize = new Vector3(400, 78, 0);

        //static RelativePositionBox entryBox = new RelativePositionBox();
        //static RelativePositionBox anyStateBox = new RelativePositionBox();
        //static RelativePositionBox exitBox = new RelativePositionBox();
        //static RelativePositionBox parentBox = new RelativePositionBox();

        // Summary:
        //     Creates an empty animator controller with a given filename in the root of the Assets folder.
        //
        // Parameters:
        //   name:
        //      The name of the file.
        public static void CreateAnimatorController(string name)
        {
            var controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/" + name + ".controller");
            var rootStateMachine = controller.layers[0].stateMachine;

            lastController = controller;
            lastRootStateMachine = rootStateMachine;
        }

        // Summary:
        //      Creates an empty animator controller at the given path with the given filename.
        //
        // Parameters:
        //   path:
        //      The path of the file.
        //
        //   fileName:
        //      The name of the file.
        public static void CreateAnimatorController(string path, string fileName)
        {
            var controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/" + path + "/" + fileName + ".controller");
            var rootStateMachine = controller.layers[0].stateMachine;

            lastController = controller;
            lastRootStateMachine = rootStateMachine;
        }

        public static void CreateControllerFromTemplate(string path, string fileName)
        {
            string moveCondition = "isMoving";

            var controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/" + path + "/" + fileName + ".controller");
            Motion idleMotion = (Motion)AssetDatabase.LoadAssetAtPath("Assets/Templates/01/Basic_Idle.anim", typeof(Motion));
            Motion moveMotion = (Motion)AssetDatabase.LoadAssetAtPath("Assets/Templates/01/Basic_Run.anim", typeof(Motion));

            controller.AddParameter(moveCondition, AnimatorControllerParameterType.Bool);

            var rootStateMachine = controller.layers[0].stateMachine;

            var idleState = rootStateMachine.AddState("Idle", GetRelativePosition(rootStateMachine, 
                new Vector3(-10, 50, 0), (AnimatorNodeType)Enum.ToObject(typeof(AnimatorNodeType), 3)));
            idleState.motion = idleMotion;
            idleState.AddStateMachineBehaviour<Basic_Idle>();
            var moveState = rootStateMachine.AddState("Move", GetRelativePosition(GetChildState(rootStateMachine,
                "Idle"),new Vector3(10, 50, 0)));
            moveState.motion = moveMotion;
            moveState.AddStateMachineBehaviour<Basic_Move>();
            
            var entryTransition = rootStateMachine.AddEntryTransition(idleState);
            var moveTransition = idleState.AddTransition(moveState);
            moveTransition.AddCondition(AnimatorConditionMode.If, 0, moveCondition);
            moveTransition.duration = 0;
            moveTransition.hasExitTime = false;
            var idleTransition = moveState.AddTransition(idleState);
            idleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, moveCondition);
            idleTransition.duration = 0;
            idleTransition.hasExitTime = false;
            
            lastController = controller;
            lastRootStateMachine = rootStateMachine;
            lastStates = new AnimatorState[] { idleState, moveState };
            lastTransitions = new AnimatorStateTransition[] { idleTransition, moveTransition };

            RefreshWindow();
        }
                
        public static void CreateControllerFromTemplate(string templatePath, string path, string fileName)
        {
            // Load a template a create a new controller from that template
            Template template = JSONTemplateUtilities.FromJSON("Assets/Templates/Data/newStateMachineData.json");

            var controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/" + path + "/" + fileName + ".controller");

            foreach (string param in template.parameters)
            {
                string[] subStrings = param.Split(' ');
                controller.AddParameter(subStrings[0],
                    (AnimatorControllerParameterType)Enum.ToObject(typeof(AnimatorControllerParameterType), Int16.Parse(subStrings[1])));
            }

            var rootStateMachine = controller.layers[0].stateMachine;
            
            AddStateMachines(rootStateMachine, template.rootStateMachine, controller);
        }

        private static void AddStateMachines(AnimatorStateMachine parent, StateMachine node, AnimatorController controller)
        {
            foreach (State state in node.states)
            {
                var newState = parent.AddState(state.stateName);
                if (state.stateMotion != null) newState.motion =
                        (Motion)AssetDatabase.LoadAssetAtPath("Assets/Templates/01/" + state.stateMotion + ".anim", typeof(Motion));

                /*
                foreach (string transition in state.transitions)
                {
                    string[] subStrings = transition.Split(' ');
                    if (Array.Find(controller.parameters, param => param.name == "") != null)
                    {
                        // add condition to state
                    }
                    //conditionName = 
                }
                */
            }

            // Recur
            #region Note
            /*
             * In the future, I should understand that during recursion, we need to work with the current node or parent
             * and not future children whom we don't even know if they exist yest
             */
            #endregion
            foreach (StateMachine stateMachine in node.subStateMachines)
            {
                var sub = parent.AddStateMachine(stateMachine.stateMachineName);
                AddStateMachines(sub, stateMachine, controller);
            }
        }

        public static void CreateTemplate(string path)
        {

        }
        
        #region Postioning Methods
            // Move states or state machines to position relative to entry node
        private static Vector3 GetRelativePosition(AnimatorStateMachine layer,
            Vector3 newPosition, AnimatorNodeType animatorNodeType)
        {
            float xMultiplier = newPosition.x >= 0 ? 1 : -1;
            float yMultiplier = newPosition.y >= 0 ? 1 : -1;
            
            switch (animatorNodeType)
            {
                case AnimatorNodeType.Entry:
                    return layer.entryPosition + new Vector3((nodeSize.x * xMultiplier) + newPosition.x,
                (nodeSize.y * yMultiplier) + newPosition.y);

                case AnimatorNodeType.Anystate:
                    return layer.anyStatePosition + new Vector3((nodeSize.x * xMultiplier) + newPosition.x,
                (nodeSize.y * yMultiplier) + newPosition.y);

                case AnimatorNodeType.Exit:
                    return layer.exitPosition + new Vector3((nodeSize.x * xMultiplier) + newPosition.x,
                (nodeSize.y * yMultiplier) + newPosition.y);

                case AnimatorNodeType.Parent:
                    return layer.parentStateMachinePosition + new Vector3((nodeSize.x * xMultiplier) + newPosition.x,
                (nodeSize.y * yMultiplier) + newPosition.y);

                default:
                    return layer.entryPosition + new Vector3((nodeSize.x * xMultiplier) + newPosition.x,
                (nodeSize.y * yMultiplier) + newPosition.y);
            }
        }

        // Move states or state machines to position relative to given state
        private static Vector3 GetRelativePosition(ChildAnimatorState state, Vector3 newPosition)
        {
            float xMultiplier = newPosition.x >= 0 ? 1 : -1;
            float yMultiplier = newPosition.y >= 0 ? 1 : -1;

            return state.position + new Vector3((nodeSize.x * xMultiplier) + newPosition.x,
                (nodeSize.y * yMultiplier) + newPosition.y);
        }

        private static Vector3 GetRelativePosition(ChildAnimatorStateMachine stateMachine, Vector3 newPosition)
        {
            float xMultiplier = newPosition.x >= 0 ? 1 : -1;
            float yMultiplier = newPosition.y >= 0 ? 1 : -1;
            
            return stateMachine.position + new Vector3((nodeSize.x * xMultiplier) + newPosition.x,
                (nodeSize.y * yMultiplier) + newPosition.y);
        }

        /*
        // Set the position of a state inside a relative position box point 
        //  at a specified distance away from the center in pixels
        //
        //  If scaleBox is set to true we expand the size of the box to include that position
        //
        //  If scaleBox is set to false the position of the state gets clamped to the edge of the box
        private static Vector3 SetPositionInBox(Vector3 center, Vector3 newPosition, bool scaleBox)
        {
            float xMultiplier = newPosition.x >= 0 ? 1 : -1;
            float yMultiplier = newPosition.y >= 0 ? 1 : -1;

            if (scaleBox)
                entryBox.center = center;
                entryBox.extents = 
                return center + new Vector3((nodeSize.x * xMultiplier) + newPosition.x,
                    (nodeSize.y * yMultiplier) + newPosition.y);
            else
                return 
        }

        // Set the position of a state inside a relative position box point
        //  at a specified distance away from the center in percentages
        //
        //  If scaleBox is set to true we expand the size of the box to include that position
        //
        //  If scaleBox is set to false the position of the state gets clamped to the edge of the box
        private static void SetRelativePositionInBox(Vector3 center, Vector3 relativePosition, bool scaleBox)
        {
            Math.Clamp(relativePosition.x, -1, 1);
            Math.Clamp(relativePosition.y, -1, 1);
        }
        /**/
        #endregion

        private static ChildAnimatorState GetChildState(AnimatorStateMachine layer, string stateName)
        {
            return Array.Find(layer.states, childState => childState.state.name == stateName);
        }

        private static ChildAnimatorStateMachine GetChildStateMachine(AnimatorStateMachine layer, string stateMachineName)
        {
            return Array.Find(layer.stateMachines, 
                childStateMachines => childStateMachines.stateMachine.name == stateMachineName);
        }

        /*
        public static void MoveRelativePostion(string stateName, Vector3 newPosition)
        {
            string controllerPath = "Assets/Templates/01/Basic_Controller.controller";
            AnimatorController basicController = (AnimatorController)AssetDatabase.LoadAssetAtPath(controllerPath,
                typeof(AnimatorController));

            var rootStateMachine = basicController.layers[0].stateMachine;
            var foundState = Array.Find(rootStateMachine.states, childState => childState.state.name == stateName);

            float xMultiplier = newPosition.x >= 0 ? 1 : -1;
            float yMultiplier = newPosition.y >= 0 ? 1 : -1;

            Debug.Log("from root: " + rootStateMachine.states[0].position);
            Debug.Log("new struct: " + foundState.position);

            foundState.position = rootStateMachine.entryPosition + new Vector3((nodeSize.x * xMultiplier) + newPosition.x,
                (nodeSize.y * yMultiplier) + newPosition.y);

            Debug.Log("from root: " + rootStateMachine.states[0].position);
            Debug.Log("new struct: " + foundState.position);

            rootStateMachine.states[0].position = foundState.position;

            Debug.Log("from root: " + rootStateMachine.states[0].position);
            Debug.Log("new struct: " + foundState.position);

            rootStateMachine.exitPosition = rootStateMachine.entryPosition + new Vector3((nodeSize.x * xMultiplier) + newPosition.x,
                (nodeSize.y * yMultiplier) + newPosition.y);

            Debug.Log("from root: " + rootStateMachine.states[0].position);
            Debug.Log("new struct: " + foundState.position);

            RefreshWindow();
        }
        /**/

        private static void RefreshWindow<T>(T type)
        {
            //AnimationWindow animatorWindow
        }

        private static void RefreshWindow()
        {
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
        }
    }
}
