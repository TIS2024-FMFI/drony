using UnityEngine;
using System;
using System.Collections.Generic;
using Interpreter;

namespace Drony.dto 
{
    public class CmdArgumentsDTO 
    {
        public Vector3 StartPosition { get; set; }
        public Quaternion StartYaw { get; set; }
        public Vector3 DestinationPosition { get; set; }
        public Quaternion DestinationYaw { get; set; }
        public int DestinationHeight { get; set; }
        public int Speed { get; set; }
        public Vector3 PointA { get; set; }
        public Vector3 PointB { get; set; }
        public Vector3 PointC { get; set; }
        public Vector3 PointD { get; set; }
        public Vector3 PointO { get; set; }
        public List<PointDTO> Points { get; set; }
        public bool IsClockwise { get; set; }
        public int NumberOfRevolutions { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public DroneMode DroneMode { get; set; }
        public Color Color { get; set; }
        public TimeSpan Time {get; set; }

        
    }

}