using System;
using UnityEngine;

public class PlayerLocationController : MonoBehaviour
{

    public Action<PlayerLocation> onPlayerLocationChange;

    public PlayerLocation currentLocation;

    private void Start()
    {
        SetShopLoacation();
    }

    public void SetShopLoacation()
    {
        SetNewLocation(PlayerLocation.Shop);
    }
    public void SetBrewLoacation()
    {
        SetNewLocation(PlayerLocation.Brew);
    }
    public void SetExploreLoacation()
    {
        SetNewLocation(PlayerLocation.Explore);
    }

    public void SetNewLocation(PlayerLocation newLocation)
    {
        if (currentLocation != newLocation)
            currentLocation = newLocation;

        onPlayerLocationChange.Invoke(currentLocation);
    }
}

[System.Serializable]
public enum PlayerLocation
{
    Shop,
    Brew,
    Explore,
}