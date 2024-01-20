import { Component, OnInit } from '@angular/core';
import { Observable, take } from 'rxjs';
import { Member } from 'src/app/_model/member';
import { Pagination } from 'src/app/_model/pagination';
import { User } from 'src/app/_model/user';
import { UserParams } from 'src/app/_model/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members: Member[] | undefined;
  pagination: Pagination | undefined;
  userParams: UserParams | undefined;
  user: User | null | undefined
  genderList = [{ value: 'male', display: 'Males' }, { value: 'female', display: 'Females' }]


  constructor(
    private memberService: MembersService,

  ) {
    this.userParams = this.memberService.getUserParams();
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    if (this.userParams) {
      this.memberService.getMembers(this.userParams).subscribe({
        next: response => {
          this.members = response.result;
          this.pagination = response.pagination;
        }
      });
    }
  }

  resetFilters() {
    if (this.user) {
      this.userParams = this.memberService.resetUserParams();
      this.loadMembers();
    }
  }

  pageChanged(event: any) {
    if (this.userParams) {
      this.userParams.pageNumber = event.page;
      this.memberService.setUserParams(this.userParams);
      this.loadMembers();
    }
  }
}
