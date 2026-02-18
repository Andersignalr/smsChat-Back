using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task EnviarMensagem(string mensagem)
    {
        await Clients.All.SendAsync("ReceberMensagem",
            Context.ConnectionId,
            mensagem);
    }
}

