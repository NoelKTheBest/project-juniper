using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace TemplateUtilities
{    
    public class Template
    {
        public string name;
        public List<string> parameters = new List<string>();
        public StateMachine rootStateMachine = new StateMachine("Base Layer");

        public Template()
        {
            name = "";
        }
    }

    public class JSONTemplateUtilities
    {
        public static string ToJSON(Template template)
        {
            return JsonConvert.SerializeObject(template, Formatting.Indented);
        }

        public static Template FromJSON(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<Template>(jsonString);
        }

        #region Unused
        public static void EditJSON(Template template, string path, StateMachine sm/*Other Params: pathToStateMachine*/)
        {
            // Indices will not include the base layer
            string[] pathStrings = path.Split(' ');
            List<int> indices = new List<int>(pathStrings.Length);
            
            foreach (string str in pathStrings)
            {
                indices.Add(Int32.Parse(str));
                Debug.Log(str);
            }

            AddFromTemplate(template.rootStateMachine, indices, sm);

            string jsonString = JsonConvert.SerializeObject(template, Formatting.Indented);
            string destination = "Assets/Templates/Data/newStateMachineData.json";
            File.WriteAllText(destination, jsonString);
        }

        public static void EditJSON(Template template, string path, State state/*Other Params: pathToStateMachine*/)
        {
            // Indices will not include the base layer
            string[] pathStrings = path.Split(' ');
            List<int> indices = new List<int>(pathStrings.Length);

            foreach (string str in pathStrings)
            {
                indices.Add(Int32.Parse(str));
                Debug.Log(str);
            }

            AddFromTemplate(template.rootStateMachine, indices, state);

            string jsonString = JsonConvert.SerializeObject(template, Formatting.Indented);
            string destination = "Assets/Templates/Data/newStateMachineData.json";
            File.WriteAllText(destination, jsonString);
        }

        private static void AddFromTemplate(StateMachine sm, List<int> indices, StateMachine newSM)
        {
            // Recur until we reach the destination state machine
            if (indices.Count > 0)
            {
                var index = indices[0];
                indices.RemoveAt(0);
                AddFromTemplate(sm.subStateMachines[index], indices, newSM);
            }
            // Perform action on desitination state machine
            else
            {
                sm.AddStateMachine(newSM);
            }
        }

        private static void AddFromTemplate(StateMachine sm, List<int> indices, State newState)
        {
            // Recur until we reach the destination state machine
            if (indices.Count > 0)
            {
                var index = indices[0];
                indices.RemoveAt(0);
                AddFromTemplate(sm.subStateMachines[index], indices, newState);
            }
            // Perform action on desitination state machine
            else
            {
                sm.AddState(newState);
            }
        }
        #endregion
    }
}

