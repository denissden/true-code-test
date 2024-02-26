using TrueCodeTest.UserManager.Data.Models;

namespace TrueCodeTest.UserManager.Dto;

public class UserDto
{
    public Guid Id { get; set; }
    public string Domain { get; set; } = default!;
    public List<TagDto> Tags { get; set; } = new();

    public static UserDto FromUser(User user)
    {
        return new UserDto
        {
            Id = user.UserId,
            Domain = user.Domain,
            Tags = user.Tags.Select(t => TagDto.FromTag(t.Tag)).ToList()
        };
    }
}