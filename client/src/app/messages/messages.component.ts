import { Component, OnInit } from '@angular/core';
import { Message } from '../_model/message';
import { Pagination } from '../_model/pagination';
import { MessageService } from '../_services/message.service';
import { findIndex } from 'rxjs';
import { ConfirmService } from '../_services/confirm.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[] = [];
  pagination: Pagination | undefined;
  container = 'Unread';
  pageNumber = 1;
  pageSize = 5;
  loading = false;

  constructor(
    private messageService: MessageService,
    private confirmService: ConfirmService,
  ) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.loading = true;
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe({
      next: (resp: any) => {
        this.messages = resp.result;
        this.pagination = resp.pagination;
        this.loading = false;
      }
    })
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }

  deleteMessage(id: number) {
    this.confirmService.confirm('Confirm  delete message', 'This cannot be undone').subscribe({
      next: result => {
        if (result) {
          this.messageService.deleteMessage(id).subscribe({
            next: _ => this.messages.splice(this.messages.findIndex(m => m.id === id), 1)
          })
        }
      }
    });
  }
}
