public class DetalhesContrachequeViewModel
{
    public int segurancaId { get; set; }
    public string nomeSeguranca { get; set; } = string.Empty;
    public IEnumerable<appSegurancas.Models.Contracheque> contracheques { get; set; } = new List<appSegurancas.Models.Contracheque>();

    public DetalhesContrachequeViewModel(int id, string nome, IEnumerable<appSegurancas.Models.Contracheque> lista)
    {
        segurancaId = id;
        nomeSeguranca = nome;
        contracheques = lista;
    }

    public DetalhesContrachequeViewModel() { }

}