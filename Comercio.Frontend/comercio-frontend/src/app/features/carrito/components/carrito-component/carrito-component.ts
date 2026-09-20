import { Component, effect, inject, signal } from '@angular/core';
import { CarritoService } from '../../services/carrito-service';
import { CarritoResponse } from '../../models/carrito-response';
import { HttpErrorResponse } from '@angular/common/http';
import { CarritoAgregar } from '../../models/carrito-agregar';
import { CarritoModificar } from '../../models/carrito-modificar';
import { CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-carrito-component',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './carrito-component.html',
  styleUrl: './carrito-component.css',
})
export class CarritoComponent {
  private readonly _carritoService = inject(CarritoService);

  readonly carritoActual = signal<CarritoResponse | null>(null);
  readonly cargando = signal<boolean>(false);
  readonly carritoVacio = signal<boolean>(false);
  readonly error = signal<string |null>(null);

  readonly compraExitosa = signal<boolean>(false);

  constructor(){
    this.cargarCarrito();
  }

  cargarCarrito(): void{
    this.cargando.set(true)
    this.error.set(null)
    this._carritoService.verCarrito().subscribe({
      next: (dataCarrito) => {
        this.carritoActual.set(dataCarrito);
        this.cargando.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.cargando.set(false);
        if(err.status === 404){
          this.carritoVacio.set(true);
        }else{
          this.error.set("no pudimos cargar tu carrito");
        }
        console.error("error al cargar el carrito", err);
      }
    });
  }

  agregarItem(productoId: number, cantidad: number): void{
    this.error.set(null);
    
    const dto: CarritoAgregar = {productoId, cantidad};

    this._carritoService.agregarProductoCarrito(dto).subscribe({
      next: (dataCarrito) => {
        this.carritoActual.set(dataCarrito);
        this.carritoVacio.set(false);
      },
      error: (err : HttpErrorResponse) => {
        if(err.status === 409){
          this.error.set(err.error?.detail ?? 'no hay stock suficiente para esa cantidad');
        }else{
          this.error.set('no pudimos agregar el producto al carrito');
        }
        console.error('error al agregar al carrito', err);
      }
    });
  }

  modificarCantidad(productoId: number, cantidad: number): void{
    if(cantidad <= 0){
      this.eliminarItem(productoId);
      return;
    }
    
    this.error.set(null);

    const dto: CarritoModificar = {productoId, cantidad};

    this._carritoService.modificarProductoCarrito(productoId, dto).subscribe({
      next: () => {
        this.cargarCarrito();
      },
      error: (err:HttpErrorResponse) => {
        if(err.status === 409){
          this.error.set(err.error?.detail ?? 'no hay stock suficiente para esta cantidad');
        }else{
          this.error.set('no pudimos modificar el carrito');
        }
        console.error('Error a modificar la cantidad', err);
      }
    });
  }

  eliminarItem(productoId: number): void{
    this.error.set(null);

    this._carritoService.eliminarProductoCarrito(productoId).subscribe({
      next: () => {
        this.cargarCarrito();
      },
      error: (err : HttpErrorResponse) => {
        this.error.set('No pudimos eliminar el producto del carrito');
        console.error('Error al eliminar el producto', err);
      }
    });
  }

  vaciarCarrito(): void{
    this.error.set(null);

    this._carritoService.limpiarCarrito().subscribe({
      next: () => {
        this.carritoActual.set(null);
        this.carritoVacio.set(true);
      },
      error: (err:HttpErrorResponse) =>{
        this.error.set('no pudimos vaciar el carrito');
        console.error('Error al vaciar el carrito', err);
      }
    });
  }

  procesarCompra(): void {
  this.cargando.set(true);
  this.error.set(null);
  
  this._carritoService.limpiarCarrito().subscribe({
    next: () => {
      this.cargando.set(false);
      this.carritoActual.set(null);
      this.compraExitosa.set(true); 
    },
    error: (err: HttpErrorResponse) => {
      this.cargando.set(false);
      this.error.set('No pudimos procesar la compra.');
    }
  });
}
  


}
