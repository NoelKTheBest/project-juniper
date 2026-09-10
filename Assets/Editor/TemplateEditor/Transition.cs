using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace TemplateUtilities 
{
	public class Transition
    {
        public List<string> conditions = new List<string>();
        public float duration;
        public bool hasFixedDuration;
        public float exitTime;
        public bool hasExitTime;
        public float offset;
        public bool orderedInterruption;
        public bool canTransitionToSelf;
    }
}