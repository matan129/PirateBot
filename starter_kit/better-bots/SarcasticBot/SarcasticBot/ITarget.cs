namespace SarcasticBot
{
    public interface ITarget
    {
        ScoreStruct GetScore(Group origin, Path path = null, bool isFast = false);
    }
}