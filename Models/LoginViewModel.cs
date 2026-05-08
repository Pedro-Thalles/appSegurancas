using System.ComponentModel.DataAnnotations;
namespace appSegurancas.Models;

public class LoginViewModel
{
	[Required(ErrorMessage = "O CPF é obrigatório")]
	public string cpf { get; set; }

	[Required(ErrorMessage = "A senha é obrigatória")]
	[DataType(DataType.Password)]
	public string senha { get; set; }

	public LoginViewModel(string cpf, string senha)
	{
		this.cpf = cpf;
		this.senha = senha;
	}

	public LoginViewModel() { }


}