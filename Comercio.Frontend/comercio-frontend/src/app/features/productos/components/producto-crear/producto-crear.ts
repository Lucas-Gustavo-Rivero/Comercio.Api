import { Component, inject, signal } from '@angular/core';
import { ProductoService } from '../../services/producto.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { single } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-producto-crear',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './producto-crear.html',
  styleUrl: './producto-crear.css',
})
export class ProductoCrear {
  private readonly _productoService = inject(ProductoService);
  private readonly _fb = inject(FormBuilder);
  private readonly _router = inject(Router);

  readonly guardando = signal<boolean>(false);
  readonly errorConflicto = signal<string | null>(null);
  readonly errorGeneral = signal<string | null>(null);
  readonly errorValidacion = signal<Record<string, string[]>>({});

  readonly form = this._fb.group({
    nombre: ['', [Validators.required, Validators.maxLength(50)]],
    descripcion: ['', [Validators.maxLength(500)]],
    precio: [null as number | null, [Validators.required, Validators.min(0.01), Validators.max(1000000)]],
    stock: [null as number | null, [Validators.required, Validators.min(0)]],
    urlImagen: ['',[Validators.maxLength(500)]]
  });


  guardar(): void{
    if(this.form.invalid){
      this.form.markAllAsTouched();
      return;
    }
    this.guardando.set(true);
    this.errorConflicto.set(null);
    this.errorGeneral.set(null);
    this.errorValidacion.set({});

    const valores = this.form.getRawValue();

    this._productoService.createProducto({
      nombre: valores.nombre!,
      descripcion: valores.descripcion || null,
      precio: valores.precio!,
      stock: valores.stock!,
      urlImagen: valores.urlImagen || null
    }).subscribe({
      next: (producto) => {
        this.guardando.set(false);
        this._router.navigate(['/productos', producto.id]);
      },
      error: (err: HttpErrorResponse) => {
        this.guardando.set(false);
        console.error("Error al crear el producto", err.error);
        if(err.status === 409){
          this.errorConflicto.set(err.error?.detail ?? 'Ya existe un producto con ese nombre');
        }else if(err.status === 400){
          this.errorValidacion.set(err.error?.errors ?? {});
        }else if(err.status === 401 || err.status === 403){
          this.errorGeneral.set('Tu sesion expiro o no tenes permisos, vuelva a iniciar sesion');
        }
        else{
          this.errorGeneral.set('Ocurrio un error al crear el producto, vuelva a intentarlo mas tarde');
        }
      }
    })
  }

  errorDeCampo(nombreCampo: string): string | null{
    const errores = this.errorValidacion();
    const clave = Object.keys(errores).find(k => k.toLowerCase() === nombreCampo.toLowerCase());
    return clave ? errores[clave][0] : null;
  }
}
