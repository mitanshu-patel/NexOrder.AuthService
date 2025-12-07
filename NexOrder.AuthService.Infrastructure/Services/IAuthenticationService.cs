using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.AuthService.Infrastructure.Services
{
    public interface IAuthenticationService
    {
        string IssueJWT(string user);
    }
}
