using System;
using System.Collections;
using System.Collections.Generic;
using Drony.Entities;
using UnityEngine;

namespace Interpreter
{
    public class TrajectoryGenerator {
        private int resolution;

        public TrajectoryGenerator(int resolution = 1000) {
            this.resolution = resolution;
        }

        public List<DroneState> GenerateHoverTrajectory(Vector3 postion, int startYaw, int timestamp, int duration) {
            List<DroneState> trajectory = new List<DroneState>();
            Quaternion yaw = Quaternion.Euler(0, startYaw, 0);
            for (int i = 0; i <= duration; i++) {
                DroneState currentState = new DroneState(postion, yaw, timestamp + i);
                trajectory.Add(currentState);
            }
            return trajectory;
        }

        public List<DroneState> GenerateLinearTrajectory(Vector3 startPosition, Vector3 destinationPosition, int startYaw, int destinationYaw, int speed, int timestamp) {
            List<DroneState> trajectory = new List<DroneState>();
            float distance = Vector3.Distance(startPosition, destinationPosition);
            float totalTime = distance / speed;
            int totalFrames = Mathf.CeilToInt(totalTime * resolution);
            Quaternion startRotation = Quaternion.Euler(0, startYaw, 0);
            Quaternion targetRotation = Quaternion.Euler(0, destinationYaw, 0);

            // interpolation between start and destination vectors
            for (int i = 0; i <= totalFrames; i++)
            {
                float t = i / (float)totalFrames;
                Vector3 point = Vector3.Lerp(startPosition, destinationPosition, t);
                Quaternion yaw = Quaternion.Slerp(startRotation, targetRotation, t);
                DroneState currentState = new DroneState(point, yaw);
                trajectory.Add(currentState);
            }
            return trajectory;
        }

        public List<Vector3> GenerateCircularTrajectory(float radius, int numPoints)
        {
            List<Vector3> trajectory = new List<Vector3>();
            
            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * Mathf.PI * 2 / numPoints;  
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                trajectory.Add(new Vector3(x, 0, z));            
            }
            
            return trajectory;
        }
    }


}
