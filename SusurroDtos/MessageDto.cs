namespace SusurroDtos
{
    public class MessageDto
    {
        public string Id { get; set; }
        public string From { get; set; }
        public DateTime CreateTime { get; set; }
        public string Password { get; set; }
        public string AllTo { get; set; }
        public string To { get; set; }
        public byte[] Text { get; set; }
        public byte[] Signature { get; set; }
    }
}
