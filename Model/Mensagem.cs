public class Mensagem
{
    public int Id { get; set; }

    public string DeUserId { get; set; } = null!;
    public string ParaUserId { get; set; } = null!;

    public string Texto { get; set; } = null!;

    public DateTime DataEnvio { get; set; }

    public bool Lida { get; set; } = false;

    // 🔥 Navegação (opcional mas profissional)
    public ApplicationUser DeUser { get; set; } = null!;
    public ApplicationUser ParaUser { get; set; } = null!;
}
