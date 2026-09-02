namespace Comercio.Api.DTOs.Paginacion
{
    public class PaginacionParams
    {
        private const int TamMaximoPagina = 50;
        private int _numeroPagina = 1;
        private int _tamPagina = 10;

        public int NumeroPagina
        {
            get => _numeroPagina;
            set => _numeroPagina = value < 1 ? 1 : value;
        }
        public int TamPagina
        {
            get => _tamPagina;
            set => _tamPagina = value switch
            {
                > TamMaximoPagina => TamMaximoPagina,
                < 1 => 1,
                _ => value
            };
        }

    }
}
