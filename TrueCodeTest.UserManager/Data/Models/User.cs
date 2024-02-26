using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrueCodeTest.UserManager.Data.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid UserId { get; set; }

    [Required] public string Name { get; set; } = default!;

    [Required] public string Domain { get; set; } = default!;

    public List<TagToUser> Tags { get; set; } = default!;
}