using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrueCodeTest.UserManager.Data.Models;

public class TagToUser 
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid EntityId { get; set;}
    public Guid UserId { get; set; }
    public Guid TagId { get; set; }

    public User User { get; set; } = default!;
    public Tag Tag { get; set; } = default!;
}
