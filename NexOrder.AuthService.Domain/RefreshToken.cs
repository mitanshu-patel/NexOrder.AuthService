using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.AuthService.Domain
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;

        public string JwtId { get; set; } = null!;

        public DateTime ExpiryDate { get; set; }

        public bool Invalidated { get; set; }

        public Guid UserId { get; set; }

        public string Email { get; set; }
    }
}
