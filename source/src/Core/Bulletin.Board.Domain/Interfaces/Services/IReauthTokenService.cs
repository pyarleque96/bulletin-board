namespace Bulletin.Board.Domain.Interfaces.Services;

public interface IReauthTokenService
{
    string IssueToken(Guid userId);
    bool ValidateAndConsume(string token, Guid userId);
}
