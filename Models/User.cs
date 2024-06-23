using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthStream.API.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAccountConfirmed { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
    public byte[] PasswordHash { get; set; } = new byte[0];
    public byte[] PasswordSalt { get; set;} = new byte[0];

    #pragma warning disable CS8618
    public User()
    {
    }
    #pragma warning restore CS8618
}