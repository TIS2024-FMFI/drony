using UnityEngine;
using System;
using System.Collections.Generic;

namespace Drony.Entities
{
    public class DroneState
    {
        public Vector3 Position { get; set; }
        public Color Color { get; set; } 
        public Quaternion YawAngle { get; set; }
        public float Time { get; set; } 

        public DroneState(Vector3 position, Color color, Quaternion yawAngle, float time)
        {
            Position = position;
            Color = color;
            YawAngle = yawAngle;
            Time = time;
        }

        public DroneState(Vector3 position, Quaternion yawAngle) 
        {
            Position = position;
            YawAngle = yawAngle;
        }

        public DroneState(Vector3 position) 
        {
            Position = position;
        }
    }
}