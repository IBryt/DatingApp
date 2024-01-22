import { Component, OnInit } from '@angular/core';
import { Member } from '../_model/member';
import { MembersService } from '../_services/members.service';
import { PaginatedResult, Pagination } from '../_model/pagination';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  members: Partial<Member[]> = [];
  predicate = 'liked';
  pageNumber = 1;
  pageSize = 5;
  pagination: Pagination | undefined;

  constructor(
    private memberService: MembersService,
  ) { }

  ngOnInit(): void {
    this.loadLikes();
  }

  loadLikes() {
    this.memberService.getLikes(this.predicate, this.pageNumber, this.pageSize).subscribe({
      next: (resp: PaginatedResult<(Member | undefined)[]>) => {
        this.members = resp.result || [];
        this.pagination = resp.pagination;
      }
    });
  }

  pageChanged(event: any){
    this.pageNumber = event.page;
    this.loadLikes();
  }
}
