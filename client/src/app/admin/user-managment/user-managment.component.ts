import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { User } from 'src/app/_model/user';
import { AdminService } from 'src/app/_services/admin.service';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';

@Component({
  selector: 'app-user-managment',
  templateUrl: './user-managment.component.html',
  styleUrls: ['./user-managment.component.css']
})
export class UserManagmentComponent implements OnInit {
  users: Partial<User[]> = [];
  bsModalRef?: BsModalRef;

  constructor(
    private adminService: AdminService,
    private modalService: BsModalService,
  ) { }

  ngOnInit(): void {
    this.getUserWithRoles();
  }

  getUserWithRoles() {
    this.adminService.getUsersWithRoles().subscribe({
      next: resp => this.users = resp
    })
  }

  openRolesModal(user?: User) {
    if(!user){
      return;
    }

    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user,
        roles: this.getRoles(user)
      }
    };
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.content.updateSelectedRoles.subscribe({
      next: (values: any) => {
        const roleToUpdate = {
          roles: [...values.filter((v: any) => v.checked).map((v: any) => v.name)]
        }
        if(roleToUpdate) {
          this.adminService.updateUserRoles(user.username, roleToUpdate.roles).subscribe({
            next: _ =>{
              user.roles = [...roleToUpdate.roles];
            }
          })
        }
      }
    })
  }

  private getRoles(user?: User) {
    const roles: any[] = [];

    if (!user) {
      return roles;
    }

    const userRoles = user.roles;
    const availableRoles: any[] = [
      { name: 'Admin', value: 'Admin' },
      { name: 'Moderator', value: 'Moderator' },
      { name: 'Member', value: 'Member' }
    ]

    availableRoles.forEach(role => {
      let isMatch = false;
      for (const userRole of userRoles) {
        if (role.name === userRole) {
          isMatch = true;
          role.checked = true;
          roles.push(role);
          break;
        }
      }
      if (!isMatch) {
        role.checked = false;
        roles.push(role);
      }
    });

    return roles;
  }
}
