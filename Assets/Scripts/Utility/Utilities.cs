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
        public static int ConvertFromMetersToMillimeters(int meter) 
        {
            return meter * MILLIMETERS_IN_ONE_METER;
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
    }
}