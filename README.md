## appSegurancas

Sistema web desenvolvido para o gerenciamento de agentes de segurança, controle de acesso a funcionalidades e fluxo de aprovação de cadastros.

## Funcionalidades Principais

* **Autenticação e Autorização:** Controle de acesso baseado em *Claims* e *Roles*, garantindo a separação de privilégios entre Administrador e Seguranças.
* **Fluxo de Aprovação:** Gerenciamento de status de cadastro (Pendente, Aprovado, Rejeitado) utilizando *Enum*.
* **Painel Administrativo:** Área exclusiva para o Administrador validar novos cadastros, acessível apenas via credenciais especiais.
* **Seed de Inicialização:** Criação automática do Administrador na primeira execução do sistema (login padrão via código).
* **Interface Inteligente:** Navegação adaptativa que direciona o usuário conforme o seu perfil logado.

---

## Pré-requisitos

* [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download)
* **SQL Server** ou **LocalDB** (Instalado via menu customizado do SQL Server / Visual Studio)

---

## Tecnologias Utilizadas

* **Backend:** C#, ASP.NET Core MVC
* **Banco de Dados:** Entity Framework Core, SQL Server / LocalDB
* **Frontend:** Razor Views, HTML5, CSS3, Bootstrap 5

---

## Como Rodar o Projeto

### 1. Clonar o Repositório
Abra o terminal e clone o projeto em sua máquina:

```
git clone https://github.com/Pedro-Thalles/appSegurancas.git
cd appSegurancas

```

### 2. Restaurar Dependências

```
dotnet restore

```
### 3. Configurar Banco de Dados

```
dotnet ef database update

```

### 4. Executar app

```
dotnet watch

```

### 5. Acesso ao sistema

O Administrador é criado automaticamente na primeira inicialização com o CPF 12345678912 e senha senhaadmin.
Mude isso na no program.cs

URL de Login do Admin: https://localhost:porta/Seguranca/Admin

URL de Login dos Seguranças: https://localhost:porta/Seguranca/Login
