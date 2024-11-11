using UnityEngine;
using System;
using System.Collections.Generic;

namespace Drony.Entities
{
    public class DroneTrajectory 
    {
        public string DroneId { get; set; }
        public List<DroneState> Trajectory { get; set; }
        public int CurrentStateIndex { get; set; }

        public DroneTrajectory(string droneID) 
        {
            DroneId = droneID;
            Trajectory = new List<DroneState>();
            CurrentStateIndex = 0;
        }

        public DroneTrajectory(string droneID, DroneState initialState)
        {
            DroneId = droneID;
            Trajectory = new List<DroneState>() { initialState };
            CurrentStateIndex = 0;
        }

        public DroneTrajectory(string droneID, List<DroneState> trajectory) 
        {
            DroneId = droneID;
            Trajectory = trajectory;
            CurrentStateIndex = 0;
        }

        public void addTrajectory(List<DroneState> trajectory) 
        {
            Trajectory.AddRange(trajectory);
        }

        public void addTrajectory(List<Vector3> trajectory) 
        {
            for (int i = 0; i < trajectory.Count; i++) {
                Trajectory.Add(new DroneState(trajectory[i]));
            }
        }

        public void addState(DroneState state) 
        {
            Trajectory.Add(state);
        }

        public int Count
        {
            get { return Trajectory.Count; }
        }

        public DroneState this[int index]
        {
            get
            {
                if (index < 0 || index >= Trajectory.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                
                return Trajectory[index];
            }
            set
            {
                if (index < 0 || index >= Trajectory.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                
                Trajectory[index] = value;
            }
        }
    }
}