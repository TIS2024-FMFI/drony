using System;
using System.Collections;
using System.Collections.Generic;
using Drony.dto;
using Drony.Entities;
using UnityEngine;
using Utility;

namespace Interpreter
{
    public class TrajectoryGenerator {
        private int MAX_FLIGHT_TIME = 3600000; // 1 hour in ms
        
        private int TAKEOFF_YAW = 0; // FIXME: add an option to set it in config file or in ui

        private List<DroneState> GenerateEmptyStates(int duration)
        {
            Debug.Log($"GenerateEmptyStates, time start: {0}");
            List<DroneState> emptyStates = new List<DroneState>();
            for (int timeMoment = 0; timeMoment < duration; timeMoment++) {
                DroneState currentState = new DroneState();
                currentState.Time = timeMoment;
                emptyStates.Add(currentState);
            }
            Debug.Log($"GenerateEmptyStates, time end: {duration - 1}");
            return emptyStates;
        }

        public DroneTrajectory initDroneTrajectory(string droneId) 
        {
            DroneTrajectory droneTrajectory = new DroneTrajectory();
            droneTrajectory.DroneId = droneId;
            droneTrajectory.Trajectory = GenerateEmptyStates(MAX_FLIGHT_TIME);
            return droneTrajectory;
        }

        public DroneTrajectory SetInitialDronePosition(
                        DroneTrajectory droneTrajectory,
                        int timestamp,
                        CmdArgumentsDTO cmdArguments)
        {
            Vector3 startPosition = cmdArguments.StartPosition;
            int timeFrom = 0;
            int timeTo = timestamp; // timestamp included
            for (int timeMoment = timeFrom; timeMoment <= timeTo; timeMoment++) {
                droneTrajectory[timeMoment].Position = startPosition;
                droneTrajectory[timeMoment].YawAngle = Quaternion.Euler(0, TAKEOFF_YAW, 0);
            }
            droneTrajectory.LastStateIndex = timeTo;

            Debug.Log($"SetInitialDronePosition, time end: {timeTo}");
            return droneTrajectory;
        }
        
        public DroneTrajectory GenerateHoverTrajectory(
                        DroneTrajectory droneTrajectory,
                        int timeFrom,
                        int timeTo,
                        DroneState exampleState)
        {
            Debug.Log($"GenerateHoverTrajectory, time start: {timeFrom}");

            for (int timeMoment = timeFrom; timeMoment < timeTo; timeMoment++) {
                DroneState droneState = new DroneState(exampleState);
                droneState.Time = timeMoment;
                droneState.IsKeyState = false;
                droneTrajectory[timeMoment] = droneState;
                droneTrajectory.LastStateIndex = timeMoment;
            }

            Debug.Log($"GenerateHoverTrajectory, time end: {droneTrajectory.LastStateIndex}");
            return droneTrajectory;
        }

        public DroneTrajectory GenerateLinearTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        CmdArgumentsDTO cmdArguments) 
        {
            Debug.Log($"GenerateLinearTrajectory, time start: {timestamp}");

            Vector3 startPosition = cmdArguments.StartPosition;
            Quaternion startRotation = cmdArguments.StartYaw;
            Vector3 destinationPosition = cmdArguments.DestinationPosition;
            Quaternion targetRotation = cmdArguments.DestinationYaw;
            int speed = cmdArguments.Speed;

            // maybe change to float?
            int distanceMeters = (int)Vector3.Distance(startPosition, destinationPosition);
            int distanceMillimeters = Utilities.ConvertFromMetersToMillimeters(distanceMeters);

            int totalTime = distanceMillimeters / speed;

            // interpolation between start and destination vectors
            int duration = timestamp + totalTime;
            for (int timeMoment = timestamp; timeMoment < duration; timeMoment++)
            {
                int localTimeMoment = timeMoment - timestamp;
                float t = localTimeMoment / (float) totalTime;
                Vector3 point = Vector3.Lerp(startPosition, destinationPosition, t);
                Quaternion yaw = Quaternion.Slerp(startRotation, targetRotation, t);
                droneTrajectory[timeMoment].Position = point;
                droneTrajectory[timeMoment].YawAngle = yaw;
                droneTrajectory.LastStateIndex = timeMoment;
            }
            
            Debug.Log($"GenerateLinearTrajectory, time end: {droneTrajectory.LastStateIndex}");
            return droneTrajectory;
        }

        public DroneTrajectory GenerateQuadraticBezierTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        CmdArgumentsDTO cmdArguments)
        {
            Debug.Log($"GenerateQuadraticBezierTrajectory, time start: {timestamp}");
            Vector3 pointA = cmdArguments.PointA;
            Vector3 pointB = cmdArguments.PointB;
            Vector3 pointC = cmdArguments.PointC;
            Quaternion startRotation =  cmdArguments.StartYaw;
            Quaternion targetRotation = cmdArguments.DestinationYaw;
            int speed = cmdArguments.Speed;

            float distanceMeters = CalculateQuadraticBezierCurveLength(pointA, pointB, pointC);
            int distanceMillimeters = Utilities.ConvertFromMetersToMillimeters(distanceMeters);
            int totalTime = distanceMillimeters / speed;

            int duration = timestamp + totalTime;
            for (int timeMoment = timestamp; timeMoment < duration; timeMoment++)
            {
                int localTimeMoment = timeMoment - timestamp;
                float t = localTimeMoment / (float) totalTime;
                Quaternion yaw = Quaternion.Slerp(startRotation, targetRotation, t);
                Vector3 point = GetQuadraticBezierPositionByTime(t, pointA, pointB, pointC);
                droneTrajectory[timeMoment].Position = point;
                droneTrajectory[timeMoment].YawAngle = yaw;
                droneTrajectory.LastStateIndex = timeMoment;
            }
            Debug.Log($"GenerateQuadraticBezierTrajectory, time end: {droneTrajectory.LastStateIndex}");
            return droneTrajectory;
        }

        public DroneTrajectory GenerateCubicBezierTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        CmdArgumentsDTO cmdArguments)
        {
            Debug.Log($"GenerateCubicBezierTrajectory, time start: {timestamp}");
            Vector3 pointA = cmdArguments.PointA;
            Vector3 pointB = cmdArguments.PointB;
            Vector3 pointC = cmdArguments.PointC;
            Vector3 pointD = cmdArguments.PointD;
            Quaternion startRotation =  cmdArguments.StartYaw;
            Quaternion targetRotation = cmdArguments.DestinationYaw;
            int speed = cmdArguments.Speed;

            float distanceMeters = CalculateCubicBezierCurveLength(pointA, pointB, pointC, pointD);
            int distanceMillimeters = Utilities.ConvertFromMetersToMillimeters(distanceMeters);
            int totalTime = distanceMillimeters / speed;

            int duration = timestamp + totalTime;
            for (int timeMoment = timestamp; timeMoment < duration; timeMoment++)
            {
                int localTimeMoment = timeMoment - timestamp;
                float t = localTimeMoment / (float) totalTime;
                Quaternion yaw = Quaternion.Slerp(startRotation, targetRotation, t);
                Vector3 point = GetCubicBezierPositionByTime(t, pointA, pointB, pointC, pointD);
                droneTrajectory[timeMoment].Position = point;
                droneTrajectory[timeMoment].YawAngle = yaw;
                droneTrajectory.LastStateIndex = timeMoment;
            }
            Debug.Log($"GenerateCubicBezierTrajectory, time end: {droneTrajectory.LastStateIndex}");
            return droneTrajectory;
        }

        private Vector3 GetQuadraticBezierPositionByTime(float Time, Vector3 A, Vector3 B, Vector3 C) 
        {
            Time = Mathf.Clamp01(Time);
            float invTime = 1 - Time;
            return (invTime * invTime * A)
                + (2 * invTime * Time * B)
                + (Time * Time * C);
        }
        private Vector3 GetCubicBezierPositionByTime(float Time, Vector3 A, Vector3 B, Vector3 C, Vector3 D) 
        {
            Time = Mathf.Clamp01(Time);
            float invTime = 1 - Time;
            return (invTime * invTime * invTime * A)
                + (3 * invTime * invTime * Time * B)
                + (3 * invTime * Time * Time * C)
                + (Time * Time * Time * D);
        }

        private float CalculateQuadraticBezierCurveLength(Vector3 A, Vector3 B, Vector3 C, int subdivisions = 100)
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

        private float CalculateCubicBezierCurveLength(Vector3 A, Vector3 B, Vector3 C, Vector3 D, int subdivisions = 100)
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

        public DroneTrajectory GenerateSinusTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        CmdArgumentsDTO cmdArguments)
        {
            throw new NotImplementedException("GenerateSinusTrajectory is not implemented yet.");
        }

        public DroneTrajectory GenerateParabolaTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        CmdArgumentsDTO cmdArguments)
        {
            throw new NotImplementedException("GenerateParabolaTrajectory is not implemented yet.");
        }
    }


}
