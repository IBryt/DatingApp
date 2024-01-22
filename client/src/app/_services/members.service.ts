import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environment/environment';
import { Member } from '../_model/member';
import { of } from 'rxjs/internal/observable/of';
import { map, take } from 'rxjs';
import { UserParams } from '../_model/userParams';
import { AccountService } from './account.service';
import { User } from '../_model/user';
import { getPaginationHeaders, getPaginationResult } from './pagination-helper';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCashe = new Map();
  user: User | null | undefined;
  userParams: UserParams | undefined;

  constructor(
    private http: HttpClient,
    private accountService: AccountService,
  ) {
    const user = this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        this.user = user;
        if (this.user) {
          this.userParams = new UserParams(this.user)
        }
      }
    });
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    if (this.user) {
      this.userParams = new UserParams(this.user);
    }
    return this.userParams;
  }

  getMembers(userParams: UserParams) {
    const key = Object.values(userParams).join('-');
    var response = this.memberCashe.get(key);

    if (response) {
      return of(response)
    }

    let params = getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    return getPaginationResult<Member[]>(this.baseUrl + 'users', params, this.http).pipe(
      map(res => {
        this.memberCashe.set(key, res);
        return res;
      })
    )
  }

  getMember(username: string) {
    const member: Member = [...this.memberCashe.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((m: Member) => m.userName === username);

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

  addLike(username: string) {
    return this.http.post(this.baseUrl + 'likes/' + username, {})
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('predicate', predicate);
    return getPaginationResult<Partial<Member[]>>(this.baseUrl + 'likes', params, this.http)
  }

  deletePhoto(photoId: number) {
    return this.http.delete<void>(this.baseUrl + 'users/delete-photo/' + photoId);
  }
}
