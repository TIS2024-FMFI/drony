using System;
using System.Collections;
using System.Collections.Generic;
using Drony.Entities;
using UnityEngine;
using Utility;

namespace Interpreter
{
    public class TrajectoryGenerator {
        private int MAX_FLIGHT_TIME = 3600000; // 1 hour
        private int TAKEOFF_SPEED = 1; // FIXME: add an option to set it in config file or in ui

        private List<DroneState> GenerateEmptyStates(int duration) 
        {
            List<DroneState> trajectory = new List<DroneState>();
            for (int i = 0; i <= duration; i++) {
                DroneState currentState = new DroneState(i);
                trajectory.Add(currentState);
            }
            return trajectory;
        }

        public DroneTrajectory initDroneTrajectory(string droneId) 
        {
            return new DroneTrajectory(droneId, GenerateEmptyStates(MAX_FLIGHT_TIME));
        }

        public DroneTrajectory SetInitialDronePosition(
                        DroneTrajectory droneTrajectory,
                        int timestamp,
                        List<object> cmdArguments)
        {
            Vector3 startPosition = (Vector3)cmdArguments[0];
            int timeFrom = 0;
            int timeTo = timestamp;
            for (int i = timeFrom; i < timeTo + 1; i++) {
                droneTrajectory[i].Position = startPosition;
                droneTrajectory.LastStateIndex = i;
            }
            return droneTrajectory;
        }

        public DroneTrajectory GenerateTakeOffTrajectory(
                        DroneTrajectory droneTrajectory,
                        int timestamp,
                        List<object> cmdArguments)
        {
            int height = (int)cmdArguments[0];

            DroneState lastState = droneTrajectory.getLastAdded();
            Vector3 lastPosition = lastState.Position;
            Vector3 destinationPosition = new Vector3(lastPosition.x, lastPosition.y + height, lastPosition.z);
            int defaultYaw = 0;  // FIXME: add possibility to set yaw in setPos command
            List<object> cmdArgumentsForLinear = new List<object> {destinationPosition, defaultYaw, TAKEOFF_SPEED};
            return GenerateLinearTrajectory(droneTrajectory, timestamp, cmdArgumentsForLinear);
        }
        
        public DroneTrajectory GenerateHoverTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timeFrom, 
                        int timeTo, 
                        DroneState exampleState) 
        {
            if (timeFrom > timeTo) {
                return droneTrajectory;
            }
            for (int i = timeFrom; i < timeTo + 1; i++) { // FIXME: timefrom + 1
                droneTrajectory[i] = new DroneState(exampleState.Position, exampleState.YawAngle, i);
                droneTrajectory.LastStateIndex = i;
            }
            return droneTrajectory;
        }

        public DroneTrajectory GenerateLinearTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        List<object> cmdArguments) 
        {
            Vector3 destinationPosition = (Vector3)cmdArguments[0];
            int destinationYaw = (int)cmdArguments[1];
            int speed = (int)cmdArguments[2];

            DroneState lastState = droneTrajectory.getLastAdded();
            int timeFrom = lastState.Time;
            int timeTo = timestamp;
            
            DroneTrajectory updatedDroneTrajectory = GenerateHoverTrajectory(
                new DroneTrajectory(droneTrajectory), 
                timeFrom, 
                timeTo, 
                lastState);

            Vector3 startPosition = lastState.Position;
            Quaternion startRotation = lastState.YawAngle;

            int distanceMeters = (int)Vector3.Distance(startPosition, destinationPosition);
            int distanceMillimeters = Utilities.ConvertFromMetersToMillimeters(distanceMeters);

            float totalTime = distanceMillimeters / speed;
            //int totalFrames = Mathf.CeilToInt(totalTime * resolution);
            Quaternion targetRotation = Quaternion.Euler(0, destinationYaw, 0);

            // interpolation between start and destination vectors
            for (int currentTime = timestamp; currentTime <= timestamp + totalTime; currentTime++)
            {
                float localTime = currentTime - timestamp;
                float t = localTime / (float)totalTime;
                Vector3 point = Vector3.Lerp(startPosition, destinationPosition, t);
                Quaternion yaw = Quaternion.Slerp(startRotation, targetRotation, t);
                updatedDroneTrajectory[currentTime].Position = point;
                updatedDroneTrajectory[currentTime].YawAngle = yaw;
                updatedDroneTrajectory.LastStateIndex = currentTime;
            }
            return updatedDroneTrajectory;
        }

        public DroneTrajectory GenerateCircularTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        List<object> cmdArguments)
        {
            throw new NotImplementedException("GenerateCircularTrajectory is not implemented yet.");
        }

        public DroneTrajectory GenerateSpiralTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        List<object> cmdArguments)
        {
            // Parse parameters
            Vector3 destinationPosition = (Vector3)cmdArguments[0];
            Vector3 pointA = (Vector3)cmdArguments[1];
            Vector3 pointB = (Vector3)cmdArguments[2];
            bool isClockwise = (bool)cmdArguments[3];
            int numberOfRevolutions = (int)cmdArguments[4];
            int speed = (int)cmdArguments[5];

            // Get the last state of the drone
            DroneState lastState = droneTrajectory.getLastAdded();
            Vector3 startPosition = lastState.Position;
            Quaternion startRotation = lastState.YawAngle;

            // Calculate the axis of rotation (Z-axis of the spiral)
            Vector3 axisZ = (pointB - pointA).normalized;

            // Calculate the total distance and time
            int distanceMeters = (int)Vector3.Distance(startPosition, destinationPosition);
            int distanceMillimeters = Utilities.ConvertFromMetersToMillimeters(distanceMeters);

            float totalTime = (distanceMillimeters / speed);
            float totalAngle = numberOfRevolutions * 2 * Mathf.PI; // Total angular rotation in radians

            // Initialize the updated trajectory with a hover phase
            int timeFrom = lastState.Time;
            int timeTo = timestamp;
            DroneTrajectory updatedDroneTrajectory = GenerateHoverTrajectory(
                new DroneTrajectory(droneTrajectory),
                timeFrom,
                timeTo,
                lastState);

            // Generate the spiral trajectory
            for (int currentTime = timestamp; currentTime <= timestamp + totalTime; currentTime++)
            {
                float localTime = currentTime - timestamp;
                float t = localTime / totalTime; // Normalized time [0, 1]

                // Interpolate along the spiral
                float height = Mathf.Lerp(startPosition.y, destinationPosition.y, t); // Linear interpolation for height
                float radius = Mathf.Lerp(0, Vector3.Distance(startPosition, axisZ), t); // Adjust radius based on progress
                float angle = (isClockwise ? -1 : 1) * totalAngle * t; // Adjust angle based on direction

                // Calculate spiral position
                Vector3 radialVector = radius * (Quaternion.AngleAxis(Mathf.Rad2Deg * angle, axisZ) * Vector3.right);
                Vector3 point = startPosition + radialVector + (axisZ * height);

                // Interpolate rotation
                Quaternion yaw = Quaternion.Slerp(startRotation, Quaternion.identity, t); // Adjust to match trajectory

                // Update the trajectory
                updatedDroneTrajectory[currentTime].Position = point;
                updatedDroneTrajectory[currentTime].YawAngle = yaw;
                updatedDroneTrajectory.LastStateIndex = currentTime;
            }

            return updatedDroneTrajectory;
}


        public DroneTrajectory GenerateSinusTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        List<object> cmdArguments)
        {
            throw new NotImplementedException("GenerateSinusTrajectory is not implemented yet.");
        }

        public DroneTrajectory GenerateParabolaTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        List<object> cmdArguments)
        {
            throw new NotImplementedException("GenerateParabolaTrajectory is not implemented yet.");
        }

        public DroneTrajectory GeneratePointsTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        List<object> cmdArguments)
        {
            throw new NotImplementedException("GeneratePointsTrajectory is not implemented yet.");
        }


    }


}
