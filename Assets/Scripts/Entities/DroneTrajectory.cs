using UnityEngine;
using System;
using System.Collections.Generic;

namespace Entities
{
    public class DroneTrajectory 
    {
        public List<DroneState> Trajectory { get; set; }

        public DroneTrajectory() 
        {
            Trajectory = new List<DroneState>();
        }

        public DroneTrajectory(DroneState initialState)
        {
            Trajectory = new List<DroneState>() { initialState };
        }

        public DroneTrajectory(List<DroneState> trajectory) 
        {
            Trajectory = trajectory;
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