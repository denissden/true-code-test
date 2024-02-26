using TrueCodeTest.UserManager.Data.Models;

namespace TrueCodeTest.UserManager.Dto;

public class TagDto
{
    public Guid Id { get; set; }
    public string Domain { get; set; } = default!;
    public string Value { get; set; } = default!;

    public static TagDto FromTag(Tag tag)
    {
        return new TagDto
        {
            Id = tag.TagId,
            Domain = tag.Domain,
            Value = tag.Value
        };
    }
}