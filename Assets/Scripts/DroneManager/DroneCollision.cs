using UI;
using UnityEngine;

namespace DroneManager
{
    public class DroneCollision : MonoBehaviour
    {
        void OnCollisionEnter(Collision collision)
        {
            var panelController = CollisionWarningController.Instance;
            panelController.Show(name, collision.gameObject.name);
        }
    }
}