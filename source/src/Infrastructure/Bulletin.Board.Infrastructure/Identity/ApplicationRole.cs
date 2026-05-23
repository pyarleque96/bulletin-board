using Microsoft.AspNetCore.Identity;

namespace Bulletin.Board.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
