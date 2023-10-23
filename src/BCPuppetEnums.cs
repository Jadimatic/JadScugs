using SlugBase.DataTypes;

namespace JadScugs;

public static class BCPuppetEnums
{
    public static SlugcatStats.Name BCPuppet = new("puppet");
    public static class Color
    {
        public static PlayerColor Cloth;
        public static PlayerColor Pattern;
    }

    public static void RegisterValues()
    {
        Color.Pattern = new PlayerColor("Pattern");
        Color.Cloth = new PlayerColor("Cloth Primary");
    }
}