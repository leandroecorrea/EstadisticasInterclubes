namespace Core.Entities
{
    public record TeamsMatch
    {
        public string TeamOne { get; set; }
        public string TeamTwo { get; set; }
        public int TeamOneScore { get; set; }
        public int TeamTwoScore { get; set; }
    }
}