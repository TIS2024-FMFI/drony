using UnityEngine;
using System;
using System.Collections.Generic;

namespace Drony.Entities
{
    public class DroneTrajectory 
    {
        public string DroneId { get; set; }
        public List<DroneState> Trajectory { get; set; }
        public List<DroneState> KeyStates {get; set; }
        public int LastStateIndex { get; set; } // index of the last state that was updated

        public DroneTrajectory() {
            KeyStates = new List<DroneState>();
        }

        public DroneTrajectory(DroneTrajectory other)
        {
            DroneId = other.DroneId;
            LastStateIndex = other.LastStateIndex;
            Trajectory = new List<DroneState>();
            for (int i = 0; i < other.Count; i++) {
                Trajectory.Add(other.Trajectory[i]);
            }
            KeyStates = new List<DroneState>();
            foreach (DroneState droneState in other.KeyStates) {
                KeyStates.Add(droneState);
            }
            
        }

        public void addTrajectory(List<Vector3> newTrajectory) 
        {
            for (int timeMoment = 0; timeMoment < newTrajectory.Count; timeMoment++) {
                DroneState newDroneState = new DroneState();
                newDroneState.Position = newTrajectory[timeMoment];
                Trajectory.Add(newDroneState);
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

        public void setLastAsKeyState() {
            Trajectory[LastStateIndex].IsKeyState = true;
            KeyStates.Add(Trajectory[LastStateIndex]);
            Debug.Log($"Set key state: {Trajectory[LastStateIndex].Time}");
        }
    }
}