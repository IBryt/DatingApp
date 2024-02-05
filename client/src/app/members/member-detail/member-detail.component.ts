import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs';
import { Member } from 'src/app/_model/member';
import { Message } from 'src/app/_model/message';
import { User } from 'src/app/_model/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;
  slideChangeMessage = '';
  slides: any = [];
  activeTab: TabDirective | undefined;
  messages: Message[] | undefined;
  member: Member = {} as Member;
  user?: User;

  constructor(
    private route: ActivatedRoute,
    private messageService: MessageService,
    public presenceService: PresenceService,
    private accountService: AccountService,
    private router: Router,
  ) { }


  ngOnInit(): void {
    this.router.routeReuseStrategy.shouldReuseRoute = () => false;
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => this.user = user
    })

    this.route.data.subscribe({
      next: data => {
        this.member = data['member']
        this.getOnlineUser(this.member.userName)
      }
    })

    for (const photo of this.member.photos) {
      this.slides.push({ image: photo.url })
    }

    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] ? this.selectTab(3) : this.selectTab(0);
      }
    })
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection()
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && !this.messages?.length && this.user) {
      this.messageService.createHubConnection(this.user, this.member.userName);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  selectTab(tabId: number) {
    this.memberTabs!.tabs[tabId].active = true;
  }

  getOnlineUser(username: string) {
    this.presenceService.getOnlineUsers([username]);
  };

  loadMessages() {
    if (this.member) {
      this.messageService.getMessageThread(this.member?.userName).subscribe({
        next: resp => {
          this.messages = resp;
        }
      })
    }
  }
}
