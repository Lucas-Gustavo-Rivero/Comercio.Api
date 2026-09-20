import { Component, inject, signal } from '@angular/core';
import { AuthService } from '../../services/auth-service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly _authService = inject(AuthService);
  private readonly _fb = inject(FormBuilder);
  private readonly _router = inject(Router);

  readonly cargando = signal<boolean>(false);
  readonly errorConflicto = signal<string | null>(null);
  readonly errorGeneral = signal<string | null>(null);
  readonly erroresValidacion = signal<Record<string, string[]>>({});

  readonly registerForm = this._fb.group({
    "nombreCompleto": ["", [Validators.required, Validators.maxLength(100)]],
    "email": ["",[Validators.required, Validators.maxLength(256), Validators.email]],
    "password": ["",[Validators.required]]
  });

  userRegister(){
    if(this.registerForm.invalid){
      this.registerForm.markAllAsTouched();
      return;
    }

    this.cargando.set(true);
    this.errorConflicto.set(null);
    this.errorGeneral.set(null);
    this.erroresValidacion.set({});

    const valores = this.registerForm.getRawValue();

    this._authService.Register({nombreCompleto: valores.nombreCompleto!,
      email: valores.email!,
      password: valores.password!
    }).subscribe({
      next: (auth) => {
        this.cargando.set(false);
        this._router.navigate(['/productos']);
      },
      error: (err : HttpErrorResponse) => {
        this.cargando.set(false);
        console.error('Error al registrarse.',err);
        if(err.status === 409){
          this.errorConflicto.set(err.error?.detail ?? 'Ya existe un email asociado.');
        }else if(err.status === 400){
          this.erroresValidacion.set(err.error?.errors ?? {});
        }else{
          this.errorGeneral.set('Ocurrio un problema al registrarse, vuelva a intentarlo mas tarde');
        }
      }
    });
  }

  errorDeCampo(nombreCampo: string): string | null{
    const errores = this.erroresValidacion();
    const clave = Object.keys(errores).find(k => k.toLowerCase() === nombreCampo.toLowerCase());
    return clave ? errores[clave][0] : null;
  }
}
