export interface ResultadoPaginado<T> {
    numeroPagina: number;
    tamPagina: number;
    totalRegistros: number;
    totalPaginas: number;
    datos: T[];
}


