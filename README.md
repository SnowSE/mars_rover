[![Board Status](https://dev.azure.com/sn0/6b8f452c-c20f-44c6-a6e3-bbd6e2e3bb69/2e6df97a-2fc1-4faa-b6d8-55ac9875d901/_apis/work/boardbadge/cce13adf-1d2f-486b-b006-dfbdfcfe7123)](https://dev.azure.com/sn0/6b8f452c-c20f-44c6-a6e3-bbd6e2e3bb69/_boards/board/t/2e6df97a-2fc1-4faa-b6d8-55ac9875d901/Microsoft.RequirementCategory/)

[![Release to prod](https://github.com/SnowSE/mars_rover/actions/workflows/deploy-main-branch.yaml/badge.svg)](https://github.com/SnowSE/mars_rover/actions/workflows/deploy-main-branch.yaml)

# 2023 Snow College Coding Competition: Mars Rover

Object: Get your rover to the destination before everyone else!

## Summary of Game Mechanics

- Join the game and get a token that represents your user.  
- Review the low-resolution map of the surface of mars and make a guess of what route you might want to use to get to the target.  
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
  - Fly the Ingenuity helicopter by giving it a destination row/column within two cells of its current location, if you give it a destination more than two cells away it will ignore the command.
- There is a rate-limit applied to all game players which limits how many movement commands per second you can send to both Perseverance and Ingenuity.
- Winners are determined by the amount of time elapsed from when the game state becomes 'Playing' to when your rover makes it to the target.
- Tip: Make sure you use the high-resolution information you get back from Perseverance and Ingenuity about their surrounding cells to help improve your planning of how you will get to the target.

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
    if (!r.IsSuccessStatusCode)
    {
        Console.WriteLine("Unfortunately there was a problem joining that game. :(");
        Console.WriteLine("Error details:");
        Console.WriteLine(await r.Content.ReadAsStringAsync());
    }    
    ```

5) If it was successful, you can turn the JSON you got back in the response into a C# object for you to work with.

    ```c#
    var joinResponse = await r.Content.ReadFromJsonAsync<JoinResponse>();

    //hang on to these for later
    int ingenuityRow = joinResponse.StartingRow;
    int ingenuityCol = joinResponse.StartingColumn;
    ```

    The JoinResponse object tells you what your token is (you'll send that back to the server for every subsequent request), where your rover landed, where you want to go (the target/destination), a low-resolution map of the entire game area, and a high-resolution map of the cells around your rover.  

    <details>
    <summary>Click here for the definition of JoinResponse and its related classes</summary>

    ///JoinResponse is a class like this:
    public class JoinResponse
    {
        public string Token { get; set; }
        public int StartingRow { get; set; }
        public int StartingColumn { get; set; }
        public int TargetRow { get; set; }
        public int TargetColumn { get; set; }
        public Neighbor[] Neighbors { get; set; }
        public LowResolutionCell[] LowResolutionMap { get; set; }
        public string Orientation { get; set; }
    }

    public class Neighbor
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int Difficulty { get; set; }
    }

    public class LowResolutionCell
    {
        public int LowerLeftRow { get; set; }
        public int LowerLeftColumn { get; set; }
        public int UpperRightRow { get; set; }
        public int UpperRightColumn { get; set; }
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
        public int Row { get; set; }
        public int Column { get; set; }
        public int BatteryLevel { get; set; }
        public Neighbor[] Neighbors { get; set; }
        public string Message { get; set; }
        public string Orientation { get; set; }
    }
    ```

1) Move the Ingenuity helicopter by giving it a destination to fly to (within two cells of its current location)

    ```c#
    //move up:
    await moveHelicopter(ingenuityRow, ingenuityCol + 2);

    //move right 
    await moveHelicopter(ingenuityRow+2, ingenuityCol);
    
    async Task moveHelicopter(int row, int col)
    {
        var response = await httpClient.GetAsync($"/game/moveingenuity?token={joinResponse.Token}&destinationRow={row}&destinationColumn={col}");
        if (response.IsSuccessStatusCode)
        {
            var moveResponse = await response.Content.ReadFromJsonAsync<IngenuityMoveResponse>();
            ingenuityRow = moveResponse.Row;
            ingenuityCol = moveResponse.Column;

            //update your internal high-res map with moveResponse.Neighbors
        }
    }
    ```

1) Keep moving until the Perseverance row/col matches the Target row/col.  Remember, winners are based solely on elapsed time and the fastest way to get to the target is to make sure you don't have to wait for your battery to recharge, so be smart about the path you take to the target. :)
