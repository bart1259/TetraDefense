using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStateManager
{
    private static GameStateManager _instance;
    public static GameStateManager Instance => GetInstance();


    public World world { get; private set; }
    public Economy Economy { get; private set; }
    public TowerPlacementService TowerPlacementService { get; private set; }
    public PathFindingService PathfindingService { get; private set; }
    public int PlayerLives = 5;

    public static GameStateManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new GameStateManager();
        }
        return _instance;
    }

    public GameStateManager()
    {
        world = new World(20,10);
        Economy = new Economy(300);
        PathfindingService = new PathFindingService(world);
        PathfindingService.SetStartingCells(new List<GridCoord> {
            new GridCoord(0,4),
            new GridCoord(0,5)
        });
        TowerPlacementService = new TowerPlacementService(world, PathfindingService);
    }

    public void TakeDamage(int amount)
    {
        PlayerLives -= amount;
        if (PlayerLives <= 0)
        {
            // Game over
            EventBus.Instance.Publish(new GameOverEvent());
        }
    }
}
