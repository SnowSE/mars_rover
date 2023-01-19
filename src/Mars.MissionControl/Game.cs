namespace Mars.MissionControl;
public class Game
{
    public string Join(string playerName)
    {
        return IdGenerator.GetNextId();
        return Guid.NewGuid().ToString();
    }
}
