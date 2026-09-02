namespace Comercio.Api.DTOs.Paginacion
{
    //Por el momento sabemos que el tamaño de pagina nunca va a ser cero, ya que el PaginacionParams verifica que no sea cero dicho atributo
    public record ResultadoPaginado<T>(IEnumerable<T> Datos, int NumeroPagina, int TamPagina, int TotalRegistros)
    {
        public int TotalPaginas { get; } = (int)Math.Ceiling((double)TotalRegistros / TamPagina);   
    }
}
