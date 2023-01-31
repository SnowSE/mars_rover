using System.Collections.Generic;
using System.Linq;

namespace Mars.MissionControl.Tests;

public class StartPlayingTests
{
    [Test]
    public void StartPlaying()
    {
        var scenario = new GameScenario(height: 7, width: 7, players: 1);
        scenario.Game.PlayGame();
        scenario.Game.GameState.Should().Be(GameState.Playing);
    }

    [Test]
    public void Player1CanMoveAndGetBackBoardDetails()
    {
        var scenario = new GameScenario(height: 7, width: 7, players: 1);
        scenario.Game.PlayGame();
        var direction = scenario.Players[0].PlayerLocation switch
        {
            { Column: 0 } => Direction.Right,
            { Row: 0 } => Direction.Forward,
            _ => Direction.Forward
        };
        var moveResult = scenario.Game.MovePerseverance(scenario.Players[0].Token, direction);
        moveResult.Should().NotBeNull();
    }


    [Test]
    public void TurningCostsOneBatteryPoint()
    {
        var scenario = new GameScenario(height: 7, width: 7, players: 1);
        scenario.Game.PlayGame();
        var origBattery = scenario.Players[0].BatteryLevel;
        var moveResult = scenario.Game.MovePerseverance(scenario.Players[0].Token, Direction.Right);
        var newBattery = moveResult.BatteryLevel;
        newBattery.Should().Be(origBattery - 1);
    }

    [Test]
    public void WhenAPlayerMovesItsBatteryIsReducedByTheCellDifficultyAmount()
    {
        var scenario = new GameScenario(height: 7, width: 7, players: 1);
        scenario.Game.PlayGame();
        //turn towards the target
        var target = scenario.Players[0].TargetLocation;
        var currentDirection = scenario.Players[0].Direction;
        var currentLocation = scenario.Players[0].PlayerLocation;
        var amAboveTarget = (currentLocation.Row > target.Row);
        var amRightOfTarget = (currentLocation.Column > target.Column);

        var direction = scenario.Players[0].PlayerLocation switch
        {
            { Column: 0 } => Direction.Right,
            { Row: 0 } => Direction.Forward,
            _ => Direction.Forward
        };
        var moveResult = scenario.Game.MovePerseverance(scenario.Players[0].Token, direction);
        Assert.Fail("this one isn't done yet");
    }
}

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
