namespace SusurroFunctions.Dtos
{
    internal class User
    {
        public string Name { get; set; }
        public byte[] Salt { get; set; }
        public byte[] PasswordHash { get; set; }
    }
}
