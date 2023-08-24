[![Board Status](https://dev.azure.com/sn0/6b8f452c-c20f-44c6-a6e3-bbd6e2e3bb69/2e6df97a-2fc1-4faa-b6d8-55ac9875d901/_apis/work/boardbadge/cce13adf-1d2f-486b-b006-dfbdfcfe7123)](https://dev.azure.com/sn0/6b8f452c-c20f-44c6-a6e3-bbd6e2e3bb69/_boards/board/t/2e6df97a-2fc1-4faa-b6d8-55ac9875d901/Microsoft.RequirementCategory/)

[![Release to prod](https://github.com/SnowSE/mars_rover/actions/workflows/deploy-main-branch.yaml/badge.svg)](https://github.com/SnowSE/mars_rover/actions/workflows/deploy-main-branch.yaml)

# 2023 Snow College Coding Competition: Mars Rover
 
> **Final competition date:** Friday, April 21st 2023 @ 4PM   
> Location: [Snow College Science Building, Room GRSC 124](https://goo.gl/maps/zKw4X9gioeNCKutZ9)

***Object: Get your rover to the destination(s) before everyone else!***

## Summary of Game Mechanics

- Join the game and get a token that represents your user.  
- Review the low-resolution map of the surface of mars and make a guess of what route you might want to use to get to the target(s).  
- Wait for the game to go from 'Joining' status to 'Playing' status so you can start moving your rover
- Perseverance (the rover)
  - Every cell on the board has a 'difficulty' value, which will reduce Perseverance's battery power.  
  - Perseverance can only move onto that cell if it has enough battery.  
  - Perseverance's battery will re-charge by a certain amount every second, so if you run out of battery and cannot move you just need to wait a bit and you'll be able to start moving again.
  - The Perseverance rover can see the 'difficulty' values of all the cells within <strong>two</strong> cells of itself.
  - The Perseverance rover can only move forward or back one cell at a time.  It can also turn right or left within its same cell (turning does not move the rover to another cell).
- Ingenuity (the helicopter)
  - The Ingenuity helicopter's battery does not recharge.  Once it runs out, it can no longer fly.  However, because it flies above the terrain, its battery is only decreased by distance it covers (not the difficulty of the terrain).
  - The Ingenuity helicopter can see the 'difficulty' values of all the cells within <strong>five</strong> cells of itself.
  - The Ingenuity helicopter can move up to two cells in any direction (including diagonally), so you can quickly scout out the terrain in front of the rover to help determine the most efficient path to the target.
  - Fly the Ingenuity helicopter by giving it a destination x/y within two cells of its current location, if you give it a destination more than two cells away it will ignore the command.
- There is a rate-limit applied to all game players which limits how many movement commands per second you can send to both Perseverance and Ingenuity.
- Winners are determined by the amount of time elapsed from when the game state becomes 'Playing' to when your rover makes it to *all* of the targets (if more than one).
- Tip: Make sure you use the high-resolution information you get back from Perseverance and Ingenuity about their surrounding cells to help improve your planning of how you will get to the target.

## Changes for the April 2023 Competetion

* [x] Each contestant is only allowed a single rover, but each rover has 10 helicopters
* [X] Impassable barriers / cliffs (require pathfinding)
* [X] Allow players to move along the border before gameplay begins
* [X] When all batteries get to 0, bump everyone's battery (to keep the game moving)
* [X] Hide the maps
* [X] Enforce the order of the targets

## Changes for the March 2023 Competition

* [x] Multiple waypoints ***(March 2023)***
* [x] Unbounded battery (you can charge beyond your initial battery level) ***(March 2023)***
* [x] Return battery level on join ***(March 2023)***

## Proposed changes for future competitions

* [ ] Change spawning locations
  - circle spawn (equidistant from target)
  - fair spawn (possibly different distances but equal best-path to target)
  - weighted spawn
* [ ] Edge wrapping
* [ ] Weather/storms changes difficulty values (so your previous map becomes invalidated, requiring constant scanning and re-evaluation)
* [ ] closing ring, as time passes boot players outside of a certain radius
* [ ] ***Change scoring: most efficient wins (battery used / time taken), within 60 seconds of first to target***

## Competition Coordinates
- map#6
 - (107, 166); (65, 700); (740, 160); (575, 643); (375, 525)
- map#7
 - (177, 236); (135, 740); (672, 172); (715, 613); (375, 375)
- Map#9 
 - (70,350); (263, 436); (273, 276); (425, 148) 
- Map#10 (make sure that goes fast)
 - (360, 115); (150, 80); (210, 440); (240, 220)

## API Documentation

There is an [API Playground](https://snow-rover.azurewebsites.net/swagger/index.html) where you can see exactly what endpoints exist on the server, what arguments need to be passed in, and what type of data you get back.  Feel free to use that to help you get familiar with how to interact with the server.  While you *can* play the game using nothing more than the API Playground, or a commandline with `curl` in bash or `Invoke-RestMethod` in PowerShell (or even from a few different browser tabs), you will have the best success by writing a program to automate your server interactions.

## Server Password

The password required to restart a game or begin playing is `password`.  If you go to https://snow-rover.azurewebsites.net you can even create your very own game where you can play with different settings without getting in anyone else's way.  Every game is automatically deleted after an hour, but you can continue to make new games.

## Detailed Game Mechanics

> There is an [console-based client](https://github.com/snow-jallen/MarsClient/blob/master/ConsoleClient/Program.cs) that you can use as a reference example.  

1) Head over to <https://snow-rover.azurewebsites.net> and join a game - the game ID will be the last part of the URL, and it will show up in the title of the page ("Mars Rover (Game 'g')", for example).
2) Create a HttpClient object that will let you send HTTP requests.  Unless you're hosting your own instance of the server, the server address will be "https://snow-rover.azurewebsites.net".

   ```c#
   var httpClient = new HttpClient { BaseAddress = new Uri("https://snow-rover.azurewebsites.net") };
   ```

3) Send a HTTP GET request to /game/join with a game ID and your name as query string parameters.  The game ID For example:

    ```c#
    var response = await httpClient.GetAsync($"/game/join?gameId={gameId}&name={name}");
    ```

4) Check to see if your request was successful or not.  

    ```c#
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine("Unfortunately there was a problem joining that game. :(");
        Console.WriteLine("Error details:");
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }    
    ```

5) If it was successful, you can turn the JSON you got back in the response into a C# object for you to work with.

    ```c#
    var joinResponse = await response.Content.ReadFromJsonAsync<JoinResponse>();

    //hang on to these for later
    int ingenuityX = joinResponse.StartingX;
    int ingenuityY = joinResponse.StartingY;
    ```

    The JoinResponse object tells you what your token is (you'll send that back to the server for every subsequent request), where your rover landed, where you want to go (the target/destination), a low-resolution map of the entire game area, and a high-resolution map of the cells around your rover.  

    <details>
    <summary>Click here for the definition of JoinResponse and its related classes</summary>

    ```C#
    ///JoinResponse is a class like this:
    public class JoinResponse
    {
        public string Token { get; set; }
        public int StartingX { get; set; }
        public int StartingY { get; set; }
        public Location[] Targets { get; set; }
        public Neighbor[] Neighbors { get; set; }
        public Lowresolutionmap[] LowResolutionMap { get; set; }
        public string Orientation { get; set; }
        public long BatteryLevel{ get; set; }
    }

    public class Location
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Neighbor
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Difficulty { get; set; }
    }

    public class LowResolutionCell
    {
        public int LowerLeftX { get; set; }
        public int LowerLeftY { get; set; }
        public int UpperRightX { get; set; }
        public int UpperRightY { get; set; }
        public int AverageDifficulty { get; set; }
    }
    ```

    </details>

6) Now that you've joined the game, you need to wait for the game state to change from 'Joining' to 'Playing'.  You can check the game state at any time by submitting a GET request to /game/status passing the token you got back from you join request as a query string parameter.  For example:

    ```c#
    while (true)
    {
        var statusResult = await httpClient.GetFromJsonAsync<StatusResult>($"/game/status?token={joinResponse.Token}");
        if (statusResult.status == "Playing")
            break;
        await Task.Delay(TimeSpan.FromSeconds(1));
    }

    public class StatusResult
    {
        public string status { get; set; }
    }
    ```

1) Now that the game is ready to play, you can start moving both the Perseverance rover, as well as the Ingenuity helicopter.  You move the Perseverance rover by sending it a command to either move forward, turn right, turn left, or move in reverse.  Something like this:

    ```c#
    var response = await httpClient.GetAsync($"/game/moveperseverance?token={joinResponse.Token}&direction={direction}");
    if (response.IsSuccessStatusCode)
    {
        var moveResult = await response.Content.ReadFromJsonAsync<MoveResponse>();
        // make decisions based on what you get back in moveResult.
    }

    public class MoveResponse
    {
        public int X { get; set; }
        public int Y { get; set; }
        public long BatteryLevel { get; set; }
        public Neighbor[] Neighbors { get; set; }
        public string Message { get; set; }
        public string Orientation { get; set; }
    }
    ```

1) Move the Ingenuity helicopter by giving it a destination to fly to (within two cells of its current location)

    ```c#
    //You have 10 helicopters you can fly, so you need to specify which one you're telling to move.
    int ingenuityId = 0;

    //move up:
    await moveHelicopter(ingenuityId, ingenuityX, ingenuityY + 2);

    //move right 
    await moveHelicopter(ingenuityId, ingenuityX+2, ingenuityY);
    
    async Task moveHelicopter(int id, int x, int y)
    {
        var response = await httpClient.GetAsync($"/game/moveingenuity?token={joinResponse.Token}&id={id}&destinationX={x}&destinationY={y}");
        if (response.IsSuccessStatusCode)
        {
            var moveResponse = await response.Content.ReadFromJsonAsync<IngenuityMoveResponse>();
            ingenuityX = moveResponse.X;
            ingenuityY = moveResponse.Y;

            //update your internal high-res map with moveResponse.Neighbors
        }
    }
    ```

1) Keep moving until the Perseverance x/y matches the Target x/y.  Remember, winners are based solely on elapsed time and the fastest way to get to the target is to make sure you don't have to wait for your battery to recharge, so be smart about the path you take to the target. :)
