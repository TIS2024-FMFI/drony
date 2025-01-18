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
        public int Time { get; set; } 
        public bool IsKeyState { get; set; }

        public DroneState() {
        }

        public DroneState(DroneState other)
        {
            Position = other.Position;
            Color = other.Color;
            YawAngle = other.YawAngle;
            Time = other.Time;
            IsKeyState = other.IsKeyState;
        }
    }
}