using Mars.MissionControl.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mars.MissionControl.Tests
{
    internal class MoveTests
    {
        [Test]
        public void CanMoveForwardAndReverse()
        {
            var game = new Game();
            var token = game.Join("p1");
            game.StartGame();
            Location initialLocation = game.GetPlayerLocation(token);
            Location newLocation;
            if (initialLocation.Row < game.Board.Height - 1)
            {
                game.Move(token, Direction.Forward);
                newLocation = game.GetPlayerLocation(token);
                newLocation.Should().Be(new Location(initialLocation.Row + 1, initialLocation.Column));
                
                game.Move(token, Direction.Reverse);
                newLocation = game.GetPlayerLocation(token);
                newLocation.Should().Be(initialLocation);
            }
            else
            {
                game.Move(token, Direction.Reverse);
                newLocation = game.GetPlayerLocation(token);
                newLocation.Should().Be(new Location(initialLocation.Row - 1, initialLocation.Column));

                game.Move(token, Direction.Forward);
                newLocation = game.GetPlayerLocation(token);
                newLocation.Should().Be(initialLocation);
            }
        }

        [Test]
        public void CanMoveLeftAndRight()
        {
            var game = new Game();
            var token = game.Join("p1");
            game.StartGame();
            Location initialLocation = game.GetPlayerLocation(token);
            Location newLocation;
            if (initialLocation.Column < game.Board.Width - 1)
            {
                game.Move(token, Direction.Right);
                newLocation = game.GetPlayerLocation(token);
                newLocation.Should().Be(new Location(initialLocation.Row, initialLocation.Column + 1));

                game.Move(token, Direction.Left);
                newLocation = game.GetPlayerLocation(token);
                newLocation.Should().Be(initialLocation);
            }
            else
            {
                game.Move(token, Direction.Left);
                newLocation = game.GetPlayerLocation(token);
                newLocation.Should().Be(new Location(initialLocation.Row, initialLocation.Column - 1));

                game.Move(token, Direction.Right);
                newLocation = game.GetPlayerLocation(token);
                newLocation.Should().Be(initialLocation);
            }
        }

        [Test]
        public void MovingRoverOffBoardThrowsInvalidMoveException()
        {
            var game = new Game();
            var token = game.Join("p1");
            game.StartGame();
            Location location = game.GetPlayerLocation(token);
            while (location.Row < game.Board.Height - 1)
            {
                location = game.Move(token, Direction.Forward);
            }
            Assert.Throws<InvalidMoveException>(() => game.Move(token, Direction.Forward));
        }
    }
}
