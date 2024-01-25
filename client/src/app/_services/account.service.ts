import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/internal/operators/map';
import { User } from '../_model/user';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environment/environment';
import { AppConstants } from '../constants';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new BehaviorSubject<User | undefined>(undefined);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(
    private http: HttpClient,
    private presenceService: PresenceService,
  ) { }

  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user);
          this.presenceService.createHubConnection(user);
        }
        return user;
      })
    )
  }

  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map((user) => {
        if (user) {
          this.setCurrentUser(user);
          this.presenceService.createHubConnection(user);
        }
      })
    )
  }

  setCurrentUser(user: User) {
    const roles = this.getDecoderetToken(user.token).role;
    Array.isArray(roles) ? user.roles = roles : user.roles = [roles];

    localStorage.setItem(AppConstants.USER_STORAGE_KEY, JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem(AppConstants.USER_STORAGE_KEY);
    this.currentUserSource.next(undefined);
    this.presenceService.stopHubConnection();
  }

  getDecoderetToken(token: string) {
    return JSON.parse(atob(token.split('.')[1]))
  }
}
