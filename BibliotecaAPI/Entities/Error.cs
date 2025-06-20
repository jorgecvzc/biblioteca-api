namespace BibliotecaAPI.Entities
{
    public class Error
    {
        public Guid Id { get; set; }
        public required string MensajeDeError { get; set; }
        public string? StackTrace { get; set; }
        public DateTime Instante { get; set; }
    }
}
