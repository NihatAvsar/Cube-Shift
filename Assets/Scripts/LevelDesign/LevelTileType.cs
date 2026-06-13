namespace CubeShift.LevelDesign
{
    public enum LevelTileType
    {
        Empty,
        Normal,
        Start,
        Goal,
        RedBreakable,
        BreakableExit,
        YellowButton,
        YellowDoor,
        GreenJumpUp,
        JumpLanding,
        IceDownStart,
        IceMiddle,
        IceLanding
    }

    [System.Flags]
    public enum LevelObjective
    {
        None = 0,
        RedBreakable = 1 << 0,
        YellowButton = 1 << 1,
        GreenJump = 1 << 2,
        IceSlide = 1 << 3
    }
}
