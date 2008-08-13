using System;

namespace OpenMetaverse
{
    public static class MathHelper
    {
        public const float E = (float)Math.E;
        public const float LOG10E = 0.4342945f;
        public const float LOG2E = 1.442695f;
        public const float PI = (float)Math.PI;
        public const float TWO_PI = (float)(Math.PI * 2.0d);
        public const float PI_OVER_TWO = (float)(Math.PI / 2.0d);
        public const float PI_OVER_FOUR = (float)(Math.PI / 4.0d);

        /// <summary>
        /// Clamp a given value between a range
        /// </summary>
        /// <param name="val">Value to clamp</param>
        /// <param name="lower">Minimum allowable value</param>
        /// <param name="upper">Maximum allowable value</param>
        /// <returns>A value inclusively between lower and upper</returns>
        public static float Clamp(float value, float min, float max)
        {
            // First we check to see if we're greater than the max
            value = (value > max) ? max : value;

            // Then we check to see if we're less than the min.
            value = (value < min) ? min : value;

            // There's no check to see if min > max.
            return value;
        }

        /// <summary>
        /// Clamp a given value between a range
        /// </summary>
        /// <param name="val">Value to clamp</param>
        /// <param name="lower">Minimum allowable value</param>
        /// <param name="upper">Maximum allowable value</param>
        /// <returns>A value inclusively between lower and upper</returns>
        public static double Clamp(double value, double min, double max)
        {
            // First we check to see if we're greater than the max
            value = (value > max) ? max : value;

            // Then we check to see if we're less than the min.
            value = (value < min) ? min : value;

            // There's no check to see if min > max.
            return value;
        }

        /// <summary>
        /// Round a floating-point value to the nearest integer
        /// </summary>
        /// <param name="val">Floating point number to round</param>
        /// <returns>Integer</returns>
        public static int Round(float val)
        {
            return (int)Math.Floor(val + 0.5f);
        }

        /// <summary>
        /// Test if a single precision float is a finite number
        /// </summary>
        public static bool IsFinite(float value)
        {
            return !(Single.IsNaN(value) || Single.IsInfinity(value));
        }

        /// <summary>
        /// Test if a double precision float is a finite number
        /// </summary>
        public static bool IsFinite(double value)
        {
            return !(Double.IsNaN(value) || Double.IsInfinity(value));
        }

        /// <summary>
        /// Get the distance between two floating-point values
        /// </summary>
        /// <param name="value1">First value</param>
        /// <param name="value2">Second value</param>
        /// <returns>The distance between the two values</returns>
        public static float Distance(float value1, float value2)
        {
            return Math.Abs(value1 - value2);
        }

        public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
        {
            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (amount == 0f)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2d * v1 - 2d * v2 + t2 + t1) * sCubed +
                    (3d * v2 - 3d * v1 - 2d * t1 - t2) * sSquared +
                    t1 * s + v1;
            return (float)result;
        }

        public static double Hermite(double value1, double tangent1, double value2, double tangent2, double amount)
        {
            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (amount == 0d)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2d * v1 - 2d * v2 + t2 + t1) * sCubed +
                    (3d * v2 - 3d * v1 - 2d * t1 - t2) * sSquared +
                    t1 * s + v1;
            return result;
        }

        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static double Lerp(double value1, double value2, double amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static float SmoothStep(float value1, float value2, float amount)
        {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2
            float result = MathHelper.Clamp(amount, 0f, 1f);
            return MathHelper.Hermite(value1, 0f, value2, 0f, result);
        }

        public static double SmoothStep(double value1, double value2, double amount)
        {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2
            double result = MathHelper.Clamp(amount, 0f, 1f);
            return MathHelper.Hermite(value1, 0f, value2, 0f, result);
        }

        public static float ToDegrees(float radians)
        {
            // This method uses double precission internally,
            // though it returns single float
            // Factor = 180 / pi
            return (float)(radians * 57.295779513082320876798154814105);
        }

        public static float ToRadians(float degrees)
        {
            // This method uses double precission internally,
            // though it returns single float
            // Factor = pi / 180
            return (float)(degrees * 0.017453292519943295769236907684886);
        }
    }
}
