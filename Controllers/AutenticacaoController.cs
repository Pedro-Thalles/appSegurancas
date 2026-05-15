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

public class AutenticacaoController : Controller
{
    private readonly segDbContext _context;
    private readonly IPasswordHasher<Seguranca> _passwordHasher;

    public AutenticacaoController(segDbContext context, IPasswordHasher<Seguranca> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public IActionResult Index()
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
    
                    if (resultado == PasswordVerificationResult.Success)//usuário aprovado, senha correta
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

                        else if (usuario.isApproved == statusAprovacao.EmEdicao)
                        {
                            // Se o usuário estiver em edição, redireciona para a página de cadastro com os dados preenchidos 
                            return RedirectToAction("Edicao", "Cadastro", new {id = usuario.id});
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