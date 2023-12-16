using Ardalis.Specification;
using TrueCodeTest.UserManager.Data.Models;

namespace TrueCodeTest.UserManager.Data.Specifications;

public sealed class GetUsersByTagAndDomainSpec : Specification<User>
{
    public GetUsersByTagAndDomainSpec(string domain, string tag)
    {
        // Should we also check Tag's domain?
        Query.Where(u => u.Domain == domain && u.Tags.Any(ut => ut.Tag.Value == tag));
            
        Query.Include(u => u.Tags).ThenInclude(ut => ut.Tag);
    }
}