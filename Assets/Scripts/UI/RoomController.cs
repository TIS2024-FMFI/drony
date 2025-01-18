using UnityEngine;

public class RoomController : MonoBehaviour
{
    public GameObject wall1;
    public GameObject wall2;
    public GameObject wall3;
    public GameObject wall4;
    public GameObject floor;
    public GameObject roof;

    public void ResizeRoom(float width, float depth, float height)
    {

        // Zmena velkosti podlahy a stropu
        floor.transform.localScale = new Vector3(width, 1, depth);
        floor.transform.localPosition = new Vector3(0, 0, 0);

        roof.transform.localScale = new Vector3(width, 1, depth);
        roof.transform.localPosition = new Vector3(0, height, 0);


        // bocna stena
        wall1.transform.localScale = new Vector3(width, height, 1);
        wall1.transform.localPosition = new Vector3(0, height / 2, depth / 2);

        // pred stena
        wall2.transform.localScale = new Vector3(depth, height, 1);
        wall2.transform.localPosition = new Vector3(width / 2, height / 2, 0);

        // bocna stena
        wall3.transform.localScale = new Vector3(width, height, 1);
        wall3.transform.localPosition = new Vector3(0, height / 2, -depth / 2);

        // zad stena
        wall4.transform.localScale = new Vector3(depth, height, 1);
        wall4.transform.localPosition = new Vector3(-width / 2, height / 2, 0);
    }

    public void ChangeWallColor(GameObject wall, Color color)
    {

        Debug.LogWarning($"mame farbu dajaku asi");
        // Overenie a zmena farby materiálu
        var renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Použitie instance materiálu na zmenu farby
            renderer.material = new Material(renderer.material);
            renderer.material.color = color;
        }
        else
        {
            Debug.LogWarning($"Wall {wall.name} does not have a Renderer component!");
        }
    }

}
