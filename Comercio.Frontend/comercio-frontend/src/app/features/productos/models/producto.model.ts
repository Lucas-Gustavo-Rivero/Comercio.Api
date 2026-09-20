export interface Producto {
    id: number;
    nombre: string;
    descripcion: string | null;
    precio: number;
    stock: number;
    urlImagen: string | null;
    rowVersion: string;
}


