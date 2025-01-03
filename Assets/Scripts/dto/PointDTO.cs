using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Drony.dto 
{
    public class PointDTO
    {
        public Vector3 Point { get; set; }
        public int Speed { get; set; }
        public Quaternion DestinationYaw { get; set; }

        public void copy(PointDTO other)
        {
            Point = other.Point;
            Speed = other.Speed;
            DestinationYaw = other.DestinationYaw;
        }
    }
}