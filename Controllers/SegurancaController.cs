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

}
