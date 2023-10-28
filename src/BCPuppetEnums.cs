using SlugBase.DataTypes;

namespace JadScugs;

public static class BCPuppetEnums
{
    public static SlugcatStats.Name BCPuppet = new("puppet");
    public static class Color
    {
        public static PlayerColor ClothPrimary;
        public static PlayerColor ClothSecondary;
        public static PlayerColor Pattern;
    }

    public static void RegisterValues()
    {
        Color.Pattern = new PlayerColor("Pattern");
        Color.ClothPrimary = new PlayerColor("Cloth Primary");
        Color.ClothSecondary = new PlayerColor("Cloth Secondary");
    }
}