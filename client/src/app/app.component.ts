import { Component, OnInit } from '@angular/core';
import { User } from './_model/user';
import { AccountService } from './_services/account.service';
import { AppConstants } from './constants';
import { PresenceService } from './_services/presence.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating app';
  users: any;

  constructor(
    private accountService: AccountService,
    private presenceService: PresenceService,
  ) { }

  ngOnInit(): void {
    this.setCurrentUser();
  }

  setCurrentUser() {
    const json = localStorage.getItem(AppConstants.USER_STORAGE_KEY)

    if (!json) {
      return;
    }

    const user: User = JSON.parse(json);
    this.accountService.setCurrentUser(user);
    this.presenceService.createHubConnection(user);
  }
}
