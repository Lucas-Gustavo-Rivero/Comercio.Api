import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment.development';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ProductoQueryParametros } from '../models/producto-query-parametros.model';
import { Observable } from 'rxjs';
import { ResultadoPaginado } from '../models/resultado-paginado.model';
import { Producto } from '../models/producto.model';
import { ProductoCrear } from '../models/producto-crear.model';
import { ProductoEdicion } from '../models/producto-modificar.model';

@Injectable({
  providedIn: 'root',
})
export class ProductoService {
  
  private apiUrl: string = `${environment.apiUrl}/productos`;
  private _httpClient = inject(HttpClient);

  getProductos(parametros: ProductoQueryParametros): Observable<ResultadoPaginado<Producto>>{
    let params = new HttpParams();

    Object.entries(parametros).forEach(([Key, valor]) => {
      if(valor !== null && valor !== undefined && valor !== ''){
        params = params.set(Key, valor.toString());
      }
    });

    return this._httpClient.get<ResultadoPaginado<Producto>>(this.apiUrl, {params});
  }

  getProducto(id: number): Observable<Producto>{
    return this._httpClient.get<Producto>(`${this.apiUrl}/${id}`);
  }

  createProducto(nuevo: ProductoCrear): Observable<Producto>{
    return this._httpClient.post<Producto>(this.apiUrl, nuevo);
  }

  updateProducto(id: number, producto: ProductoEdicion): Observable<void>{
    return this._httpClient.put<void>(`${this.apiUrl}/${id}`, producto);
  }

  deleteProducto(id: number): Observable<void>{
    return this._httpClient.delete<void>(`${this.apiUrl}/${id}`);
  }
}
