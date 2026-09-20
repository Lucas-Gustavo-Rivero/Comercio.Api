import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../../features/auth/services/auth-service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  
  const authService = inject(AuthService);
  const token = authService.token();

  if(token){
    const reqClonado = req.clone({
      setHeaders: {Authorization: `Bearer ${token}`}
    });

    return next(reqClonado);
  }

  return next(req);
};
