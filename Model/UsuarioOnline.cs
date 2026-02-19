public class UsuarioOnline
{
    public string Nome { get; set; } = null!;
    public string? Foto { get; set; }
    public HashSet<string> Conexoes { get; set; } = new();
}
