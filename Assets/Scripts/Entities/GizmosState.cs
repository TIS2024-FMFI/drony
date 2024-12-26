using UnityEngine;
using System;
using System.Collections.Generic;

namespace Drony.Entities
{
    public class GizmosState
    {
        public Vector3 Position { get; set; }
        public Color Color { get; set; } 
        public string Text { get; set; }
        public GUIStyle Style { get; set; }
        public GizmosState(Vector3 position, Color color, string text, GUIStyle style) {
            Position = position;
            Color = color;
            Text = text;
            Style = style;
        }
    }
}