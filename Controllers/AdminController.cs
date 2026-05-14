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
        // Se j� estiver logado como admin, vai direto pro painel
        if (User.Identity.IsAuthenticated && User.HasClaim("Perfil", "Admin"))
        {
            return RedirectToAction("PainelAdmin");
        }
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Admin(string login, string senha)
    {
        // Busca o admin �nico no banco
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

    [Authorize]
    public IActionResult PesquisarSegurancaPendente(string pesquisa)
    {

        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(pesquisa))
        {
            return RedirectToAction("AprovarSeguranca");
        }

        var segurancas = _context.Segurancas
            .Where(s => s.isApproved == statusAprovacao.Pendente &&
                   (s.nome.Contains(pesquisa) || s.sobreNome.Contains(pesquisa) || s.cpf.Contains(pesquisa) || s.matricula.Contains(pesquisa)))
            .ToList();

        return View("AprovarSeguranca", segurancas);

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

    [Authorize]
    public IActionResult PesquisarSegurancaAprovado(string pesquisa) 
    {

        if (!User.HasClaim("Perfil", "Admin"))
        { 
            return Forbid();
        }

        if(string.IsNullOrEmpty(pesquisa))
        {
            return RedirectToAction("VisualizarSegurancas");
        }

        var segurancas = _context.Segurancas
            .Where(s => s.isApproved == statusAprovacao.Aprovado &&
                   (s.nome.Contains(pesquisa) || s.sobreNome.Contains(pesquisa) || s.cpf.Contains(pesquisa) || s.matricula.Contains(pesquisa)))
            .ToList();

        return View("VisualizarSegurancas", segurancas);

    }


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

        var viewModel = new DetalhesContrachequeViewModel(id, seguranca.nome, listaContracheques);

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
        ModelState.Remove("nomeSeguranca"); // Remove a valida��o do nome, pois ele n�o � enviado no formul�rio
        if (ModelState.IsValid)
        {

            var contrachequeExistente = _context.Contracheques.FirstOrDefault(c =>
            c.mesUpload == model.mesUpload && c.anoUpload == model.anoUpload && c.segurancaId == model.segurancaId);

            int segurancaId = model.segurancaId;

            if (contrachequeExistente != null)
            {
                ModelState.AddModelError(string.Empty, "J� existe um contracheque para esse m�s e ano. Por favor, escolha outro per�odo.");
                return View(model);
            }

            if (model.arquivoPdf != null && model.arquivoPdf.Length > 0)
            {
                // Definir o nome do arquivo
                string nomeArquivo = $"Contracheque_{model.segurancaId}_{model.mesUpload}_{model.anoUpload}.pdf";

                // Caminho onde ser� salvo 
                string caminhoPasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracheques");
                string caminhoCompleto = Path.Combine(caminhoPasta, nomeArquivo);

                // Salvar o arquivo f�sico no HD
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await model.arquivoPdf.CopyToAsync(stream);
                }

                // Salvar as informa��es no Banco de Dados
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

        Console.WriteLine("ModelState inv�lido ou arquivo n�o selecionado.");

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ExcluirContracheque(int id)
    {
        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }
        Console.WriteLine("\n\n\n\n\n\nAcessando Rota\n\n\n\n\n\n\n");

        var contracheque = await _context.Contracheques.FindAsync(id);

        if (contracheque == null)
        {
            ModelState.AddModelError(string.Empty, "Esse contracheque n�o Existe mais");
            return NotFound();
        }

        int segurancaId = contracheque.segurancaId;

        Console.WriteLine($"\n\n\n\n\n\nID do contracheque a ser excluído: {id}\n\n\n\n\n\n\n");
        Console.WriteLine($"\n\n\n\n\n\n\n\n ID do seguranca relacionado: {segurancaId} \n\n\n\n\n\n\n\n");

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

    [Authorize]
    public IActionResult SegurancasRejeitados()
    {
        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }
        var segurancas = _context.Segurancas.Where(s => s.isApproved != statusAprovacao.Aprovado && s.isApproved != statusAprovacao.Pendente).ToList();
        return View(segurancas);
    }

    [Authorize]
    public IActionResult PesquisarSegurancaRejeitado(string pesquisa)
    {

        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(pesquisa))
        {
            return RedirectToAction("SegurancasRejeitados");
        }

        var segurancas = _context.Segurancas
            .Where(s => (s.isApproved == statusAprovacao.Rejeitado || s.isApproved == statusAprovacao.EmEdicao) &&
                   (s.nome.Contains(pesquisa) || s.sobreNome.Contains(pesquisa) || s.cpf.Contains(pesquisa) || s.matricula.Contains(pesquisa)))
            .ToList();

        return View("SegurancasRejeitados", segurancas);

    }

    [HttpPost]
    public async Task<IActionResult> SolicitarEdicao(int id)
    {

        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }

        var seguranca = await _context.Segurancas.FindAsync(id);

        if(seguranca == null) return NotFound();

        if(seguranca.isApproved != statusAprovacao.Rejeitado)
        {
            ModelState.AddModelError(string.Empty, "S� � poss�vel solicitar edi��o para contas rejeitadas.");
            return RedirectToAction("SegurancasRejeitados");
        }

        if(seguranca.isApproved == statusAprovacao.EmEdicao)
        {
            ModelState.AddModelError(string.Empty, "Edi��o j� solicitada para essa conta.");
            return RedirectToAction("SegurancasRejeitados");
        }

        if(seguranca.isApproved == statusAprovacao.Pendente || seguranca.isApproved == statusAprovacao.Aprovado)
        {
            ModelState.AddModelError(string.Empty, "S� � poss�vel solicitar edi��o para contas rejeitadas.");
            return RedirectToAction("SegurancasRejeitados");
        }

        seguranca.isApproved = statusAprovacao.EmEdicao;

        _context.Update(seguranca);
        await _context.SaveChangesAsync();


        //Quando a pessoa faz o cadastro, a conta j� � criada. A linha no banco � criada. 
        //Ent�o eu preciso modificar o status dessa conta de modo que a pessoa saiba que pode editar os dados.

        return RedirectToAction("SegurancasRejeitados");
    }

    [HttpPost]
    public async Task<IActionResult> CancelarEdicao(int id)
    {
        if (!User.HasClaim("Perfil", "Admin"))
        {
            return Forbid();
        }
        var seguranca = await _context.Segurancas.FindAsync(id);
        Console.WriteLine(seguranca.nome);
        if (seguranca == null) return NotFound();
        if (seguranca.isApproved != statusAprovacao.EmEdicao)
        {
            ModelState.AddModelError(string.Empty, "S� � poss�vel cancelar edi��o para contas que est�o em edi��o.");
            return RedirectToAction("SegurancasRejeitados");
        }
        seguranca.isApproved = statusAprovacao.Rejeitado;
        _context.Update(seguranca);
        await _context.SaveChangesAsync();
        return RedirectToAction("SegurancasRejeitados");
    }

}