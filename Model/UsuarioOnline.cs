public class UsuarioOnline
{
    public string Nome { get; set; } = string.Empty;
    public HashSet<string> Conexoes { get; set; } = new();
}
