import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ToastrModule } from 'ngx-toastr';
import { TabsModule } from 'ngx-bootstrap/tabs'
import { CarouselModule } from 'ngx-bootstrap/carousel';
import { NgxSpinnerModule } from "ngx-spinner";

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    BsDropdownModule.forRoot(),
    ToastrModule.forRoot({
      positionClass: 'toast-bottom-right'
    }),
    TabsModule.forRoot(),
    CarouselModule.forRoot(),
    NgxSpinnerModule,
  ],
  exports: [
    BsDropdownModule,
    ToastrModule,
    TabsModule,
    CarouselModule,
    NgxSpinnerModule,
  ]
})
export class SharedModule { }
