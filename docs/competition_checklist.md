# Competition Checklist

## Pre-Competition

- [] Reserve room
- [] Order pizza, arrange for pickup
- [] Vertically scale the app service
- [] Create new maps
  - Find new imagery from [NASA](https://www.nasa.gov/perseverance/images), save in `resources` folder.
  - Crop images to 500x500ish or 750x750ish, save as .png. **Make sure the image is square.**
  - Place images in `src/Mars.Web/wwwroot/images` with naming format `terrain_XX.png`
  - Process image
    - Make json file: `python tools/convertMap.py --file path/to/image --size 750 --out-type json`
    - Make csv file: `python tools/convertMap.py --file path/to/image --size 750 --out-type csv`
  - Place csv and json files in `src/Mars.Web/data`
  - Increase local appsettings.json `MaxMaps` value to include new images
  - Run app locally to generate new lowres json files
  - Commit & push **just before competition**
- [] Review app service configuration values
  - ApiLimitPerSecond
  - MaxMaps
  - GAME_PASSWORD
- [] Have your list of maps/coordinates ready for each round
- [] Get the prizes ready

### For each round:

- Create a new game on the server
  - Select the map
  - Paste in the target locations for that round
  - Set the starting battery level
  - Start game to allow joining
- Once everyone has joined, run the start-game.ps1 script  
  This will set the recharge-per-second rate and begin game play.
- Record top players from each round

## Post Competition

- [] Scale down the app service
- [] Put the password back to 'password'
