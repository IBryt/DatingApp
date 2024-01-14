import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environment/environment';
import { Member } from '../_model/member';
import { USER_KEY } from '../constants';
import { Observable } from 'rxjs';

const storedData = JSON.parse(localStorage.getItem(USER_KEY) || '{}');
const token = storedData.token || '';
const httpOptions = {
  headers: new HttpHeaders({
    Authorization: 'Bearer ' + token
  })
};
@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
  ) { }

  getMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'users', httpOptions)
  }

  getMember(username: string) {
    return this.http.get<Member>(this.baseUrl + 'users/' + username, httpOptions)
  }
}
