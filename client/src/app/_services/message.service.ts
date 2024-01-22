import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environment/environment';
import { getPaginationHeaders, getPaginationResult } from './pagination-helper';
import { Message } from '../_model/message';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  constructor(
    private http: HttpClient,
  ) { }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginationResult<Message[]>(this.baseUrl + 'messages', params, this.http);
  }
}
