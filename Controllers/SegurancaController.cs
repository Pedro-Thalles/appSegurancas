using Microsoft.AspNetCore.Mvc;
using appSegurancas.Data;
using appSegurancas.Models;
using Microsoft.EntityFrameworkCore;

namespace appSegurancas.Controllers;

public class SegurancaController : Controller
{
    private readonly segDbContext _context;

   
    public SegurancaController(segDbContext context)
    {
        _context = context;
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
            
            seguranca.passwordHash = "HASH_" + senhaPura;

            _context.Segurancas.Add(seguranca);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home"); // Manda pra home após salvar
        }
        return View(seguranca);
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var usuario = _context.Segurancas.FirstOrDefault(s => s.cpf == model.cpf);

            if (usuario != null)
            {
                string hashDigitado = "HASH_" + model.senha;

                if (usuario.passwordHash == hashDigitado)
                {
                    if (!usuario.isApproved)
                    {
                        ModelState.AddModelError(string.Empty, "Usuário não aprovado. Contate o administrador.");
                        return View(model);
                    }

                    return RedirectToAction("Index", "Home");
                }

            }

            ModelState.AddModelError(string.Empty, "CPF ou senha inválidos.");

        }

        return View(model);

    }


    

    // GET: /Seguranca/PainelAdmin
    public IActionResult PainelAdmin()
    {
       
        var pendentes = _context.Segurancas
            .Where(s => s.isApproved == false)
            .ToList();

        return View(pendentes);
    }

    // POST: /Seguranca/Aprovar/5
    [HttpPost]
    public async Task<IActionResult> Aprovar(int id)
    {
        var usuario = await _context.Segurancas.FindAsync(id);

        if (usuario != null)
        {
            usuario.isApproved = true;
            _context.Update(usuario);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("PainelAdmin");
    }




    // GET: /Seguranca/MeusContracheques/5
    public IActionResult MeusContracheques(int id)
    {
      
        var usuario = _context.Segurancas
            .Include(s => s.Contracheques) 
            .FirstOrDefault(s => s.id == id);

        if (usuario == null) return NotFound();

        return View(usuario.Contracheques);
    }

    public async Task<IActionResult> GerarDadosTeste(int id)
    {
        var novoContracheque = new Contracheque
        {
            // Removi o id fixo para o banco gerar automático
            MesUpload = 4,
            AnoUpload = 2026,
            segurancaId = id // <--- Isso aqui é o que "cola" o dado no segurança certo!
        };

        _context.Contracheques.Add(novoContracheque);
        await _context.SaveChangesAsync();

        return Content($"Gerado para o Segurança {id}!");
    }


}