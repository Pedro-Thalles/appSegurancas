//Este é um arquivo de configuração do Entity Framework Core para o banco de dados da aplicação.
//Ele define o contexto do banco de dados, as entidades que serão mapeadas para as tabelas e as regras de configur
using Microsoft.EntityFrameworkCore;
using appSegurancas.Models;

namespace appSegurancas.Data;

public class segDbContext : DbContext
{
	public segDbContext(DbContextOptions<segDbContext> options) : base(options) { }
	public DbSet<Seguranca> Segurancas { get; set; }
	public DbSet<Contracheque> Contracheques { get; set; }
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Seguranca>().HasIndex(s => s.cpf).IsUnique();
		modelBuilder.Entity<Seguranca>().HasIndex(s => s.matricula).IsUnique();
	}
}		