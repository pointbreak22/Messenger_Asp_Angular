import {Component, inject, OnInit} from '@angular/core';
import {RouterOutlet} from '@angular/router';
import {MatSlideToggleModule} from '@angular/material/slide-toggle';
import {CommonModule} from '@angular/common';
import {VideoChatService} from './services/video-chat.service';
import {AuthService} from './services/auth.service';
import {MatDialog} from '@angular/material/dialog';
import {VideoChatComponent} from './components/video-chat/video-chat.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, MatSlideToggleModule, CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  // title = 'ChatClient';
  // ringtone: HTMLAudioElement | null = null;
  private signalRService = inject(VideoChatService);
  private authService = inject(AuthService);
  private dialog = inject(MatDialog);

  ngOnInit(): void {
    if (!this.authService.getAccessToken) return;
    this.signalRService.startConnection();
    this.startOfferReceive();

    // document.addEventListener('click', this.allowAudioPlayback, {once: true});
  }

  allowAudioPlayback = () => {
    // this.ringtone = new Audio('assets/phone-ring.mp3');
    //
    // // Разрешаем браузеру заранее проигрывать звук
    // if (this.ringtone) {
    //   this.ringtone.play().then(() => {
    //     this.ringtone!.currentTime = 0;
    //     console.log('🔓 Audio playback unlocked');
    //   }).catch((err) => {
    //     console.warn('⚠️ User interaction required to play audio', err);
    //   });
    // }
  };


  startOfferReceive() {
    this.signalRService.offerReceived.subscribe(async (data) => {
      if (data) {
        // if (this.ringtone) {
        //   try {
        //     await this.ringtone.play();
        //   } catch (err) {
        //     console.warn('⚠️ Failed to auto-play ringtone:', err);
        //   }
        // }


        let ringtone = new Audio('assets/phone-ring.mp3');
        ringtone.play();

        this.dialog.open(VideoChatComponent, {
          width: '400px',
          height: '600px',
          disableClose: false,
        });
        this.signalRService.remoteUserId = data.senderId;
        this.signalRService.incomingCall = true;
      }
    })
  }
}
