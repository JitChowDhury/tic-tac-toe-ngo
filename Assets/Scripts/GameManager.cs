using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public event EventHandler<OnClickedGridPositionEventArgs> OnClickedGridPosition;
    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChanges;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;

    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
    }
    public class OnClickedGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle,
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB,
    }
    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;

    }

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();

    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one Gamamanger exist");
        }
        Instance = this;
        playerTypeArray = new PlayerType[3, 3];

        lineList = new List<Line>
        {
            //horizontal
            new Line{
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0),new Vector2Int(1,0),new Vector2Int(2,0), },
                centerGridPosition=new Vector2Int(1,0),
                orientation=Orientation.Horizontal,
            },
            new Line{
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,1),new Vector2Int(1,1),new Vector2Int(2,1), },
                centerGridPosition=new Vector2Int(1,1),
                orientation=Orientation.Horizontal,
            },
            new Line{
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,2),new Vector2Int(1,2),new Vector2Int(2,2), },
                centerGridPosition=new Vector2Int(1,2),
                orientation=Orientation.Horizontal,
            },
            //vertical
            new Line{
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0),new Vector2Int(0,1),new Vector2Int(0,2), },
                centerGridPosition=new Vector2Int(0,1),
                orientation=Orientation.Vertical,
            },
            new Line{
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(1,0),new Vector2Int(1,1),new Vector2Int(1,2), },
                centerGridPosition=new Vector2Int(1,1),
                orientation=Orientation.Vertical,
            },
            new Line{
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(2,0),new Vector2Int(2,1),new Vector2Int(2,2), },
                centerGridPosition=new Vector2Int(2,1),
                orientation=Orientation.Vertical,
            },

            //diagonnals
            new Line{
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0),new Vector2Int(1,1),new Vector2Int(2,2), },
                centerGridPosition=new Vector2Int(1,1),
                orientation=Orientation.DiagonalA,
            },
            new Line{
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,2),new Vector2Int(1,1),new Vector2Int(2,0), },
                centerGridPosition=new Vector2Int(1,1),
                orientation=Orientation.DiagonalB,
            },
        };
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnCurrentPlayablePlayerTypeChanges?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }


    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType.Value)
        {
            return;
        }
        if (playerTypeArray[x, y] != PlayerType.None)
        {
            return;
        }

        playerTypeArray[x, y] = playerType;
        OnClickedGridPosition?.Invoke(this, new OnClickedGridPositionEventArgs { x = x, y = y, playerType = playerType, });


        switch (currentPlayablePlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }
        TestWinner();
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
        );
    }

    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return aPlayerType != PlayerType.None && aPlayerType == bPlayerType && bPlayerType == cPlayerType;
    }


    private void TestWinner()
    {

        foreach (Line line in lineList)
        {
            if (TestWinnerLine(line))
            {
                Debug.Log("Winner");
                currentPlayablePlayerType.Value = PlayerType.None;
                OnGameWin?.Invoke(this, new OnGameWinEventArgs
                {
                    line = line
                });
            }
        }
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }
}
