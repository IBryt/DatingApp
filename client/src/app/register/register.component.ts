import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  @Output() cancelRegister: any = new EventEmitter();
  model: any = {};

  constructor(
    private accountService: AccountService,
  ) { }

  register() {
    this.accountService.register(this.model).subscribe({
      next: user => {
        console.log(user);
        this.cancel();
        return user;
      },
      error: error => console.log(error)
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
