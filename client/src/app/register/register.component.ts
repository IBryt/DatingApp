import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  @Input() usersFromHomeComponent: any;
  @Output() cancelRegister: any = new EventEmitter();
  model: any = {};

  constructor() {
    console.log(this.usersFromHomeComponent);
  }
  register() {
    console.log(this.model);
  }
  cancel() {
    this.cancelRegister.emit(false);
  }
}
