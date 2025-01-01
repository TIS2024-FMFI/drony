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
        private int TAKEOFF_SPEED = 1; // FIXME: add an option to set it in config file or in ui
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

        public DroneTrajectory GenerateTakeOffTrajectory(
                        DroneTrajectory droneTrajectory,
                        int timestamp,
                        CmdArgumentsDTO cmdArguments)
        {
            int height = cmdArguments.DestinationHeight;

            DroneState lastState = droneTrajectory.getLastAdded();
            Vector3 lastPosition = lastState.Position;
            Vector3 destinationPosition = new Vector3(lastPosition.x, lastPosition.y + height, lastPosition.z);
            int defaultYaw = (int)lastState.YawAngle.eulerAngles.y; // FIXME: velmi blbe riesenie, treba prerobit

            CmdArgumentsDTO cmdArgumentsForLinear = new CmdArgumentsDTO(); 
            cmdArgumentsForLinear.DestinationPosition = destinationPosition;
            cmdArgumentsForLinear.DestinationYaw = defaultYaw;
            cmdArgumentsForLinear.Speed = TAKEOFF_SPEED;

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
            Vector3 destinationPosition = cmdArguments.DestinationPosition;
            int destinationYaw = cmdArguments.DestinationYaw;
            int speed = cmdArguments.Speed;

            DroneState lastState = droneTrajectory.getLastAdded();
            int timeLastStateEndPlusOne = lastState.Time + 1;
            int timeLinearTrajectoryStart = timestamp;

            DroneTrajectory updatedDroneTrajectory;

            if (timeLastStateEndPlusOne > timeLinearTrajectoryStart) {
                // The last trajectory finished after the time, the next trajectory is starting.
                // The next trajectory will start from the state at a time timeLinearTrajectoryStart
                // The next trajectory will be bezier trajectory with 3 points 
                // TODO: do this shit, not implemented yet
                updatedDroneTrajectory = droneTrajectory;

                
            } else if (timeLastStateEndPlusOne < timeLinearTrajectoryStart) {
                // There is a gap between two trajectories
                // The drone will be hovering between execution of these two trajectories
                updatedDroneTrajectory = GenerateHoverTrajectory(
                                new DroneTrajectory(droneTrajectory), 
                                timeLastStateEndPlusOne, 
                                timeLinearTrajectoryStart, 
                                lastState
                            );
            } else {
                // The next trajectory starting right after the last one
                updatedDroneTrajectory = droneTrajectory;
            }
            
            

            Vector3 startPosition = lastState.Position;
            Quaternion startRotation = lastState.YawAngle;

            int distanceMeters = (int)Vector3.Distance(startPosition, destinationPosition);
            int distanceMillimeters = Utilities.ConvertFromMetersToMillimeters(distanceMeters);

            int totalTime = distanceMillimeters / speed;
            Quaternion targetRotation = Quaternion.Euler(0, destinationYaw, 0);

            // interpolation between start and destination vectors
            int duration = timestamp + totalTime;
            for (int timeMoment = timestamp; timeMoment < duration; timeMoment++)
            {
                int localTimeMoment = timeMoment - timestamp;
                float t = localTimeMoment / (float) totalTime;
                Vector3 point = Vector3.Lerp(startPosition, destinationPosition, t);
                Quaternion yaw = Quaternion.Slerp(startRotation, targetRotation, t);
                updatedDroneTrajectory[timeMoment].Position = point;
                updatedDroneTrajectory[timeMoment].YawAngle = yaw;
                updatedDroneTrajectory.LastStateIndex = timeMoment;
            }
            
            Debug.Log($"GenerateLinearTrajectory, time end: {droneTrajectory.LastStateIndex}");
            return updatedDroneTrajectory;
        }

        public DroneTrajectory GenerateCircularTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        CmdArgumentsDTO cmdArguments)
        {
            throw new NotImplementedException("GenerateCircularTrajectory is not implemented yet.");
        }

        public DroneTrajectory GenerateSpiralTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        CmdArgumentsDTO cmdArguments)
        {
            // Parse parameters
            Vector3 destinationPosition = cmdArguments.DestinationPosition;
            Vector3 pointA = cmdArguments.PointA;
            Vector3 pointB = cmdArguments.PointB;
            bool isClockwise = cmdArguments.IsClockwise;
            int numberOfRevolutions = cmdArguments.NumberOfRevolutions;
            int speed = cmdArguments.Speed;

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

        public DroneTrajectory GeneratePointsTrajectory(
                        DroneTrajectory droneTrajectory, 
                        int timestamp, 
                        CmdArgumentsDTO cmdArguments)
        {
            throw new NotImplementedException("GeneratePointsTrajectory is not implemented yet.");
        }


    }


}
