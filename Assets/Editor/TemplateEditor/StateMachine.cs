using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace TemplateUtilities
{
	public class StateMachine
    {
        public string stateMachineName;
        public int index;
        public List<string> behaviours = new List<string>();
        public List<string> transitions = new List<string>();
        public List<State> states = new List<State>();
        public List<StateMachine> subStateMachines = new List<StateMachine>();
        
        public StateMachine(string name, State state)
        {
            stateMachineName = name;
            states.Add(state);
        }
        
        [JsonConstructor]
        public StateMachine(string name)
        {
            stateMachineName = name;
        }

        public void AddBehaviour(string type, List<string> fields, List<string> values)
        {
            // new behaviours must be in the form:
            //  b [type] [property] [value] [property] [value] ...
            string b = type + " ";

            for (int i = 0; i < fields.Count; i++)
            {
                b += fields[i] + values[i];
            }
            Debug.Log(b);

            behaviours.Add(b);
        }

        public void AddTransition(string destination, string parameter, int conditionType)
        {
            // new transitions must be in the form:
            //  tr [stateName] [parameter] [conditionType]
            string tr = destination + " " + parameter + " " + conditionType;
            Debug.Log(tr);

            transitions.Add(tr);
        }

        public void AddState(State newState)
        {
            newState.index = states.Count;
            states.Add(newState);
        }

        public void AddStateMachine(StateMachine newStateMachine)
        {
            newStateMachine.index = subStateMachines.Count;
            subStateMachines.Add(newStateMachine);
        }
    }
}