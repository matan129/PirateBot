namespace SarcasticBot
{
    public interface ITarget
    {
        ScoreStruct GetScore(Group origin, out Path path, bool isFast);
    }
}