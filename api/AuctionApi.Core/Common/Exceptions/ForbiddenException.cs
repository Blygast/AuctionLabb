namespace AuctionApi.Core.Common.Exceptions;

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message) { }
}
