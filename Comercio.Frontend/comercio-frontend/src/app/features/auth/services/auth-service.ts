import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { environment } from '../../../../environments/environment.development';
import { Login } from '../models/login';
import { Observable, tap } from 'rxjs';
import { AuthResponse } from '../models/auth-response';
import { Register } from '../models/register';


const TOKEN_KEY = 'comercio_token';
const ROL_KEY = 'comercio_rol';
const EMAIL_KEY = 'comercio_email';

@Injectable({
  providedIn: 'root',
})
export class AuthService {

  private readonly apiUrl: string = `${environment.apiUrl}/auth`;
  private readonly _httpClient = inject(HttpClient);

  readonly token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  readonly rol = signal<string | null>(localStorage.getItem(ROL_KEY));
  readonly email = signal<string | null>(localStorage.getItem(EMAIL_KEY));

  readonly estaAutenticado = computed(() => this.token() !== null);
  readonly esAdministrador = computed(() => this.rol() === 'Administrador');

  Login(loginDto: Login): Observable<AuthResponse> {
    return this._httpClient.post<AuthResponse>(`${this.apiUrl}/login`, loginDto).pipe(
      tap((auth) => this.guardarSesion(auth))
    );
  }

  Register(registerDto: Register): Observable<AuthResponse>{
    return this._httpClient.post<AuthResponse>(`${this.apiUrl}/register`, registerDto).pipe(
      tap((auth) => this.guardarSesion(auth))
    );
  }

  cerrarSesion(): void{
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(ROL_KEY);
    localStorage.removeItem(EMAIL_KEY);

    this.token.set(null);
    this.rol.set(null);
    this.email.set(null);
  }

  private guardarSesion(auth: AuthResponse): void{
    localStorage.setItem(TOKEN_KEY, auth.token);
    localStorage.setItem(ROL_KEY, auth.rol);
    localStorage.setItem(EMAIL_KEY, auth.email);

    this.token.set(auth.token);
    this.rol.set(auth.rol);
    this.email.set(auth.email);
  }
}
