using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;


[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _context;

    public static Dictionary<string, (string Nome, HashSet<string> Conexoes)> OnlineUsers = new();

    public ChatHub(AppDbContext context)
    {
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier!;
        var nome = Context.User?.Claims
            .FirstOrDefault(c => c.Type == "Nome")?.Value
            ?? Context.User?.Identity?.Name
            ?? "Usuário";

        if (!UserTracker.OnlineUsers.ContainsKey(userId))
        {
            UserTracker.OnlineUsers[userId] = new UsuarioOnline
            {
                Nome = nome
            };
        }

        UserTracker.OnlineUsers[userId]
            .Conexoes
            .Add(Context.ConnectionId);

        await Clients.All.SendAsync("UsuariosOnline",
            UserTracker.OnlineUsers.Select(u => new
            {
                UserId = u.Key,
                Nome = u.Value.Nome,
                Foto = _context.Users
                    .Where(x => x.Id == u.Key)
                    .Select(x => x.FotoPerfil)
                    .FirstOrDefault()
            }));

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier!;

        if (UserTracker.OnlineUsers.ContainsKey(userId))
        {
            UserTracker.OnlineUsers[userId]
                .Conexoes
                .Remove(Context.ConnectionId);

            if (UserTracker.OnlineUsers[userId].Conexoes.Count == 0)
            {
                UserTracker.OnlineUsers.Remove(userId);
            }
        }

        await Clients.All.SendAsync("UsuariosOnline",
            UserTracker.OnlineUsers.Select(u => new
            {
                UserId = u.Key,
                Nome = u.Value.Nome,
                Foto = _context.Users
                    .Where(x => x.Id == u.Key)
                    .Select(x => x.FotoPerfil)
                    .FirstOrDefault()
            }));



        await base.OnDisconnectedAsync(exception);
    }



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

    public async Task EnviarMensagemPrivada(string userIdDestino, string mensagem)
    {
        var userIdOrigem = Context.UserIdentifier!;
        var nomeOrigem = Context.User?.Claims
            .FirstOrDefault(c => c.Type == "Nome")?.Value
            ?? "Usuário";

        var novaMensagem = new Mensagem
        {
            DeUserId = userIdOrigem,
            ParaUserId = userIdDestino,
            Texto = mensagem,
            DataEnvio = DateTime.UtcNow,
            Lida = false
        };

        _context.Mensagens.Add(novaMensagem);
        await _context.SaveChangesAsync();

        var payload = new
        {
            DeUserId = userIdOrigem,
            ParaUserId = userIdDestino,
            DeNome = nomeOrigem,
            Mensagem = mensagem
        };


        if (userIdDestino == userIdOrigem)
        {
            // 🔥 envia só uma vez
            await Clients.User(userIdOrigem)
                .SendAsync("ReceberMensagemPrivada", payload);
        }
        else
        {
            await Clients.User(userIdDestino)
                .SendAsync("ReceberMensagemPrivada", payload);

            await Clients.User(userIdOrigem)
                .SendAsync("ReceberMensagemPrivada", payload);
        }
    }

    public async Task CarregarMensagens(string outroUserId)
    {
        var meuUserId = Context.UserIdentifier!;

        var mensagens = await _context.Mensagens
            .Where(m =>
                (m.DeUserId == meuUserId && m.ParaUserId == outroUserId) ||
                (m.DeUserId == outroUserId && m.ParaUserId == meuUserId)
            )
            .OrderBy(m => m.DataEnvio)
            .Select(m => new
            {
                m.Id,
                m.DeUserId,
                m.ParaUserId,
                m.Texto,
                m.DataEnvio
            })
            .ToListAsync();

        await Clients.Caller.SendAsync("MensagensCarregadas", mensagens);
    }

    public async Task ObterMeusDados()
    {
        var userId = Context.UserIdentifier!;

        var user = await _context.Users
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.Id,
                x.Nome,
                x.FotoPerfil
            })
            .FirstOrDefaultAsync();

        if (user == null) return;

        await Clients.Caller.SendAsync("MeusDados", new
        {
            userId = user.Id,
            nome = user.Nome,
            foto = user.FotoPerfil
        });
    }


}
