namespace Core.Entities
{
    public class Result
    {
        public Guid Id { get; set; }
        public static readonly Guid TIE_RESULT = Guid.Empty;
        public Guid WinnerId { get; set; }
        public int HolesWon { get; set; }
        public int HolesRemaining { get; set; }
    }
}