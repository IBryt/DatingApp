import { Component, OnInit } from '@angular/core';
import { Observable, take } from 'rxjs';
import { Member } from 'src/app/_model/member';
import { Pagination } from 'src/app/_model/pagination';
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


  constructor(
    private memberService: MembersService,
    private accountService: AccountService,
  ) {
    const user = this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) {
          this.userParams = new UserParams(user)
        }
      }
    });
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

  pageChanged(event: any) {
    if (this.userParams) {
      this.userParams.pageNumber = event.page;
      this.loadMembers();
    }
  }
}
