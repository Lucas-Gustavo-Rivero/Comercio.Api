import { Component, effect, inject, signal } from '@angular/core';
import { ProductoService } from '../../services/producto.service';
import { Producto } from '../../models/producto.model';
import { ProductoQueryParametros } from '../../models/producto-query-parametros.model';
import { ProductoOrdenarPor } from '../../models/producto-ordenar-por.enum.model';
import { CurrencyPipe } from '@angular/common';
import { RouterLink } from "@angular/router";
import { AuthService } from '../../../auth/services/auth-service';
import { CarritoService } from '../../../carrito/services/carrito-service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-producto-list.component',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './producto-list.component.html',
  styleUrl: './producto-list.component.css',
})
export class ProductoListComponent {

  private readonly PARAMETROS_INICIALES: ProductoQueryParametros = {
    numeroPagina: 1,
    tamPagina: 8,
    busqueda: null,
    ordenarPor: ProductoOrdenarPor.Id,
    precioMax: null,
    precioMin: null,
    descendente: false
  }
  private _productoService = inject(ProductoService);
  readonly authService = inject(AuthService);
  private _carritoService = inject(CarritoService);

  readonly productos = signal<Producto[]>([]);
  readonly cargando = signal<boolean>(false);
  readonly totalPaginas = signal<number>(0);


  parametros = signal<ProductoQueryParametros>({ ...this.PARAMETROS_INICIALES});


  constructor(){
    effect(() => {
      this.cargarProductos(this.parametros());
    })
  }

  paginaAnterior(): void{
    const paramsActual = this.parametros();
    if(paramsActual.numeroPagina > 1){
      this.parametros.update(p => ({...p, numeroPagina: p.numeroPagina - 1}));
    }
  }

  paginaSiguiente(): void{
    const paramsActual = this.parametros();
    if(paramsActual.numeroPagina < this.totalPaginas()){
      this.parametros.update(p => ({...p, numeroPagina: p.numeroPagina + 1}));
    }
  }

  buscar(termino: string):void{
    this.parametros.update(p => ({
      ...p, busqueda: termino.trim() !== '' ? termino : null, numeroPagina: 1
    }));
  }

  cambiarOrden(valorSeleccionado: string): void{
    const clave = valorSeleccionado as keyof typeof ProductoOrdenarPor;
    const orden = ProductoOrdenarPor[clave] ?? ProductoOrdenarPor.Id;

    this.parametros.update(p => ({
      ...p, ordenarPor: orden, numeroPagina:1
    }));
  }

  alternarDireccion():void{
    this.parametros.update(p => ({
      ...p, descendente: !p.descendente, numeroPagina: 1
    }));
  }

  filtrarPrecio(precioMin: string, precioMax: string): void{
    const valorMin = parseFloat(precioMin);
    const valorMax = parseFloat(precioMax);

    this.parametros.update(p => ({
      ...p,precioMin: !isNaN(valorMin) ? valorMin : null, precioMax: !isNaN(valorMax) ? valorMax : null, numeroPagina: 1
    }));
  }

  limpiarFiltros(inputBusqueda: HTMLInputElement, inputMin: HTMLInputElement, inputMax: HTMLInputElement): void{
    inputBusqueda.value = '';
    inputMax.value = '';
    inputMin.value = '';

    this.parametros.set({...this.PARAMETROS_INICIALES});
  }

  private cargarProductos(params: ProductoQueryParametros): void{
    this.cargando.set(true);

    this._productoService.getProductos(params).subscribe({
      next: (repuesta) => {
        this.productos.set(repuesta.datos);
        this.totalPaginas.set(repuesta.totalPaginas);
        this.cargando.set(false);
      },
      error: (err) => {
        this.cargando.set(false);
        console.error('Error al cargar la API', err);
      }
    });
  }

  agregarAlCarrito(productoId: number): void{
    this._carritoService.agregarProductoCarrito({productoId: productoId, cantidad: 1}).subscribe({
      next: () => alert('Producto agregado al carrito'),
      error: (err:HttpErrorResponse) => {
        if(err.status === 409){
          alert(err.error?.detail ?? 'no hay stock suficiente para este producto');
        }else{
          alert('No pudimos agregarlo al carrito intente mas tarde.')
        }
        console.error('Error al agregar el producto al carrito',err);
      }
    });
  }
}
