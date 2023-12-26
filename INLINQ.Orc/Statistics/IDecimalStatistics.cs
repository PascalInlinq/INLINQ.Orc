namespace INLINQ.Orc.Statistics
{
    public interface IDecimalStatistics
    {
		decimal? Minimum { get; }
		decimal? Maximum { get; }
		decimal? Sum { get; }
    }
}
