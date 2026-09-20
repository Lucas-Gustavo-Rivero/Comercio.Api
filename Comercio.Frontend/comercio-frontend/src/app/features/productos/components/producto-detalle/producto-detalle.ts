import { Component, effect, inject, input, signal } from '@angular/core';
import { ProductoService } from '../../services/producto.service';
import { Producto } from '../../models/producto.model';
import { HttpErrorResponse } from '@angular/common/http';
import { CurrencyPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../auth/services/auth-service';
import { CarritoService } from '../../../carrito/services/carrito-service';

@Component({
  selector: 'app-producto-detalle',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './producto-detalle.html',
  styleUrl: './producto-detalle.css',
})
export class ProductoDetalle {

  private _productoService = inject(ProductoService);
  private _carritoService = inject(CarritoService);
  private router = inject(Router);
  readonly authService = inject(AuthService);
  readonly id = input.required<string>();

  readonly producto = signal<Producto | null>(null);
  readonly cargando = signal<boolean>(true);
  readonly noEncontrado = signal<boolean>(false);
  readonly error = signal<boolean>(false);

  readonly agregando = signal<boolean>(false);
  readonly mensajeCarrito = signal<string | null>(null);

  readonly cantidad = signal<number>(1);


  constructor(){
    effect(() => {
      this.cargarProducto(Number(this.id()));
    });
  }
  
  private cargarProducto(id: number): void{
    this.cargando.set(true);
    this.noEncontrado.set(false);
    this.error.set(false);

    this._productoService.getProducto(id).subscribe({
      next: (producto) => {
        this.producto.set(producto);
        this.cargando.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.cargando.set(false);
        if(err.status == 404){
          this.noEncontrado.set(true);
        }else{
          this.error.set(true);
        }
        console.error('Error al cargar el producto', err);
      } 
    });
  }

  eliminarProducto(): void{
    const confirmado = confirm("Seguro que desea eliminar este producto?");
    if(!confirmado){
      return;
    }

    this._productoService.deleteProducto(Number(this.id())).subscribe({
      next: () => this.router.navigate(['/productos']),
      error: (err: HttpErrorResponse) => {
        if(err.status == 404){
          this.noEncontrado.set(true);
        }else if(err.status === 401 || err.status === 403){
          console.error('Tu sesion expiro o no tenes permisos, vuelva a iniciar sesion');
        }
        else{
          this.error.set(true);
        }
        console.error('Error al eliminar el producto', err);
      }
    });
  }

  modificarCantidad(valor: number): void{
    const nuevo = this.cantidad() + valor;

    if(nuevo >= 1 && nuevo <= 100){
      this.cantidad.set(nuevo);
    }
  }

  agregarAlCarrito(): void{
    const producto = this.producto();

    if(!producto){
      return;
    }
    this.agregando.set(true);
    this.mensajeCarrito.set(null);

    this._carritoService.agregarProductoCarrito({productoId: producto.id, cantidad: this.cantidad() }).subscribe({
      next: () => {
        this.agregando.set(false);
        this.mensajeCarrito.set('Producto agregado al carrito');
      },
      error: (err: HttpErrorResponse) => {
        this.agregando.set(false);
        this.mensajeCarrito.set(err.status === 409 ? (err.error?.detail ?? 'no hay stock suficiente') : 'no pudimos agregarlo al carrito');
      }
    });
    
  }

}
