import { Component, Input } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs';
import { Member } from 'src/app/_model/member';
import { User } from 'src/app/_model/user';
import { AccountService } from 'src/app/_services/account.service';
import { environment } from 'src/environment/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent {
  @Input() member: Member = <Member>{};
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl
  uploader: FileUploader;
  user: User | null = <User>{};

  constructor(
    private accountService: AccountService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => this.user = user
    })
    this.uploader = this.initializeUploader()
  }

  initializeUploader(): FileUploader {
    const uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: `Bearer ${this.user?.token}`,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024, // 10 megabites
    });

    uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false
    }

    uploader.onSuccessItem = (file, response, status, headers) => {
      if (response) {
        const photo = JSON.parse(response);
        this.member.photos.push(photo);
      }
    }

    return uploader;
  }

  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

}
