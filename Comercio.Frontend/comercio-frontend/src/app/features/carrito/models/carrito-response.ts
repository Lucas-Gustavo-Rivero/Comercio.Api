import { CarritoItem } from "./carrito-item";

export interface CarritoResponse {
    carritoId: number;
    items: CarritoItem[];
    total: number;
}

