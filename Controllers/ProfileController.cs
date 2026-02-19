using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[Route("profile")]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _environment = environment;
    }

    [Authorize]
    [HttpGet("minhafoto")]
    public async Task<IActionResult> MinhaFoto()
    {
        var user = await _userManager.GetUserAsync(User);
        return Ok(user?.FotoPerfil ?? "");
    }


    [HttpPost("UploadFoto")]
    public async Task<IActionResult> UploadFoto(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest();

        var user = await _userManager.GetUserAsync(User);

        var uploads = Path.Combine(
            _environment.WebRootPath,
            "uploads/profile"
        );

        Directory.CreateDirectory(uploads);

        var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploads, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        user.FotoPerfil = $"/uploads/profile/{fileName}";
        await _userManager.UpdateAsync(user);

        return Ok(user.FotoPerfil);
    }
}
