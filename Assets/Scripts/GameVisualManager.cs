
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform lineCompletePrefab;

    private List<GameObject> visualGameObjectList;

    private void Awake()
    {
        visualGameObjectList = new List<GameObject>();
    }
    void Start()
    {
        GameManager.Instance.OnClickedGridPosition += GameManager_OnClickedGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        foreach (GameObject visualGameObject in visualGameObjectList)
        {
            Destroy(visualGameObject);
        }

        visualGameObjectList.Clear();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        float eulerZ = 0f;

        switch (e.line.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal: eulerZ = 0f; break;
            case GameManager.Orientation.Vertical: eulerZ = 90f; break;
            case GameManager.Orientation.DiagonalA: eulerZ = 45f; break;
            case GameManager.Orientation.DiagonalB: eulerZ = -45f; break;


        }


        Transform LineCompleteTransform = Instantiate(lineCompletePrefab, GetWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y), Quaternion.Euler(0, 0, eulerZ));
        LineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);
        visualGameObjectList.Add(LineCompleteTransform.gameObject);
    }

    private void GameManager_OnClickedGridPosition(object sender, GameManager.OnClickedGridPositionEventArgs e)
    {
        if (IsServer)
        {
            // Server spawns directly
            SpawnVisual(e.x, e.y, e.playerType);
        }
        else
        {
            // Clients request the server to spawn
            SpawnRequestRpc(e.x, e.y, e.playerType);
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnRequestRpc(int x, int y, GameManager.PlayerType playerType)
    {
        SpawnVisual(x, y, playerType); // Server spawns → auto syncs
    }

    private void SpawnVisual(int x, int y, GameManager.PlayerType playerType)
    {
        Transform prefab = playerType == GameManager.PlayerType.Cross ? crossPrefab : circlePrefab;

        Transform obj = Instantiate(prefab, GetWorldPosition(x, y), Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn(true);

        visualGameObjectList.Add(obj.gameObject);
    }

    private Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
