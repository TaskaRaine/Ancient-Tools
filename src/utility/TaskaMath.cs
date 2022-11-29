namespace AncientTools.Utility
{
    using System;
    using Vintagestory.API.MathTools;

    class TaskaMath
    {
        public static double ShortLerpDegrees(double initialValue, double targetValue, double lerpFactor)
        {
            double delta = AngleClamp(targetValue - initialValue, 360);
            return GameMath.Lerp(initialValue, initialValue + (delta > 180 ? delta - 360 : delta), lerpFactor);
        }
        private static double AngleClamp(double angleDiff, double maxAngle)
        {
            return GameMath.Clamp(angleDiff - Math.Floor(angleDiff / maxAngle) * maxAngle, 0, maxAngle);
        }
    }
}
