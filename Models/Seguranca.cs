using System.ComponentModel.DataAnnotations;

namespace appSegurancas.Models;

public class Seguranca
{
    public int id { get; set; }
    /*os colchetes servem para indicar que o campo é obrigatório,
    ou seja, não pode ser nulo. Isso é importante para garantir
    a integridade dos dados no banco de dados e evitar erros de validação.*/
    [Required, MaxLength(100)]
    public string nome { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string sobreNome { get; set; } = string.Empty;

    [Required, MaxLength(11)]
    public string cpf { get; set; } = string.Empty;

    [Required]
    public string passwordHash { get; set; } = string.Empty;

    [EmailAddress]
    public string email { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string matricula { get; set; } = string.Empty;

    public string role { get; set; } = "User";

    public bool isApproved { get; set; } = false;

    public DateTime createdAt { get; set; } = DateTime.Now;
    public DateTime updatedAt { get; set; } = DateTime.Now;

    public List<Contracheque> Contracheques { get; set; } = new();


}