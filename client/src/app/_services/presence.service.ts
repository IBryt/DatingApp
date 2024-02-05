import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environment/environment';
import { User } from '../_model/user';
import { BehaviorSubject, take } from 'rxjs';
import { Router } from '@angular/router';
import { HubHelpers } from '../_helpers/hub-helpers';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  private onlineUserSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUserSource.asObservable();

  constructor(
    private toastr: ToastrService,
    private router: Router,
  ) { }

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build()

    this.hubConnection
      .start()
      .catch(error => console.log(error));

    this.hubConnection.on('UserIsOnline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe({
        next: usernames => {
          this.onlineUserSource.next([...usernames, username])
        }
      });
    })

    this.hubConnection.on('UserIsOffline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe({
        next: usernames => {
          this.onlineUserSource.next(usernames.filter(u => u !== username))
        }
      });
    })

    this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUserSource.next(usernames);
    })

    this.hubConnection.on('NewMessageReceived', ({ username, knownAs }) => {
      this.toastr.info(knownAs + ' has sent you a new message!')
        .onTap
        .pipe(take(1))
        .subscribe({
          next: _ => this.router.navigateByUrl('/members/' + username + '?tab=3')
        })
    })
  }
  
  async getOnlineUsers(users: string[]): Promise<any> {
    await HubHelpers.waitForHubConnection(this.hubConnection);
    return this.hubConnection?.invoke('GetOnlineUsers', users)
      .catch(error => console.log(error));
  }

  stopHubConnection() {
    this.hubConnection!.stop()
      .catch(error => console.log(error));
  }
}
