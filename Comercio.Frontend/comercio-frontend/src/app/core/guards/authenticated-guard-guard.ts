import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../features/auth/services/auth-service';

export const authenticatedGuardGuard: CanActivateFn = (route, state) => {
  const _authService = inject(AuthService);
  const _router = inject(Router);

  if(_authService.estaAutenticado()){
    return true;
  }

  return _router.navigate(['/login']);
};
