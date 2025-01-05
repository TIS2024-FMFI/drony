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
                        Vector3 startPosition,
                        Vector3 destinationPosition,
                        Quaternion startRotation,
                        Quaternion targetRotation,
                        int speed) 
        {
            Debug.Log($"GenerateLinearTrajectory, time start: {timestamp}");

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
                        Vector3 pointA,
                        Vector3 pointB,
                        Vector3 pointC,
                        Quaternion startRotation,
                        Quaternion targetRotation,
                        int speed)
        {
            Debug.Log($"GenerateQuadraticBezierTrajectory, time start: {timestamp}");

            float distanceMeters = MathOperations.CalculateQuadraticBezierCurveLength(pointA, pointB, pointC);
            int distanceMillimeters = Utilities.ConvertFromMetersToMillimeters(distanceMeters);
            int totalTime = distanceMillimeters / speed;

            int duration = timestamp + totalTime;
            for (int timeMoment = timestamp; timeMoment < duration; timeMoment++)
            {
                int localTimeMoment = timeMoment - timestamp;
                float t = localTimeMoment / (float) totalTime;
                Quaternion yaw = Quaternion.Slerp(startRotation, targetRotation, t);
                Vector3 point = MathOperations.GetQuadraticBezierPositionByTime(t, pointA, pointB, pointC);
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
                        Vector3 pointA,
                        Vector3 pointB,
                        Vector3 pointC,
                        Vector3 pointD,
                        Quaternion startRotation,
                        Quaternion targetRotation,
                        int speed)
        {
            Debug.Log($"GenerateCubicBezierTrajectory, time start: {timestamp}");

            float distanceMeters = MathOperations.CalculateCubicBezierCurveLength(pointA, pointB, pointC, pointD);
            int distanceMillimeters = Utilities.ConvertFromMetersToMillimeters(distanceMeters);
            int totalTime = distanceMillimeters / speed;

            int duration = timestamp + totalTime;
            for (int timeMoment = timestamp; timeMoment < duration; timeMoment++)
            {
                int localTimeMoment = timeMoment - timestamp;
                float t = localTimeMoment / (float) totalTime;
                Quaternion yaw = Quaternion.Slerp(startRotation, targetRotation, t);
                Vector3 point = MathOperations.GetCubicBezierPositionByTime(t, pointA, pointB, pointC, pointD);
                droneTrajectory[timeMoment].Position = point;
                droneTrajectory[timeMoment].YawAngle = yaw;
                droneTrajectory.LastStateIndex = timeMoment;
            }
            Debug.Log($"GenerateCubicBezierTrajectory, time end: {droneTrajectory.LastStateIndex}");
            return droneTrajectory;
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
