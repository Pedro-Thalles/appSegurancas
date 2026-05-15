using Microsoft.AspNetCore.Mvc;
using appSegurancas.Data;
using appSegurancas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace appSegurancas.Controllers;

public class CadastroController : Controller
{
    private readonly segDbContext _context;
    private readonly IPasswordHasher<Seguranca> _passwordHasher;

    public CadastroController(segDbContext context, IPasswordHasher<Seguranca> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }
    //GET
    //site/Cadastro
    public IActionResult Index()
    {

        var seguranca = new Seguranca();
        return View(seguranca);
    }
     public IActionResult Edicao(int id)
    {
        var seguranca = _context.Segurancas.FirstOrDefault(s => s.id == id);
        if (seguranca == null){ return NotFound(); }
        return View(seguranca);
    }
    //POST de cadastro
    [HttpPost]
    public async Task<IActionResult> Index(Seguranca seguranca, string senhaPura, string senhaConfirm)
    {
        ModelState.Remove("Contracheques");
        ModelState.Remove("role");
        ModelState.Remove("passwordHash");

        if (ModelState.IsValid)
        {
            var segurancaExistente = _context.Segurancas.FirstOrDefault(s => s.cpf == seguranca.cpf);

            if(senhaPura != senhaConfirm)
            {
                
                ModelState.AddModelError(string.Empty, "As senhas não coincidem.");
                if(segurancaExistente != null && segurancaExistente.isApproved == statusAprovacao.EmEdicao)
                {   
                    Console.WriteLine("\n\n\n\n\n ENTREI AQUI \n\n\n\n\n");
                    return View(segurancaExistente);
                }
                return View(seguranca);
            }
            //consulta se já existe um cadastro com aquele cpf.
            
            //Se já existir, e estiver em edição, atualiza os dados e a senha, e manda pra login.
            if (segurancaExistente != null && segurancaExistente.isApproved == statusAprovacao.EmEdicao) { 
            
                segurancaExistente.nome = seguranca.nome;
                segurancaExistente.sobreNome = seguranca.sobreNome;
                segurancaExistente.email = seguranca.email;
                segurancaExistente.matricula = seguranca.matricula;
                segurancaExistente.passwordHash = _passwordHasher.HashPassword(segurancaExistente, senhaPura);
                segurancaExistente.isApproved = statusAprovacao.Pendente;
                segurancaExistente.updatedAt = DateTime.Now;
                _context.Segurancas.Update(segurancaExistente);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login", "Seguranca");

            }
            //Se já existir, e não estiver em edição, mostra mensagem de erro.
            if (segurancaExistente != null && segurancaExistente.isApproved != statusAprovacao.EmEdicao)
            {

                ModelState.AddModelError(string.Empty, "Já existe uma conta para esse CPF");    
                return View(seguranca);
            }

            //Se não existir, cria um novo cadastro normalmente.

            seguranca.passwordHash = _passwordHasher.HashPassword(seguranca, senhaPura);
            _context.Segurancas.Add(seguranca);
            await _context.SaveChangesAsync();
            //manda pro controller SegurancaControler, ação login
            return RedirectToAction("Login", "Seguranca");
        }
        return View(seguranca);
    }

}

