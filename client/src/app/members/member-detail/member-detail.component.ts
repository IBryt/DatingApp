import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/_model/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  slideChangeMessage = '';
  slides: any = [];

  member: Member = <Member>{};

  constructor(
    private membersService: MembersService,
    private route: ActivatedRoute,
  ) { }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.membersService.getMember(this.route.snapshot.paramMap.get('username')!).subscribe({
      next: member => {
        this.member = member;
        for (const photo of this.member.photos) {
          this.slides.push({ image: photo.url })
        }
      },
    })
  }
}
