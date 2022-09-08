using Rumble.Platform.Common.Exceptions;
using Rumble.Platform.Common.Models;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ArrangeConstructorOrDestructorBody

namespace Dmz.Exceptions;

public class AccountNotFoundException : PlatformException
{
    public TokenInfo Token { get; init; }

    public AccountNotFoundException(TokenInfo token) : base("No portal account found.") => Token = token;
}