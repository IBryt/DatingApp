import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_model/member';
import { Message } from 'src/app/_model/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;
  slideChangeMessage = '';
  slides: any = [];
  activeTab: TabDirective | undefined;
  messages: Message[] | undefined;
  member: Member = {} as Member;;

  constructor(
    private membersService: MembersService,
    private route: ActivatedRoute,
    private messageService: MessageService,
  ) { }

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member']
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

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && !this.messages?.length) {
      this.loadMessages();
    }
  }

  selectTab(tabId: number) {
    this.memberTabs!.tabs[tabId].active = true;
  }

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
