const inputMessage = document.getElementById("message-input");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .build();

connection.start();

connection.on("ReceberMensagem", (connectionId, mensagem) => {
    console.log(`${connectionId}: ${mensagem}`);
});

inputMessage.addEventListener("keydown", function (event) {
    if (event.key === "Enter") {

        if (event.shiftKey) return;

        event.preventDefault();
        enviar();
    }
});

async function enviar(){
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