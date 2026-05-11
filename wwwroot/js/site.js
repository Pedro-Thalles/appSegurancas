// Aguarda o documento carregar completamente
document.addEventListener("DOMContentLoaded", function () {

    // Pega o botão pelo ID
    const btnSair = document.getElementById("btn-sair");

    // Se o botão existir na tela, adiciona o evento de clique
    if (btnSair) {
        btnSair.addEventListener("click", function (event) {

            // event.preventDefault() é o segredo! Ele impede que o link mude de página na hora.
            event.preventDefault();

            // Guarda a URL real para onde o link ia (/Login/Logout)
            const urlDestino = this.getAttribute("href");

            // Chama o pop-up bonitão do SweetAlert2
            Swal.fire({
                title: 'Tem certeza?',
                text: "Você será desconectado do sistema.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33', // Cor vermelha para o botão de sair
                cancelButtonColor: '#6c757d', // Cor cinza para o cancelar
                confirmButtonText: 'Sim, quero sair',
                cancelButtonText: 'Cancelar'
            }).then((resultado) => {
                // Se o usuário clicou no botão "Sim, quero sair"
                if (resultado.isConfirmed) {
                    // Agora sim, redireciona para a Action do Controller
                    window.location.href = urlDestino;
                }
            });
        });
    }

    document.querySelectorAll('.btn-confirmar').forEach(botao => {
        botao.addEventListener('click', function (e) {
            const botaoClicado = e.target;
            const acao = botaoClicado.innerText; // "Aprovar" ou "Rejeitar"

            Swal.fire({
                title: `Confirmar ${acao}?`,
                text: `Você tem certeza que deseja ${acao.toLowerCase()} este cadastro?`,
                icon: acao === 'Aprovar' ? 'question' : 'warning',
                showCancelButton: true,
                confirmButtonColor: acao === 'Aprovar' ? '#198754' : '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sim, confirmar!',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Configura o formulário para ir para a Action do botão clicado
                    const form = botaoClicado.closest('form');
                    form.action = botaoClicado.getAttribute('formaction'); // Pega o link do asp-action
                    form.submit(); // Envia o formulário manualmente
                }
            });
        });
    });


});


