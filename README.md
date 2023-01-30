[![Board Status](https://dev.azure.com/sn0/6b8f452c-c20f-44c6-a6e3-bbd6e2e3bb69/2e6df97a-2fc1-4faa-b6d8-55ac9875d901/_apis/work/boardbadge/cce13adf-1d2f-486b-b006-dfbdfcfe7123)](https://dev.azure.com/sn0/6b8f452c-c20f-44c6-a6e3-bbd6e2e3bb69/_boards/board/t/2e6df97a-2fc1-4faa-b6d8-55ac9875d901/Microsoft.RequirementCategory/)

[![Release to prod](https://github.com/SnowSE/mars_rover/actions/workflows/deploy-main-branch.yaml/badge.svg)](https://github.com/SnowSE/mars_rover/actions/workflows/deploy-main-branch.yaml)

# mars_rover

Another Snow College coding competition game. Get your rover to the destination before everyone else!

## Object of the game

Get your rover to the target first!

## Terms

| Term  | Definition                                                        |
| ----- | ----------------------------------------------------------------- |
| Map   | Collection of cells representing the martian landscape            |
| Rover | The Perseverance rover, the car driving around the moon           |
| Cell  | Representation of an area of ground with a damage score from 0-50 |

## Features - Perseverance Rover

- Map of unknown size
- Rover has a battery level, moving over terrain will drain your battery based on the difficulty of the terrain.  If you run out of battery you cannot move and have to wait for your solar panels to charge your batteries.  Your batteries will charge at the rate of x points per second.
- Terrain can vary from smooth to difficult, driving over more difficult terrain drains the the rover battery more.
- A low-res map is given to each player at the beginning of the game.  From this you can learn the overall size of the board and, on average, what parts of the board are more or less difficult.
- As your rover moves you can see high-res terrain info (its location + 2 cells in each direction, 25 cells total).
- Rover can turn right, turn left, go forward, go backward (no diagonal movement)
- As your rover moves over terrain, its battery level decreases by the terrain difficulty

### Endpoints

[Swagger API Page](https://snow-rover.azurewebsites.net/swagger/index.html)

## Future Enhancement - Ingenuity Helicopter

- Ingenuity starts off with a certain amount of battery power
- Moving over a cell costs 1 battery point
- Ingenuity can see current location + 5 cells in each direction
- Ingenuity can move in any direction (including diagonal) by giving it a destination within 2 cells of its current location
- If no movement command sent within 1 second Ingenuity lands, no battery is used while landed
- When the battery dies, you're dead.  Ingenuity does not recharge.
