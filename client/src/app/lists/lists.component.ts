import { Component } from '@angular/core';
import { Member } from '../_model/member';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent {
  members: Partial<Member[]> = [];
  predicate = 'liked'

  constructor(
    private memberService: MembersService,
  ) { }

  loadLikes() {
    this.memberService.getLikes(this.predicate).subscribe({
      next: resp => this.members = resp
    })
  }
}
