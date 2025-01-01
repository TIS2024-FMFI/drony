using UnityEngine;
using System;
using System.Collections.Generic;

namespace Drony.dto 
{
    public class CmdArgumentsDTO 
    {
        public Vector3 StartPosition { get; set; }
        public Vector3 DestinationPosition { get; set; }
        public int DestinationYaw { get; set; }
        public int DestinationHeight { get; set; }
        public int Speed { get; set; }
        public Vector3 PointA { get; set; }
        public Vector3 PointB { get; set; }
        public Vector3 PointC { get; set; }
        public bool IsClockwise { get; set; }
        public int NumberOfRevolutions { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        
    }

}