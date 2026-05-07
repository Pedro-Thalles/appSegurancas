using Microsoft.AspNetCore.Mvc;
using appSegurancas.Data;
using appSegurancas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace appSegurancas.Controllers;

public class SegurancaController : Controller
{
    private readonly segDbContext _context;
    private readonly IPasswordHasher<Seguranca> _passwordHasher;


    public SegurancaController(segDbContext context, IPasswordHasher<Seguranca> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public IActionResult Index()
    {
        return View();
    }

    // GET: /Seguranca/Cadastro
    // Abre a página com o formulário vazio
    public IActionResult Cadastro()
    {
        return View();
    }


    // POST: /Seguranca/Cadastro
    [HttpPost]
    public async Task<IActionResult> Cadastro(Seguranca seguranca, string senhaPura)
    {
        ModelState.Remove("Contracheques");
        ModelState.Remove("role");
        ModelState.Remove("passwordHash");

        if (ModelState.IsValid)
        {
            
            seguranca.passwordHash = _passwordHasher.HashPassword(seguranca, senhaPura);

            _context.Segurancas.Add(seguranca);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login"); // Manda pra home após salvar
        }
        return View(seguranca);
    }

    

    // GET: /Seguranca/MeusContracheques/5
    [Authorize]
    public IActionResult MeusContracheques()
    {
        Console.WriteLine($"Usuario logado: {User.Identity.IsAuthenticated}");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        int idLogado = int.Parse(userId.Value);

        var usuario = _context.Segurancas
            .Include(s => s.Contracheques) 
            .FirstOrDefault(s => s.id == idLogado);

        if (usuario == null) return NotFound();

        usuario.Contracheques = usuario.Contracheques.OrderBy(c => c.mesUpload).ToList();
     
        return View(usuario.Contracheques);
    }

    // GET: /Seguranca/DownloadContracheque/5
    public IActionResult DownloadContracheque(int id)
    {
        // 1. Busca o registro do contracheque no banco
        var contracheque = _context.Contracheques.FirstOrDefault(c => c.id == id);

        if (contracheque == null || string.IsNullOrEmpty(contracheque.filePath))
        {
            return NotFound("Arquivo não encontrado no banco de dados.");
        }

        // 2. Monta o caminho físico completo no servidor
        // O Path.Combine garante que funcione em qualquer sistema (Windows/Linux)
        var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contracheque.filePath.TrimStart('/'));

        // 3. Verifica se o arquivo físico REALMENTE existe na pasta
        if (!System.IO.File.Exists(caminhoArquivo))
        {
            return NotFound("O arquivo físico não foi encontrado na pasta de uploads.");
        }

        // 4. Lê os bytes do arquivo
        var bytes = System.IO.File.ReadAllBytes(caminhoArquivo);

        // 5. Define o nome que o arquivo terá quando o usuário baixar
        string nomeParaDownload = $"Contracheque_{contracheque.mesUpload}_{contracheque.anoUpload}.pdf";

        // 6. Retorna o arquivo para o navegador
        return File(bytes, "application/pdf", nomeParaDownload);
    }

    //####################### LOGIN E LOGOUT #######################

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {

            if (model.cpf == "12345678912")
            {
                ModelState.AddModelError(string.Empty, "Impossível logar como administrador. Acesse a página Admin.");
                return View(model);
            }


            var usuario = _context.Segurancas.FirstOrDefault(s => s.cpf == model.cpf);

            if (usuario != null)
            {
                try
                {
                    var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.passwordHash, model.senha);

                    if (resultado == PasswordVerificationResult.Success)
                    {
                        if (usuario.isApproved == statusAprovacao.Pendente)
                        {
                            ModelState.AddModelError(string.Empty, "Seu cadastro ainda não foi aprovado. Por favor, aguarde ou contate o administrador");
                            return View(model);
                        }
                        else if (usuario.isApproved == statusAprovacao.Rejeitado)
                        {
                            ModelState.AddModelError(string.Empty, "Infelizmente seu cadastro foi rejeitado. Por favor, contate o administrador para mais informações.");
                            return View(model);
                        }

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, usuario.nome),
                            new Claim(ClaimTypes.NameIdentifier, usuario.id.ToString()),
                            new Claim(ClaimTypes.Role, "Seguranca"),
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties { IsPersistent = true };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));


                        //esse é o objetivo, se chegar aqui, mata o login
                        //return RedirectToAction("MeusContracheques", new { id = usuario.id });

                        return RedirectToAction("Index", "Home");

                    }
                    ModelState.AddModelError(string.Empty, "CPF ou senha inválidos.");
                    ViewBag.Erro = "Senha incorreta.";
                    return View();

                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Você precisa mudar sua senha");
                    return View(model);
                }

            }

            ModelState.AddModelError(string.Empty, "CPF ou senha inválidos.");

        }

        return View(model);

    }


    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}