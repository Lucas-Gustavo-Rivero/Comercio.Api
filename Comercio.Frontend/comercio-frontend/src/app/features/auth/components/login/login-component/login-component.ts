import { Component, inject, signal } from '@angular/core';
import { AuthService } from '../../../services/auth-service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Login } from '../../../models/login';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-login-component',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login-component.html',
  styleUrl: './login-component.css',
})
export class LoginComponent {
  
  private readonly _authService = inject(AuthService);
  private readonly _fb = inject(FormBuilder);
  private readonly _router = inject(Router);

  readonly cargando = signal<boolean>(false);
  readonly errorCredencial = signal<string | null>(null);
  readonly errorGeneral = signal<string | null>(null);

  
  readonly loginForm = this._fb.group({
    "email": ["", [Validators.required, Validators.maxLength(256), Validators.email]],
    "password": ["", [Validators.required]]
  });

  
  userLogin(): void{

    if(this.loginForm.invalid){
      this.loginForm.markAllAsTouched();
      return;
    }

    this.cargando.set(true);
    this.errorCredencial.set(null);
    this.errorGeneral.set(null);
    
    const valores = this.loginForm.getRawValue();    
    this._authService.Login({email: valores.email!, password: valores.password!}).subscribe({
      next: () => {
        this.cargando.set(false);
        this._router.navigate(['/productos']);
      },
      error: (err : HttpErrorResponse) => {
        this.cargando.set(false);
        if(err.status === 401){
          this.errorCredencial.set(err.error?.detail ?? 'email o contraseña incorrectos.');
        }else{
          this.errorGeneral.set('Ocurrio un error. intente mas tarde.');
        }
      }
    });
  }
}
