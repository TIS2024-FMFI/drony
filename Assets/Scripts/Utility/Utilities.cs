using Unity.VisualScripting;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

namespace Utility
{
    public static class Utilities
    {
        private static int MILLIS_IN_SEC = 1000;
        private static int FRAME_RATE = 100;
        private static int MILLIMETERS_IN_ONE_METER = 1000;
        public static int ConvertFromPlaybackSpeedToMillisGap(int playbackSpeed) 
        {
            return playbackSpeed * (MILLIS_IN_SEC / FRAME_RATE);
        }
        public static int ConvertFromMetersToMillimeters(float meter) 
        {
            return (int) (meter * MILLIMETERS_IN_ONE_METER);
        }
        public static float ConvertFromMillisecondsToSeconds(int ms)
        {
            return ms / MILLIS_IN_SEC;
        }
        public static string ReadTextFile(string path)
        {
            return File.ReadAllText(path);
        }
        public static byte[] ReadByteFile(string path)
        {
            return File.ReadAllBytes(path);
        }
        public static List<string> GetLinesFromString(string data)
        {
            return data.Split(new[] { '\n' }, StringSplitOptions.None).ToList();
        }
        public static Color GetContrastColor(int index)
        {
            // Generate a fixed palette of 21 highly contrasting colors
            Color[] contrastColors = new Color[]
            {
                Color.red, Color.green, Color.blue, Color.yellow, Color.magenta,
                Color.cyan, Color.white, Color.black, new Color(1, 0.5f, 0), // Orange
                new Color(0.5f, 0, 1), // Purple
                new Color(0, 0.5f, 1), // Sky Blue
                new Color(0.5f, 1, 0), // Light Green
                new Color(1, 0, 0.5f), // Pinkish Red
                new Color(0.5f, 0.5f, 0), // Olive
                new Color(0.5f, 0, 0.5f), // Deep Purple
                new Color(0, 0.5f, 0.5f), // Teal
                new Color(0.8f, 0.8f, 0.8f), // Light Gray
                new Color(0.3f, 0.3f, 0.3f), // Dark Gray
                new Color(0.7f, 0.1f, 0.1f), // Dark Red
                new Color(0.1f, 0.7f, 0.1f), // Dark Green
                new Color(0.1f, 0.1f, 0.7f) // Dark Blue
            };
            return contrastColors[Mathf.Abs(index) % contrastColors.Count()];
        }
    }
}