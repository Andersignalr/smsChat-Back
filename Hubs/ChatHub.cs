using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize]
public class ChatHub : Hub
{
    public async Task EnviarMensagem(string mensagem)
    {
        var nome = Context.User?.Claims
            .FirstOrDefault(c => c.Type == "Nome")?.Value
            ?? Context.User?.Identity?.Name
            ?? "Usuário";

        await Clients.All.SendAsync("ReceberMensagem", new
        {
            Usuario = nome,
            Mensagem = mensagem
        });
    }
}
