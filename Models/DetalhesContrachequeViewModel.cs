public class DetalhesContrachequeViewModel
{
    public int segurancaId { get; set; }
    public string nomeSeguranca { get; set; }
    public IEnumerable<appSegurancas.Models.Contracheque> contracheques { get; set; }
}