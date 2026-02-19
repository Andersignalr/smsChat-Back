using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string Nome { get; set; } = string.Empty;
    public string? FotoPerfil { get; set; }

}
