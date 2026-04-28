using System.ComponentModel.DataAnnotations;

namespace appSegurancas.Models;

public class ContrachequeViewModel
{
    public int segurancaId { get; set; }
    public string nomeSeguranca { get; set; } // Só para exibir na tela, pois já passo o id dele ao clicar em cadastrar contracheque

    [Required(ErrorMessage = "Informe o mês")]
    [Range(1, 12)]
    public int mesUpload { get; set; }

    [Required(ErrorMessage = "Informe o ano")]
    public int anoUpload { get; set; }

    [Required(ErrorMessage = "Selecione o arquivo PDF")]
    public IFormFile arquivoPdf { get; set; } // Aqui é onde o arquivo "viaja"
}