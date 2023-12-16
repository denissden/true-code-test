using Ardalis.Specification;
using TrueCodeTest.UserManager.Data.Models;

namespace TrueCodeTest.UserManager.Data.Specifications;

public sealed class GetUsersByDomainPaginatedSpec : Specification<User>
{
    public GetUsersByDomainPaginatedSpec(string domain, int pageNumber, int pageSize)
    {
        Query.Where(u => u.Domain == domain);

        Query.Include(u => u.Tags).ThenInclude(ut => ut.Tag);

        Query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }
}