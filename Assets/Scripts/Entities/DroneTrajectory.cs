using UnityEngine;
using System;
using System.Collections.Generic;
using Interpreter;

namespace Drony.Entities
{
    public class DroneTrajectory 
    {
        public string DroneId { get; set; }
        public List<DroneState> Trajectory { get; set; }
        public int LastStateIndex { get; set; }

        public DroneTrajectory(string droneID) 
        {
            DroneId = droneID;
            Trajectory = new List<DroneState>();
            LastStateIndex = 0;
        }

        public DroneTrajectory(string droneID, DroneState initialState)
        {
            DroneId = droneID;
            Trajectory = new List<DroneState>() { initialState };
            LastStateIndex = 0;
        }

        public DroneTrajectory(string droneID, List<DroneState> emptyTrajectory) 
        {
            DroneId = droneID;
            Trajectory = emptyTrajectory;
            LastStateIndex = 0;  // the trajectory is empty, so there is no need to append LastStateIndex
        }

        public DroneTrajectory(DroneTrajectory other)
        {
            DroneId = other.DroneId;
            LastStateIndex = other.LastStateIndex;
            Trajectory = new List<DroneState>();
            for (int i = 0; i < other.Count; i++) {
                Trajectory.Add(other.Trajectory[i]);
            }
        }

        public void addTrajectory(List<Vector3> newTrajectory) 
        {
            for (int i = 0; i < newTrajectory.Count; i++) {
                Trajectory.Add(new DroneState(newTrajectory[i]));
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

        public DroneState getLastAdded() {
            return this[LastStateIndex];
        }

        public DroneState getNext(int playbackSpeed, int currentStateIndex) {
            
            if (currentStateIndex < 0 || currentStateIndex >= Trajectory.Count) {
                // TODO: implement custom exception
                throw new ArgumentOutOfRangeException(nameof(currentStateIndex), "Index is out of range.");
            }
            DroneState next = Trajectory[currentStateIndex];
            
            return next;
        }
    }
}