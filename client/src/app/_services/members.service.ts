import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environment/environment';
import { Member } from '../_model/member';
import { of } from 'rxjs/internal/observable/of';
import { map } from 'rxjs';
import { PaginatedResult } from '../_model/pagination';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  paginatedResult: PaginatedResult<Member[]> = new PaginatedResult<Member[]>();

  constructor(
    private http: HttpClient,
  ) { }

  getMembers(page?: number, itemsPerPage?:number) {
    
    // if (this.members.length > 0) {
    //   return of(this.members)
    // }
    let params = new HttpParams();
    if(page != null && itemsPerPage != null){
      params = params.append('pageNumber', page.toString())
      params = params.append('pageSize', itemsPerPage.toString())
    }

    return this.http.get<Member[]>(this.baseUrl + 'users', {observe: 'response', params}).pipe(
      map(response => {
        this.paginatedResult.result = response.body ?? [];
        const pagination = response.headers.get('Pagination');
        if(pagination !== null) {
          this.paginatedResult.pagination = JSON.parse(pagination)
        }
        return this.paginatedResult;
      })
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
