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

        public DroneState(Vector3 position, Color color, Quaternion yawAngle, int time)
        {
            Position = position;
            Color = color;
            YawAngle = yawAngle;
            Time = time;
        }

        public DroneState(Vector3 position, Quaternion yawAngle, int time) 
        {
            Position = position;
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

        public DroneState(int time) 
        {
            Time = time;
        }

        public DroneState(DroneState other)
        {
            Position = other.Position;
            Color = other.Color;
            YawAngle = other.YawAngle;
            Time = other.Time;
        }
    }
}