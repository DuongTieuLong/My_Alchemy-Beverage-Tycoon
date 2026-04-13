using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public UIControlWithLocation uiControlWithLocation;
    public PlayerLocationController playerLocation;

    private void OnEnable()
    {
        playerLocation.onPlayerLocationChange += UpdateForNewLocation;
    }

    private void OnDisable()
    {
        playerLocation.onPlayerLocationChange -= UpdateForNewLocation;
    }

    public void UpdateForNewLocation(PlayerLocation currentLocation)
    {
        uiControlWithLocation.RefreshUIWithLocation(currentLocation);
    }
}
