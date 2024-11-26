namespace Utility
{
    public static class Utilities
    {
        private static int millisInSec = 1000;
        private static int frameRate = 100;
        public static int ConvertFromPlaybackSpeedToMillisGap(int playbackSpeed) {
            return playbackSpeed * (millisInSec / frameRate);
        }
    }
}