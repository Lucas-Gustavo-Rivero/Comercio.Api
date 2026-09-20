import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './features/auth/services/auth-service';
import { CarritoService } from './features/carrito/services/carrito-service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  readonly authService = inject(AuthService);
  private readonly _router = inject(Router);
  readonly carritoService = inject(CarritoService);

  cerrarSesion(): void{
    this.authService.cerrarSesion();
    this._router.navigate(['/login']);
  }
}
