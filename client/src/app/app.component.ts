import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from './_model/user';
import { USER_KEY } from './constants';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating app';
  users: any;

  constructor(
    private http: HttpClient,
    private accountService: AccountService
  ) { }

  ngOnInit(): void {
    this.getUsers();
    this.setCurrentUser();
  }

  setCurrentUser() {
    const json = localStorage.getItem(USER_KEY)

    if (!json) {
      return;
    }

    const user: User = JSON.parse(json);
    this.accountService.setCurrentUser(user);
  }

  getUsers() {
    this.http.get('https://localhost:5001/api/users')
      .subscribe({
        next: response => this.users = response,
        error: error => console.log(error),
      });
  }
}
