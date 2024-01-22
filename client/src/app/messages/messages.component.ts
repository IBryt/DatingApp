import { Component, OnInit } from '@angular/core';
import { Message } from '../_model/message';
import { Pagination } from '../_model/pagination';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit{
  messages: Message[] = [];
  pagination: Pagination | undefined;
  container = 'Outbox';
  pageNumber = 1;
  pageSize = 5;

  constructor(
    private messageService: MessageService,
  ) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe({
      next: (resp: any) => {
        this.messages = resp.result;
        this.pagination = resp.pagination;
      }
    })
  }

  pageChanged(event:any){
    if(this.pageNumber !== event.page){
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }
}
