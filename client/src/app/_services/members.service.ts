import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environment/environment';
import { Member } from '../_model/member';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
  ) { }

  getMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'users')
  }

  getMember(username: string) {
    return this.http.get<Member>(this.baseUrl + 'users/' + username)
  }

  updateMember(member: Member) {
    return this.http.put<void>(this.baseUrl + 'users', member)
  }
}
