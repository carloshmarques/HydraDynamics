/* main.js - Lógica de Performance HydraLife + HydraEvents */

// 1. Deteção Automática On Load
window.addEventListener('DOMContentLoaded', () => {
    const htmlElement = document.documentElement;
    const actionBtn = document.querySelector('.button');

    // Se detetar menos de 4GB de RAM, ativa o modo performance logo ao início
    if (navigator.deviceMemory && navigator.deviceMemory < 4) {
        htmlElement.classList.add('low-memory');
        if (actionBtn) actionBtn.innerText = "Modo Performance: ON";
    }

    // 2. Alternância Manual (O Teu Botão "Iniciar C#")
    if (actionBtn) {
        actionBtn.addEventListener('click', () => {
            htmlElement.classList.toggle('low-memory');

            const badgeText = document.querySelector('#performance-badge .status-text');
            if (htmlElement.classList.contains('low-memory')) {
                badgeText.innerText = "Modo Económico";
                actionBtn.innerText = "Efeitos: OFF (Performance)";
                console.log("HydraLife: Modo de baixo consumo ativado.");

                // Envia evento para o C#
                HydraEvents.send("performance-low");
            } else {
                badgeText.innerText = "Sistema Optimizado";
                actionBtn.innerText = "Efeitos: ON (Aero Glass)";
                console.log("HydraLife: Efeitos visuais ativados.");

                // Envia evento para o C#
                HydraEvents.send("performance-high");
            }
        });
    }

    // 3. Recebe eventos do C# (HydraEvents)
    window.HydraEvents = {
        receive(evt) {
            console.log("C# → JS event:", evt);

            // C# envia hostmode real
            if (evt.name === "hostmode") {
                htmlElement.classList.add(evt.payload.mode);
                console.log("HydraLife: HostMode aplicado →", evt.payload.mode);
            }

            // C# envia comando para ativar performance
            if (evt.name === "force-low") {
                htmlElement.classList.add("low-memory");
            }

            // C# envia comando para ativar efeitos
            if (evt.name === "force-high") {
                htmlElement.classList.remove("low-memory");
            }
        },

        send(name, payload = {}) {
            window.chrome.webview.postMessage(JSON.stringify({
                name,
                payload
            }));
        }
    };

    // 4. Assim que o site carrega, pede ao C# o hostmode real
    HydraEvents.send("request-hostmode");
    window.HydraTheme = {
    apply(theme) {
        const html = document.documentElement;

        html.classList.remove("light", "dark");
        html.classList.add(theme);

        console.log("HydraThemeSync: tema aplicado →", theme);

        // Opcional: atualizar UI
        const badge = document.querySelector("#theme-badge");
        if (badge) badge.innerText = theme === "dark" ? "Modo Escuro" : "Modo Claro";
    }
};

});
