using Microsoft.AspNetCore.Mvc;
using appSegurancas.Data;
using appSegurancas.Models;

namespace appSegurancas.Controllers;

public class SegurancaController : Controller
{
    private readonly segDbContext _context;

    // O ASP.NET injeta o Banco de Dados aqui automaticamente
    public SegurancaController(segDbContext context)
    {
        _context = context;
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
            // Por enquanto vamos apenas simular o hash da senha
            seguranca.passwordHash = "HASH_" + senhaPura;

            _context.Segurancas.Add(seguranca);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home"); // Manda pra home após salvar
        }
        return View(seguranca);
    }
}