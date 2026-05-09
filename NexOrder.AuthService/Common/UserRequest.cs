namespace NexOrder.AuthService.Common
{
    public class UserRequest
    {
        public Guid UserId { get; set; }

        public string Email { get; set; } = string.Empty;
    }
}
