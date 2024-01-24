import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs/internal/operators/map';

export const adminGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);
  return accountService.currentUser$.pipe(
    map(user => {
      if (!(user?.roles.includes('Admin') === true || user?.roles.includes('Moderator') === true)) {
        toastr.error('You cannot enter this area');
        return false;
      }
      return true;

    })
  )
}

