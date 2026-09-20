import { computed, inject, Injectable, signal } from '@angular/core';
import { environment } from '../../../../environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { CarritoResponse } from '../models/carrito-response';
import { CarritoAgregar } from '../models/carrito-agregar';
import { CarritoModificar } from '../models/carrito-modificar';

@Injectable({
  providedIn: 'root',
})
export class CarritoService {
  private readonly apiUrl: string = `${environment.apiUrl}/carritos`;
  private readonly _httpClient = inject(HttpClient);

  readonly carrito = signal<CarritoResponse | null>(null);
  
  readonly cantidadTotalItem = computed(() => {
    const c = this.carrito();
    return c ? c.items.reduce((total, item) => total + item.cantidad, 0) : 0;
  })

  verCarrito(): Observable<CarritoResponse>{
    return this._httpClient.get<CarritoResponse>(this.apiUrl).pipe(
      tap((carrito) => this.carrito.set(carrito))
    );
  }

  agregarProductoCarrito(carrito: CarritoAgregar): Observable<CarritoResponse>{
    return this._httpClient.post<CarritoResponse>(`${this.apiUrl}/items`, carrito).pipe(
      tap((carrito) => this.carrito.set(carrito))
    );
  }

  modificarProductoCarrito(productoId: number, carrito: CarritoModificar): Observable<void>{
    return this._httpClient.put<void>(`${this.apiUrl}/items/${productoId}`, carrito);
  }

  eliminarProductoCarrito(productoId: number): Observable<void>{
    return this._httpClient.delete<void>(`${this.apiUrl}/items/${productoId}`);
  }

  limpiarCarrito(): Observable<void>{
    return this._httpClient.delete<void>(this.apiUrl).pipe(
      tap(() => this.carrito.set(null))
    );
  }
}
