using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace TemplateUtilities
{
	public class State
    {
        public string stateName;
        public string stateMotion;
        public int index;
        public List<string> behaviours = new List<string>();
        public List<string> transitions = new List<string>();

        //public Dictionary<string, string> properties = new Dictionary<string, string>();
        
        public State(string name, string motion)
        {
            stateName = name;
            stateMotion = motion;
        }

        [JsonConstructor]
        public State(string name)
        {
            stateName = name;
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
    }
}