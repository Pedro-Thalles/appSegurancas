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

public class AdminController : Controller {

    private readonly segDbContext _context;
    private readonly IPasswordHasher<Seguranca> _passwordHasher;

    public AdminController(segDbContext context, IPasswordHasher<Seguranca> passwordHasher) {
        _context = context;
        _passwordHasher = passwordHasher;
        
    }

    public IActionResult Index()
    {
        // Se já estiver logado como admin, vai direto pro painel
        if (User.Identity.IsAuthenticated && User.HasClaim("Perfil", "Admin"))
        {
            return RedirectToAction("PainelAdmin");
        }
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Admin(string login, string senha)
    {
        // Busca o admin único no banco
        var admin = _context.Segurancas.FirstOrDefault(s => s.IsAdmin == true && s.cpf == login);

        if (admin != null)
        {

            var resultado = _passwordHasher.VerifyHashedPassword(admin, admin.passwordHash, senha);

            if (resultado == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Administrador"),
                new Claim(ClaimTypes.NameIdentifier, admin.id.ToString()),
                new Claim("Perfil", "Admin") // Claim crucial para diferenciar
            };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("PainelAdmin");
            }
        }

        ViewBag.Erro = "Acesso administrativo negado.";
        ModelState.AddModelError(string.Empty, "Acesso administrativo negado");
        return View();
    }


    public IActionResult PainelAdmin()
    {

        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }

        return View();
    }


    // GET: /Seguranca/PainelAdmin
    [Authorize]
    public IActionResult AprovarSeguranca()
    {
        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }

        var pendentes = _context.Segurancas
            .Where(s => s.isApproved == statusAprovacao.Pendente)
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
            usuario.isApproved = statusAprovacao.Aprovado;
            _context.Update(usuario);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("AprovarSeguranca");
    }

    [HttpPost]
    public async Task<IActionResult> Rejeitar(int id)
    {
        var usuario = await _context.Segurancas.FindAsync(id);

        if (usuario != null)
        {
            usuario.isApproved = statusAprovacao.Rejeitado;
            _context.Update(usuario);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("AprovarSeguranca");
    }

    [Authorize]
    public IActionResult VisualizarSegurancas()
    {
        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }
        var segurancas = _context.Segurancas.Where(s => s.isApproved == statusAprovacao.Aprovado).ToList();
        return View(segurancas);
    }

    /*[Authorize]
    public IActionResult VerContrachequeSeguranca(int id) { 
    
        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }
        var contracheques = _context.Contracheques
            .Include(c => c.seguranca)
            .Where(c => c.segurancaId == id)
            .OrderBy(c => c.mesUpload)
            .ToList();

        if (contracheques == null || !contracheques.Any()) return View(); //NotFound();

        return View(contracheques);

    }*/

    [Authorize]
    public IActionResult VerContrachequeSeguranca(int id)
    {
        if (!User.HasClaim("Perfil", "Admin")) {

            return Forbid();

        }
        
        var seguranca = _context.Segurancas.FirstOrDefault(s => s.id == id);

        if(seguranca == null) return NotFound();

        var listaContracheques = _context.Contracheques
            .Include(c => c.seguranca)
            .Where(c => c.segurancaId == id)
            .OrderBy(c => c.mesUpload)
            .ToList();

        var viewModel = new DetalhesContrachequeViewModel
        {

            segurancaId = id,
            nomeSeguranca = seguranca.nome,
            contracheques = listaContracheques

        };

        return View(viewModel);


    }


    // GET: /Seguranca/UploadContracheque/5
    [Authorize]
    public IActionResult UploadContracheque(int id)
    {
        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }
        var seguranca = _context.Segurancas.Find(id);
        if (seguranca == null) return NotFound();

        var model = new ContrachequeViewModel
        {
            segurancaId = id,
            nomeSeguranca = seguranca.nome
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> UploadContracheque(ContrachequeViewModel model)
    {
        ModelState.Remove("nomeSeguranca"); // Remove a validação do nome, pois ele não é enviado no formulário
        if (ModelState.IsValid)
        {

            var contrachequeExistente = _context.Contracheques.FirstOrDefault(c =>
            c.mesUpload == model.mesUpload && c.anoUpload == model.anoUpload && c.segurancaId == model.segurancaId);

            int segurancaId = model.segurancaId;

            if (contrachequeExistente != null)
            {
                ModelState.AddModelError(string.Empty, "Já existe um contracheque para esse mês e ano. Por favor, escolha outro período.");
                return View(model);
            }

            if (model.arquivoPdf != null && model.arquivoPdf.Length > 0)
            {
                // Definir o nome do arquivo
                string nomeArquivo = $"Contracheque_{model.segurancaId}_{model.mesUpload}_{model.anoUpload}.pdf";

                // Caminho onde será salvo 
                string caminhoPasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracheques");
                string caminhoCompleto = Path.Combine(caminhoPasta, nomeArquivo);

                // Salvar o arquivo físico no HD
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await model.arquivoPdf.CopyToAsync(stream);
                }

                // Salvar as informações no Banco de Dados
                var novoContracheque = new Contracheque
                {
                    mesUpload = model.mesUpload,
                    anoUpload = model.anoUpload,
                    segurancaId = model.segurancaId,

                    filePath = $"/uploads/contracheques/{nomeArquivo}"
                };

                _context.Contracheques.Add(novoContracheque);
                await _context.SaveChangesAsync();
                Console.WriteLine("TESTE");
                return RedirectToAction("VerContrachequeSeguranca", new { id = segurancaId });
            }
        }

        Console.WriteLine("ModelState inválido ou arquivo não selecionado.");

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ExcluirContracheque(int id)
    {
        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }

        var contracheque = await _context.Contracheques.FindAsync(id);

        if (contracheque == null)
        {
            ModelState.AddModelError(string.Empty, "Esse contracheque não Existe mais");
            return NotFound();
        }

        int segurancaId = contracheque.segurancaId;

        if (!string.IsNullOrEmpty(contracheque.filePath)) {
               
            var caminhoCompleto = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", "uploads", "pdf", contracheque.filePath);

            if(System.IO.File.Exists(caminhoCompleto)){

                System.IO.File.Delete(caminhoCompleto);

            }

        }

        _context.Contracheques.Remove(contracheque);
        await _context.SaveChangesAsync();



        return RedirectToAction("VerContrachequeSeguranca", new { id = segurancaId });

    }

}