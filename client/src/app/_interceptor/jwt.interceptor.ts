import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, mergeMap, take } from 'rxjs';
import { AccountService } from '../_services/account.service';
import { User } from '../_model/user';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

  constructor(
    private accountService: AccountService,
  ) { }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return this.accountService.currentUser$.pipe(
      take(1),
      mergeMap((currentUser: User | null) => {
        if (currentUser) {
          const modifiedRequest = request.clone({
            setHeaders: {
              Authorization: `Bearer ${currentUser.token}`
            }
          });
          return next.handle(modifiedRequest);
        } else {
          // Handle the case where there is no current user (e.g., redirect to login)
          return next.handle(request);
        }
      })
    );
  }
}


