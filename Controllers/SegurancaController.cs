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
                    //esse é o objetivo, se chegar aqui, mata o login 
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

    /*public async Task<IActionResult> GerarDadosTeste(int id)
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
    }*/

    //GET: /Seguranca/VisualizarSegurancas

    public IActionResult VisualizarSegurancas()
    {
        var segurancas = _context.Segurancas.Where(s => s.isApproved == true).ToList();
        return View(segurancas);
    }


    // GET: /Seguranca/UploadContracheque/5
    public IActionResult UploadContracheque(int id)
    {
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
                return RedirectToAction("VisualizarSegurancas");
            }
        }

        Console.WriteLine("ModelState inválido ou arquivo não selecionado.");

        return View(model);
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




}