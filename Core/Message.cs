namespace ChatAPI.Core
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ProfessionalId { get; set; }
        public Guid BeneficiaryId { get; set; }
        public string? Text { get; set; }
        public Guid ThreadId { get; set; }
    }
}
