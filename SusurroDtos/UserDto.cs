namespace SusurroDtos
{
    public class UserDto
    {
        public string Name { get; set; }
        public byte[] Salt { get; set; }
        public byte[] PasswordHash { get; set; }
        public string PublicKey { get; set; }
    }
}
