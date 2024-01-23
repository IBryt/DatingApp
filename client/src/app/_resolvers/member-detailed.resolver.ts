import { ResolveFn } from '@angular/router';
import { Member } from '../_model/member';
import { MembersService } from '../_services/members.service';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';

export const memberDetailedResolver: ResolveFn<Observable<Member>> = (route) => {
  return inject(MembersService).getMember(route.paramMap.get('username')!);
};
