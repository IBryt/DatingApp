import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { AbstractControl, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';


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
      username: new FormControl('', Validators.required),
      password: new FormControl('', [
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(8)
      ]),
      confirmPassword: new FormControl('', [
        Validators.required,
        this.matchValues('password'),
      ]),
    });

    fg.controls.password.valueChanges.subscribe({
      next: _ => fg.controls.confirmPassword.updateValueAndValidity(),
    })

    return fg;
  }

  matchValues(matchTo: any): ValidatorFn {
    return (control: AbstractControl) => {
      const matchingControl = control.parent?.get(matchTo) as AbstractControl;
      return control.value === matchingControl?.value ? null : { isMatching: true };
    };
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
