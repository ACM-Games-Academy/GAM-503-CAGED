using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;

    void Start()
    {
        SpawnFloor();
        SpawnWall(true);  // true = left wall
        SpawnWall(false); // false = right wall
    }

    void SpawnFloor()
    {
        if (floorPrefab == null) return;

        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camBottom = cam.transform.position.y - (camHeight / 2f);

        Vector2 position = new Vector2(cam.transform.position.x, camBottom + 0.5f);
        Instantiate(floorPrefab, position, Quaternion.identity);
    }

    void SpawnWall(bool isLeft)
    {
        if (wallPrefab == null) return;

        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float camEdgeX = isLeft
            ? cam.transform.position.x - (camWidth / 2f)
            : cam.transform.position.x + (camWidth / 2f);

        Vector2 position = new Vector2(camEdgeX + (isLeft ? 0.5f : -0.5f), cam.transform.position.y);
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);

        // Scale the wall to match camera height and keep it narrow
        wall.transform.localScale = new Vector3(1f, camHeight, 1f); // Adjust X if wall still looks wide
    }

}
