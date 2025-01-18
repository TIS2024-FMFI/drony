using System.Collections.Generic; 
using UnityEngine;

namespace Utility
{
    public static class MathOperations
    {
        public static (Vector3 T1, Vector3 T2) GetSmoothPointsForBezier(Vector3 A, Vector3 B, Vector3 C, float smoothness)
        {
            float angleABC = CalculateAngleABC(A, B, C);
            float angleBeta = (180 - angleABC) / 2;
            Vector3 T1 = CalculatePointA(B, A, C, smoothness, angleBeta);
            Vector3 T2 = CalculatePointA(B, C, A, smoothness, angleBeta);
            return (T1, T2);
        }
        public static float CalculateAngleABC(Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 BA = A - B;
            Vector3 BC = C - B;

            float dotProduct = Vector3.Dot(BA, BC);

            float magnitudeBA = BA.magnitude;
            float magnitudeBC = BC.magnitude;

            float cosTheta = dotProduct / (magnitudeBA * magnitudeBC);

            float angleRadians = Mathf.Acos(cosTheta);

            float angleDegrees = Mathf.Rad2Deg * angleRadians;

            return angleDegrees;
        }
        public static Vector3 CalculatePointA(Vector3 B, Vector3 C, Vector3 D, float ABLength, float angleABC)
        {
            Vector3 BC = C - B;
            Vector3 BD = D - B;

            Vector3 normal = Vector3.Cross(BD, BC).normalized;

            Vector3 unitBC = BC.normalized;

            Vector3 perpendicular = Vector3.Cross(normal, unitBC).normalized;

            float u = ABLength * Mathf.Cos(angleABC * Mathf.Deg2Rad);
            float v = ABLength * Mathf.Sin(angleABC * Mathf.Deg2Rad);

            Vector3 AB = u * unitBC + v * perpendicular;

            Vector3 A = B + AB;
            return A;
        }
        public static List<Vector3> FindPointsForSpiral(Vector3 A, Vector3 B, Vector3 S, Vector3 D, int numberOfRevolutions, bool isClockwise)
        {
            if (numberOfRevolutions <= 0)
            {
                return null;
            }
            Vector3 SO = FindPerpendicularPoint(A, B, S);
            Vector3 DO = FindPerpendicularPoint(A, B, D);
            float distanceSD = Vector3.Distance(SO, DO);
            int numberOfPointsInTurn = 4;
            int numberOfPoints = numberOfRevolutions * numberOfPointsInTurn;
            float Dx = distanceSD / numberOfPoints;
            List<Vector3> points = new List<Vector3>();

            for (int i = 0; i < numberOfPoints; i++)
            {
                float angle = isClockwise ? -90 : 90;
                Vector3 P1 = FindPointOnCircleByAB(SO, S, A, B, angle);
                Vector3 P1s = FindPointOnParallel(A, B, P1, Dx);
                points.Add(P1s);
                S = P1s;
                SO = FindPerpendicularPoint(A, B, S);
            }
            return points;
        }
        /// <summary>
        /// Finds the perpendicular projection of point S onto the line AB.
        /// </summary>
        /// <param name="A">Point A on the line.</param>
        /// <param name="B">Point B on the line.</param>
        /// <param name="S">Point S to project onto the line.</param>
        /// <returns>Perpendicular projection point on the line AB.</returns>
        public static Vector3 FindPerpendicularPoint(Vector3 A, Vector3 B, Vector3 S)
        {
            Vector3 AB = B - A;
            Vector3 AS = S - A;
            float t = Vector3.Dot(AS, AB) / Vector3.Dot(AB, AB);
            return A + t * AB; // Point on AB
        }
        /// <summary>
        /// Finds a point P1' on a vector parallel to AB, at a distance Dx from P1.
        /// </summary>
        /// <param name="A">Point A on the line.</param>
        /// <param name="B">Point B on the line.</param>
        /// <param name="P1">Starting point P1.</param>
        /// <param name="Dx">Distance to move from P1 along the parallel vector.</param>
        /// <returns>Point P1' at distance Dx along the parallel vector.</returns>
        public static Vector3 FindPointOnParallel(Vector3 A, Vector3 B, Vector3 P1, float Dx)
        {
            Vector3 AB = B - A; // Vector along the line AB
            Vector3 unitDirection = AB.normalized; // Unit vector in the direction of AB

            // Point P1' in the specified direction and distance Dx
            return P1 + unitDirection * Dx;
        }     
        public static Vector3 FindPerpendicularRandomPoint(Vector3 A, Vector3 B, Vector3 O)
        {
            // Step 1: Calculate the normal vector of the plane (AB direction)
            Vector3 normal = (B - A).normalized;

            // Step 2: Calculate C (constant term in the plane equation)
            float C = Vector3.Dot(normal, O);

            // Step 3: Generate random x and y values
            float randomX = UnityEngine.Random.Range(-10f, 10f);
            float randomY = UnityEngine.Random.Range(-10f, 10f);

            // Step 4: Solve for z
            float z;
            if (Mathf.Abs(normal.z) > 1e-5) // Check if N_z is not zero
            {
                z = (C - normal.x * randomX - normal.y * randomY) / normal.z;
            }
            else
            {
                // N_z is zero, so set z arbitrarily and solve for x or y instead
                z = UnityEngine.Random.Range(-10f, 10f);
                randomX = (C - normal.y * randomY - normal.z * z) / normal.x;
            }

            // Step 5: Return the random point
            return new Vector3(randomX, randomY, z);
        }
        public static Vector3 FindPointOnCircle(Vector3 O, Vector3 P, Vector3 D, float alphaDegrees)
        {
            // Calculate the normal of the plane defined by P, O, and D
            Vector3 planeNormal = Vector3.Cross(P - O, D - O).normalized;

            // Calculate the radius vector (OP) and normalize it
            Vector3 OP = P - O;
            float radius = OP.magnitude;
            Vector3 OPNormalized = OP.normalized;

            // Find a vector perpendicular to OP in the plane (perpendicular1)
            Vector3 perpendicular1 = Vector3.Cross(planeNormal, OPNormalized).normalized;

            // Convert alpha to radians
            float alphaRadians = Mathf.Deg2Rad * alphaDegrees;

            // Compute the new point P1 using the circle equation
            Vector3 P1 = O + Mathf.Cos(alphaRadians) * radius * OPNormalized +
                            Mathf.Sin(alphaRadians) * radius * perpendicular1;

            return P1;
        }
        public static Vector3 FindPointOnCircleByAB(Vector3 O, Vector3 P, Vector3 A, Vector3 B, float alphaDegrees)
        {
            // Calculate the normal of the plane defined by A and B (AB direction)
            Vector3 planeNormal = (B - A).normalized;

            // Calculate the radius vector (OP) and normalize it
            Vector3 OP = P - O;
            float radius = OP.magnitude;
            Vector3 OPNormalized = OP.normalized;

            // Find a vector perpendicular to OP in the plane (perpendicular1)
            Vector3 perpendicular1 = Vector3.Cross(planeNormal, OPNormalized).normalized;

            // Convert alpha to radians
            float alphaRadians = Mathf.Deg2Rad * alphaDegrees;

            // Compute the new point P1 using the circle equation
            Vector3 P1 = O + Mathf.Cos(alphaRadians) * radius * OPNormalized +
                            Mathf.Sin(alphaRadians) * radius * perpendicular1;

            return P1;
        }
        public static Vector3 GetQuadraticBezierPositionByTime(float Time, Vector3 A, Vector3 B, Vector3 C) 
        {
            Time = Mathf.Clamp01(Time);
            float invTime = 1 - Time;
            return (invTime * invTime * A)
                + (2 * invTime * Time * B)
                + (Time * Time * C);
        }
        public static Vector3 GetCubicBezierPositionByTime(float Time, Vector3 A, Vector3 B, Vector3 C, Vector3 D) 
        {
            Time = Mathf.Clamp01(Time);
            float invTime = 1 - Time;
            return (invTime * invTime * invTime * A)
                + (3 * invTime * invTime * Time * B)
                + (3 * invTime * Time * Time * C)
                + (Time * Time * Time * D);
        }
        public static float CalculateQuadraticBezierCurveLength(Vector3 A, Vector3 B, Vector3 C, int subdivisions = 100)
        {
            float totalLength = 0f;

            // Previous point on the curve
            Vector3 previousPoint = A;

            // Step size
            float step = 1f / subdivisions;

            for (int i = 1; i <= subdivisions; i++)
            {
                float t = i * step;

                // Calculate the current point on the curve
                Vector3 currentPoint = GetQuadraticBezierPositionByTime(t, A, B, C);

                // Add the distance between the previous point and the current point
                totalLength += Vector3.Distance(previousPoint, currentPoint);

                // Update the previous point
                previousPoint = currentPoint;
            }

            return totalLength;
        }
        public static float CalculateCubicBezierCurveLength(Vector3 A, Vector3 B, Vector3 C, Vector3 D, int subdivisions = 100)
        {
            float totalLength = 0f;

            // Previous point on the curve
            Vector3 previousPoint = A;

            // Step size
            float step = 1f / subdivisions;

            for (int i = 1; i <= subdivisions; i++)
            {
                float t = i * step;

                // Calculate the current point on the curve
                Vector3 currentPoint = GetCubicBezierPositionByTime(t, A, B, C, D);

                // Add the distance between the previous point and the current point
                totalLength += Vector3.Distance(previousPoint, currentPoint);

                // Update the previous point
                previousPoint = currentPoint;
            }

            return totalLength;
        }
    }
}
