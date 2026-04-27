using System.ComponentModel.DataAnnotations;

namespace appSegurancas.Models;

public class  Contracheque
{
    public int id { get; set; }
	/*os colchetes servem para indicar que o campo é obrigatório,
    ou seja, não pode ser nulo. Isso é importante para garantir
    a integridade dos dados no banco de dados e evitar erros de validação.*/
	[Required]
    public int segurancaId { get; set; }

    public int mesUpload { get; set; }
    public int anoUpload { get; set; }

    [Required]
    public string filePath { get; set; } = string.Empty;// string.Empty é para evitar campo nulo

	public DateTime createdAt { get; set; } = DateTime.Now;
    public DateTime updatedAt { get; set; } = DateTime.Now;


    public Seguranca? seguranca { get; set; }//propriedade de navegação 
}