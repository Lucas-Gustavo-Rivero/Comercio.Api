import { PaginacionParams } from "../../../shared/models/paginacion-params.model";
import { ProductoOrdenarPor } from "./producto-ordenar-por.enum.model";

export interface ProductoQueryParametros extends PaginacionParams{
    busqueda: string | null;
    precioMin: number | null;
    precioMax: number | null;
    ordenarPor: ProductoOrdenarPor;
    descendente: boolean;
}

