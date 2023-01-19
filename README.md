# mars_rover

Another Snow College coding competition game. Get your rover to the destination before everyone else!

## Terms

| Term  | Definition                                                        |
| ----- | ----------------------------------------------------------------- |
| Map   | Collection of cells representing the martian landscape            |
| Rover | The Perseverance rover, the car driving around the moon           |
| Cell  | Representation of an area of ground with a damage score from 0-50 |

## Features - Perseverance Rover

- Map of unknown size
- Rover has a health value, once health gets to zero your rover is dead and immovable.
- Terrain can vary from safe to rough, driving over rougher terrain damages the rover more.
- A low-res map is given to each player at the beginning of the game
- As your rover moves you can see high-res terrain info (its location + 2 cells in each direction, 25 cells total)
- Rover can turn right, turn left, go forward, go backward (no diagonal movement)
- As your rover moves over terrain, its health score decreases by the terrain score

### Endpoints

| Request URL                                                                        | Response                                                                                                                                                                                                  |
| ---------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| /join?name={playerName}                                                            | {token}                                                                                                                                                                                                   |
| /status?token={token}                                                              | {state: waiting} <br> - OR - <br> { state: playing,<br>startingRow: int,<br>startingCol: int,<br>perseverenceHealth: int,<br> targetRow: int,<br>targetCol: int } <br> - OR - <br> {state: gameOver} |
| /move/perseverance?direction=\_\_\_ <br> (direction is Right, Left, Forward, Back) | {sight: \[cell\], newHealth: int} <br> - OR - <br> {error: "Rate limit exceeded"} <br> - OR - <br> {error: "Invalid direction"} <br> - OR - <br> {error: "You're dead"}                                  |

## Future Enhancement - Ingenuity Helicopter

- Ingenuity starts off with less health than rover
- Ingenuity can see current location + 5 cells in each direction
- Ingenuity can move in any direction (including diagonal), 2x rate limit of rover
- Send movement commands (W, NW, N, NE, E, SE, S, SW, Hover)
- If no movement command sent within 1 second Ingenuity lands, its health is reduced by the terrain score

### Endpoints for Ingenuity
