import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  @Input() usersFromHomeComponent: any;
  model: any = {};

  constructor() {
    console.log(this.usersFromHomeComponent);
  }
  register() {
    console.log(this.model);
  }
  cancel() {
    console.log('cancelled');
  }
}
