namespace AsasKit.Application.Abstractions;
public interface ICurrentUser
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
}