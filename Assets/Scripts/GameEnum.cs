public enum GameState
{
    Invalid = -1,
    Initialization,
    HomeMenu,
    Navigation,
    Combat,
    Victory,
    GameOver,
    Credits,
    Start
}

public enum SpellType
{
    Invalid = -1,
    Attack,
    Heal,
    Neutral
}

public enum EffectType
{
    Invalid = -1,
    Direct,
    Dot
}

public enum BuffType
{
    Invalid = -1,
    Damage,
    Speed
}

public enum TargetingType
{
    Invalid = -1,
    Simple,
    Aoe
}