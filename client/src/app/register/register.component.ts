import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';


@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister: any = new EventEmitter();
  registerForm: FormGroup;
  maxDate: Date = new Date();
  validationErrors: string[] = [];

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.registerForm = this.initializeForm();
  }

  ngOnInit(): void {
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18)
  }

  initializeForm(): FormGroup {
    const fg = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(8)
      ]],
      confirmPassword: ['', [
        Validators.required,
        this.matchValues('password'),
      ]],
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
    this.accountService.register(this.registerForm.value).subscribe({
      next: user => {
        this.router.navigateByUrl('/members');
      },
      error: error => {
        this.validationErrors = error;
      }
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
