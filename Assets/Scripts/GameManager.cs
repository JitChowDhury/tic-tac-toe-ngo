using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event EventHandler<OnClickedGridPositionEventArgs> OnClickedGridPosition;
    public class OnClickedGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
    }
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one Gamamanger exist");
        }
        Instance = this;
    }
    public void ClickedOnGridPosition(int x, int y)
    {
        OnClickedGridPosition?.Invoke(this, new OnClickedGridPositionEventArgs { x = x, y = y, });
        Debug.Log("Clicked Grid Position " + x + " , " + y);
    }
}
