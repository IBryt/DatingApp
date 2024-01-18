import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { FormControl, FormGroup } from '@angular/forms';


@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  @Output() cancelRegister: any = new EventEmitter();
  model: any = {};
  registerForm: FormGroup;

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService,
  ) {
    this.registerForm = this.initializeForm();
  }

  initializeForm(): FormGroup {
    const fg = new FormGroup({
      username: new FormControl(),
      password: new FormControl(),
      confirmPassword: new FormControl(),
    });

    return fg;
  }

  register() {
    console.log(this.registerForm.value);
    // this.accountService.register(this.model).subscribe({
    //   next: user => {
    //     console.log(user);
    //     this.cancel();
    //     return user;
    //   },
    //   error: error => this.toastr.error(error.error)
    // });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
