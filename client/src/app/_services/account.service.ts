import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/internal/operators/map';
import { User } from '../_model/user';
import { BehaviorSubject, ReplaySubject } from 'rxjs';
import { USER_KEY } from '../constants';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = 'https://localhost:5001/api/';
  private currentUserSource = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(
    private http: HttpClient,
  ) { }

  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if (user) {
          localStorage.setItem(USER_KEY, JSON.stringify(user));
          this.currentUserSource.next(user);
        }
        return user;
      })
    )
  }

  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map((user) => {
        if (user) {
          localStorage.setItem(USER_KEY, JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    )
  }

  setCurrentUser(user: User) {
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem(USER_KEY);
    this.currentUserSource.next(null);
  }
}
