using Ardalis.Specification;
using TrueCodeTest.UserManager.Data.Models;

namespace TrueCodeTest.UserManager.Data.Specifications;

public sealed class GetUserByIdAndDomainSpec : SingleResultSpecification<User>
{
    public GetUserByIdAndDomainSpec(Guid userId, string domain)
    {
        Query.Where(u => u.UserId == userId && u.Domain == domain);

        Query.Include(u => u.Tags).ThenInclude(ut => ut.Tag);
    }
}