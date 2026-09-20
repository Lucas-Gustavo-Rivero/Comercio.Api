import { Component, effect, inject, input, signal } from '@angular/core';
import { ProductoService } from '../../services/producto.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ProductoEdicion } from '../../models/producto-modificar.model';
import { find } from 'rxjs';

@Component({
  selector: 'app-producto-modificar',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './producto-modificar.html',
  styleUrl: './producto-modificar.css',
})
export class ProductoModificar {
  
  private readonly _productoService = inject(ProductoService);
  private readonly _fb = inject(FormBuilder);
  private readonly _router = inject(Router);

  readonly id = input.required<string>();
  readonly rowVersionActual = signal<string>('');
  readonly guardando = signal<boolean>(false);
  readonly errorConflicto = signal<string | null>(null);
  readonly errorGeneral = signal<string | null>(null);
  readonly errorValidacion = signal<Record<string, string[]>>({});

  form = this._fb.group({
    nombre: ['', [Validators.required, Validators.maxLength(50)]],
    descripcion: ['',[Validators.maxLength(500)]],
    precio: [null as number | null, [Validators.required, Validators.min(0.01), Validators.max(1000000)]],
    stock: [null as number | null, [Validators.required, Validators.min(0)]],
    urlImagen: ['',[Validators.maxLength(500)]]
  });

  constructor(){
    effect(() => {
      this.cargarDatosParaEditar(Number(this.id()));
    })
  }

  cargarDatosParaEditar(id: number): void{
    this.errorConflicto.set(null);
    this._productoService.getProducto(id).subscribe({
      next: (productoDb) => {
        this.form.patchValue({
          nombre: productoDb.nombre,
          descripcion: productoDb.descripcion,
          precio: productoDb.precio,
          stock: productoDb.stock,
          urlImagen: productoDb.urlImagen
        });
        this.rowVersionActual.set(productoDb.rowVersion);
      },
      error: (err: HttpErrorResponse) =>{
        console.error('Hubo un problema al modificar el producto', err);
      }
    });
  }

  guardarProducto(): void{
    if(this.form.invalid){
      this.form.markAllAsTouched();
      return;
    }
    
    const valores = this.form.getRawValue();

    const productoModificado: ProductoEdicion = {
      id: Number(this.id()),
      nombre: valores.nombre!,
      descripcion: valores.descripcion,
      precio: valores.precio!,
      stock: valores.stock!,
      urlImagen: valores.urlImagen,
      rowVersion: this.rowVersionActual()
    };
    
    this.guardando.set(true);

    this._productoService.updateProducto(Number(this.id()), productoModificado).subscribe({
      next: () => {
        this.guardando.set(false);
        this._router.navigate(['/productos', this.id()]);
      },
      error: (err: HttpErrorResponse) => {
        this.guardando.set(false);
        if(err.status == 409){
          this.errorConflicto.set(err.error?.detail ?? 'El producto fue modificado por otro usuario');
        }else if(err.status == 400){
          this.errorValidacion.set(err.error?.errors ?? {});
        }else if(err.status === 401 || err.status === 403){
          this.errorGeneral.set('Tu sesion expiro o no tenes permisos, vuelva a iniciar sesion');
        }
        else{
          this.errorGeneral.set('Ocurrio un error al modificar el producto');
        }
      }
    });
  }
  errorDeCampo(nombreCampo: string): string | null{
    const errores = this.errorValidacion();
    const clave = Object.keys(errores).find(k => k.toLowerCase() === nombreCampo.toLowerCase());
    return clave ? errores[clave][0] : null;
  }
}
