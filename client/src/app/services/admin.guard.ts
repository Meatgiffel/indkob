import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const adminGuard: CanActivateFn = async () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  await auth.ensureLoaded();
  const user = auth.snapshot;
  if (user && user.isAdmin) {
    return true;
  }
  return router.parseUrl('/list');
};

