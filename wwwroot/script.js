const inputMessage = document.getElementById("message-input");

const contactsArea = document.getElementById("contacts-area");

let usuarioSelecionadoId = null;

let meuUserId = null;

document.querySelectorAll(".contact-area")
    .forEach(contato => {

        contato.addEventListener("click", function () {
            selecionarContato(this);
        });

    });

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .build();

connection.start();


connection.on("ReceberMensagem", (dados) => {
    console.log(`${dados.usuario}: ${dados.mensagem}`);
});


connection.on("UsuariosOnline", (usuarios) => {

    console.log("Online:", usuarios);

    const status = document.querySelector(".head-contacts-area .status");
    status.innerText = usuarios.length + " online";

    const lista = document.querySelector(".contacts-area");
    lista.innerHTML = "";

    usuarios.forEach(user => {

        const div = document.createElement("div");
        div.className = "contact-area";

        // 🔥 Guardando informação escondida
        div.dataset.userid = user.userId;
        div.dataset.nome = user.nome;

        div.innerHTML = `
            <div class="contact-info">
                <div class="name">${user.nome}</div>
                <div class="status" style="color: green;">online</div>
            </div>
        `;

        // 🔥 Evento de clique
        div.addEventListener("click", function () {

            usuarioSelecionadoId = this.dataset.userid;

            //limpa lista de mensagens
            document.querySelector(".right-center").innerHTML = "";

            // Atualiza o topo da conversa
            document.querySelector(".right-head .name").innerText =
                this.dataset.nome;

            document.querySelector(".right-head .status").innerText =
                "online";

            // Remove seleção anterior
            document.querySelectorAll(".contact-area")
                .forEach(c => c.classList.remove("selected"));

            // Marca como selecionado
            this.classList.add("selected");

            const badge = this.querySelector(".badge");
            if (badge) badge.remove();

            connection.invoke("CarregarMensagens", usuarioSelecionadoId);

        });

        lista.appendChild(div);
    });
});

connection.on("MeuUserId", (id) => {
    meuUserId = id;
    console.log("Meu ID:", meuUserId);
});



connection.on("ReceberMensagemPrivada", function (dados) {

    const conversaAberta =
        (
            dados.deUserId === usuarioSelecionadoId &&
            dados.paraUserId === meuUserId
        ) ||
        (
            dados.deUserId === meuUserId &&
            dados.paraUserId === usuarioSelecionadoId
        );

    const outroUserId =
        dados.deUserId === meuUserId
            ? dados.paraUserId
            : dados.deUserId;


    moverParaTopo(outroUserId);

    if (!conversaAberta) {
        marcarBadge(dados.deUserId === meuUserId
            ? dados.paraUserId
            : dados.deUserId);
        return;
    }

    const chatArea = document.querySelector(".right-center");

    const isMe = dados.deUserId === meuUserId;

    const container = document.createElement("div");
    container.classList.add("message-container");
    container.classList.add(isMe ? "me" : "they");

    const message = document.createElement("div");
    message.classList.add("message");
    message.classList.add(isMe ? "me" : "they");
    message.innerText = dados.mensagem;

    container.appendChild(message);
    chatArea.appendChild(container);

    chatArea.scrollTop = chatArea.scrollHeight;
});

connection.on("MensagensCarregadas", function (mensagens) {

    const chatArea = document.querySelector(".right-center");
    chatArea.innerHTML = "";

    mensagens.forEach(m => {

        const isMe = m.deUserId === meuUserId;

        const container = document.createElement("div");
        container.classList.add("message-container");
        container.classList.add(isMe ? "me" : "they");

        const message = document.createElement("div");
        message.classList.add("message");
        message.classList.add(isMe ? "me" : "they");
        message.innerText = m.texto;

        container.appendChild(message);
        chatArea.appendChild(container);
    });

    chatArea.scrollTop = chatArea.scrollHeight;
});




inputMessage.addEventListener("keydown", function (event) {
    if (event.key === "Enter") {

        if (event.shiftKey) return;

        event.preventDefault();
        enviarPrivada();
    }
});

async function enviar() {
    const inputMessage = document.getElementById("message-input");

    const mensagem = inputMessage.value.trim();

    if (!mensagem) return;

    if (connection.state !== signalR.HubConnectionState.Connected) {
        console.warn("Ainda não conectado ao SignalR");

        return;
    }

    try {
        await connection.invoke("EnviarMensagem", mensagem);
        inputMessage.value = "";
    } catch (err) {
        console.error(err);
    }

}

function enviarPrivada() {
    const mensagem = document.getElementById("message-input").value;

    if (!usuarioSelecionadoId) {
        alert("Selecione um contato primeiro");
        return;
    }

    connection.invoke("EnviarMensagemPrivada",
        usuarioSelecionadoId,
        mensagem
    );

    document.getElementById("message-input").value = "";
}

function marcarBadge(userId) {

    const contato = document.querySelector(
        `.contact-area[data-userid="${userId}"]`
    );

    if (!contato) return;

    // evita criar vários badges
    if (contato.querySelector(".badge")) return;

    const badge = document.createElement("span");
    badge.classList.add("badge");
    badge.innerText = "●";

    contato.appendChild(badge);
}


function moverParaTopo(userId) {

    const lista = document.querySelector(".contacts-area");

    const contato = document.querySelector(
        `.contact-area[data-userid="${userId}"]`
    );

    if (!contato) return;

    // Move para o topo
    lista.prepend(contato);
}
