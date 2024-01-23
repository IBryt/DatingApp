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
  @ViewChild('memberTabs') memberTabs: TabsetComponent | undefined;
  slideChangeMessage = '';
  slides: any = [];
  activeTab: TabDirective | undefined;
  messages: Message[] | undefined;
  member: Member | undefined;

  constructor(
    private membersService: MembersService,
    private route: ActivatedRoute,
    private messageService: MessageService,
  ) { }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.membersService.getMember(this.route.snapshot.paramMap.get('username')!).subscribe({
      next: member => {
        this.member = member;
        for (const photo of this.member.photos) {
          this.slides.push({ image: photo.url })
        }
      },
    })
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && !this.messages?.length) {
      this.loadMessages();
    }
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
