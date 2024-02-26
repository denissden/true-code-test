using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrueCodeTest.UserManager.Data.Models;

public class Tag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid TagId { get; set; }

    [Required] public string Value { get; set; } = default!;

    [Required] public string Domain { get; set; } = default!;


    public List<User> Users { get; set; } = default!;
}