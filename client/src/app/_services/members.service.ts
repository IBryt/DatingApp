import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environment/environment';
import { Member } from '../_model/member';
import { of } from 'rxjs/internal/observable/of';
import { map } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  constructor(
    private http: HttpClient,
  ) { }

  getMembers() {
    if (this.members.length > 0) {
      return of(this.members)
    }
    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(members => this.members = members)
    )
  }

  getMember(username: string) {
    const member = this.members.find(m => m.userName === username)
    if (member) {
      return of(member);
    }
    return this.http.get<Member>(this.baseUrl + 'users/' + username)
  }

  updateMember(member: Member) {
    return this.http.put<void>(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    )
  }

  setMainPhoto(photoId: number) {
    return this.http.put<void>(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete<void>(this.baseUrl + 'users/delete-photo/' + photoId);
  }
}
