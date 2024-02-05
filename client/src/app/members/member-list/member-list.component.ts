import { Component, OnInit } from '@angular/core';
import { Member } from 'src/app/_model/member';
import { Pagination } from 'src/app/_model/pagination';
import { User } from 'src/app/_model/user';
import { UserParams } from 'src/app/_model/userParams';
import { MembersService } from 'src/app/_services/members.service';
import { PresenceService } from 'src/app/_services/presence.service';

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
    private presenceService: PresenceService,
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
          this.getOnlineUsers();
          this.pagination = response.pagination;
        }
      });
    }
  }

  getOnlineUsers() {
    if (!this.members) {
      return;
    }
    this.presenceService.getOnlineUsers(this.members.map(x => x.userName));
  };


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
