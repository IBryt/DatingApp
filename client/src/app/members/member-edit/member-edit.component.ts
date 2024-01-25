import { Component, HostListener, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { Member } from 'src/app/_model/member';
import { User } from 'src/app/_model/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent {
  @ViewChild('editForm') editForm: NgForm | undefined;
  member: Member = <Member>{};
  user?: User = <User>{};
  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
    if (this.editForm?.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(
    private accountService: AccountService,
    private membersService: MembersService,
    private toastr: ToastrService,
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        this.user = user;
        this.loadMember();
      }
    })

  }

  loadMember() {
    if (this.user) {
      this.membersService.getMember(this.user.username).subscribe({
        next: member => this.member = member
      })
    }
  }

  updateMember() {
    this.membersService.updateMember(this.member).subscribe({
      next: _ => {
        this.toastr.success('Profile updated successfully');
        this.editForm?.reset(this.member)
      }
    })
  }
}
