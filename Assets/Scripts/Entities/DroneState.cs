using UnityEngine;
using System;
using System.Collections.Generic;

namespace Entities
{
    public class DroneState
    {
        public Vector3 Position { get; set; }
        public Color Color { get; set; } 
        public float YawAngle { get; set; }
        public float Time { get; set; } 

        public DroneState(Vector3 position, Color color, float yawAngle, float time)
        {
            Position = position;
            Color = color;
            YawAngle = yawAngle;
            Time = time;
        }

        public DroneState(Vector3 position) 
        {
            Position = position;
        }
    }
}