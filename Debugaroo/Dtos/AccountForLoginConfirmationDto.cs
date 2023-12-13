namespace Debugaroo.Dtos
{
    public partial class AccountForLoginConfirmationDto
    {
        public byte[] PasswordHash {get; set;} = Array.Empty<byte>();
        public byte[] PasswordSalt {get; set;} = Array.Empty<byte>();
    }
}