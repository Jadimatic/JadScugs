using SlugBase.DataTypes;

namespace JadScugs;

public static class BCPuppetEnums
{
    public static SlugcatStats.Name BCPuppet = new("puppet");
    public static class Color
    {
        public static PlayerColor Body;
        public static PlayerColor Cloth;
    }

    public static void RegisterValues()
    {
        Color.Body = new PlayerColor("Body Primary");
        Color.Cloth = new PlayerColor("Cloth Primary");
    }
}