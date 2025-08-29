
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform crossPrefab;

    void Start()
    {
        GameManager.Instance.OnClickedGridPosition += GameManager_OnClickedGridPosition;
    }

    private void GameManager_OnClickedGridPosition(object sender, GameManager.OnClickedGridPositionEventArgs e)
    {
        SpawnObjectRpc(e.x, e.y);
        Debug.Log("GameManager_OnClickedGridPosition");
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y)
    {
        Debug.Log("SpawnObject");
        Transform spawnCrossTransform = Instantiate(crossPrefab);
        spawnCrossTransform.GetComponent<NetworkObject>().Spawn(true);
        spawnCrossTransform.position = GetWorldPosition(x, y);
    }

    private Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
