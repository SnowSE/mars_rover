using System;
using System.Collections.Generic;

namespace Mars.MissionControl.Tests;

public class InsanelySimpleRoverDriver
{
    private Orientation currentOrientation;
    private Location currentLocation;
    private readonly Queue<Location> targets;
    private readonly PlayerToken token;
    private readonly Game game;
    private long batteryLevel;

    public InsanelySimpleRoverDriver(JoinResult player, Game game)
    {
        this.currentOrientation = player.Orientation;
        this.currentLocation = player.PlayerLocation;
        this.targets = new Queue<Location>(game.Targets);
        this.token = player.Token;
        this.game = game;
    }

    public Location CurrentLocation => currentLocation;
    public string LastMoveMessage { get; private set; }

    public void DriveUntilBatteryDies()
    {
        MakeMoves(int.MaxValue);
    }

    internal void MoveTo(Location destination)
    {
        while (true)
        {
            var direction = determineDirection(currentOrientation, currentLocation, destination);
            var moveResult = game.MovePerseverance(token, direction);
            LastMoveMessage = moveResult.Message;
            if (moveResult.Message.Contains("Insufficient battery"))
            {
                return;
            }

            currentOrientation = moveResult.Orientation;
            batteryLevel = moveResult.BatteryLevel;
            currentLocation = moveResult.Location;

            if (moveResult.Location == destination)
            {
                return;
            }
        }
    }

    internal void MakeMoves(int numberOfMoves)
    {
        var destination = targets.Dequeue();
        for (int i = 0; i < numberOfMoves; i++)
        {
            var direction = determineDirection(currentOrientation, currentLocation, destination);
            var moveResult = game.MovePerseverance(token, direction);
            if (moveResult.Message.Contains("Insufficient battery"))
            {
                return;
            }
            if (moveResult.Message == GameMessages.YouMadeItToTheTarget || moveResult.Message == GameMessages.YouMadeItToAllTheTargets)
            {
                if (targets.Count == 0)
                {
                    return;
                }
                destination = targets.Dequeue();
                Console.WriteLine();
                Console.WriteLine($"You made it to a target, {targets.Count} targets remain.");
            }

            currentOrientation = moveResult.Orientation;
            batteryLevel = moveResult.BatteryLevel;
            currentLocation = moveResult.Location;
        }
    }

    private Direction determineDirection(Orientation orientation, Location currentLocaiton, Location target)
    {
        var targetIsToTheRight = (target.X > currentLocaiton.X);
        if (target.Y == currentLocaiton.Y)
        {
            if (targetIsToTheRight)
            {
                if (orientation == Orientation.East)
                {
                    return Direction.Forward;
                }
                return Direction.Right;
            }
            if (orientation == Orientation.West)
            {
                return Direction.Forward;
            }
            return Direction.Left;
        }
        var targetIsAbove = target.Y > currentLocaiton.Y;
        if (targetIsAbove)
        {
            if (orientation == Orientation.North)
            {
                return Direction.Forward;
            }

            return Direction.Right;
        }
        if (orientation == Orientation.South)
        {
            return Direction.Forward;
        }
        return Direction.Right;
    }
}
