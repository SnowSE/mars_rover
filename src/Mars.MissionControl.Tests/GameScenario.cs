using System.Collections.Generic;
using System.Linq;

namespace Mars.MissionControl.Tests;

public class GameScenario
{
    public GameScenario(int height, int width, int players)
    {
        Game = new Game(new GameStartOptions
        {
            Height = height,
            Width = width,
            MapNumber = 1,
        });

        Players = new(Enumerable.Range(0, players)
                                .Select(p => Game.Join($"Player{p}")));
    }

    public Game Game { get; private set; }

    public List<JoinResult> Players { get; private set; }
}
