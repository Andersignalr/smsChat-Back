const API_URL = "https://localhost:7281/auth";

async function registrar() {
    const nome = document.getElementById("nome").value;
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;
    const msg = document.getElementById("msg");

    try {
        const response = await fetch(`${API_URL}/register`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include", // 🔥 ESSENCIAL para Identity
            body: JSON.stringify({ nome, email, password })
        });

        if (!response.ok) {
            msg.innerText = "Erro ao registrar";
            return;
        }

        msg.innerText = "Registrado com sucesso!";
        window.location.href = "chat.html";

    } catch (err) {
        console.error(err);
        msg.innerText = "Erro de conexão";
    }
}

async function login() {
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;
    const msg = document.getElementById("msg");

    try {
        const response = await fetch(`${API_URL}/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include", // 🔥 ESSENCIAL
            body: JSON.stringify({ email, password })
        });

        if (!response.ok) {
            msg.innerText = "Email ou senha inválidos";
            return;
        }

        window.location.href = "chat.html";

    } catch (err) {
        console.error(err);
        msg.innerText = "Erro de conexão";
    }
}
