namespace Barker.Interfaces
{
    public class OccuranceMetric
    {
        public OccuranceMetric(string hashtag, long count)
        {
            Hashtag = hashtag;
            Count = count;
        }

        public string Hashtag { get; }
        public long Count { get; } 
    }
}