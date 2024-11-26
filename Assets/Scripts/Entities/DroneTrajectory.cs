using UnityEngine;
using System;
using System.Collections.Generic;
using Utility;

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

        public DroneTrajectory(string droneID, List<DroneState> initialTrajectory) 
        {
            DroneId = droneID;
            Trajectory = initialTrajectory;
            CurrentStateIndex = 0;
        }

        public void addTrajectory(List<DroneState> newTrajectory) 
        {
            if (newTrajectory.Count == 0) {
                return;
            }
            if (Trajectory.Count == 0) {
                Trajectory.AddRange(newTrajectory);
                return;
            } 
            int lastTime = Trajectory[Trajectory.Count - 1].Time;
            int newStartTime = newTrajectory[0].Time;
            if (lastTime < newStartTime) {
                List<DroneState> hoverStates = 
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

        public DroneState getNext(int playbackSpeed) {
            int gapInMillis = Utilities.ConvertFromPlaybackSpeedToMillisGap(playbackSpeed);
            if (CurrentStateIndex < 0 || CurrentStateIndex >= Trajectory.Count) {
                // TODO: implement custom exception
                throw new ArgumentOutOfRangeException(nameof(CurrentStateIndex), "Index is out of range.");
            }
            DroneState next = Trajectory[CurrentStateIndex];
            CurrentStateIndex += gapInMillis;
            return next;
        }
    }
}