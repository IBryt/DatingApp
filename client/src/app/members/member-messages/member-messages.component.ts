import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_model/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm?: NgForm
  @Input() username: string = '';
  @Input() messages: Message[] | undefined;
  messageContent = '';
  loading = false;

  constructor(
    public messageService: MessageService,
  ) { }

  ngOnInit(): void {

  }

  sendMessage() {
    this.loading = true;
    this.messageService.sendMessage(this.username, this.messageContent).then(() =>
      this.messageForm?.reset())
      .finally(() => this.loading = false);
  }
}
