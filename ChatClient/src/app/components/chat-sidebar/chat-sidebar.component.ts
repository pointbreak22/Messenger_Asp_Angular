import {Component, inject, OnInit} from '@angular/core';
import {MatMenuModule} from '@angular/material/menu';
import {MatIconModule} from '@angular/material/icon';
import {MatIconButton} from '@angular/material/button';
import {NgForOf, NgIf, TitleCasePipe} from '@angular/common';
import {AuthService} from '../../services/auth.service';
import {Router} from '@angular/router';
import {ChatService} from '../../services/chat.service';
import {User} from '../../models/user';
import {TypingIndicatorComponent} from '../typing-indicator/typing-indicator.component';

@Component({
  selector: 'app-chat-sidebar',
  standalone: true,
  imports: [

    MatIconModule,
    MatMenuModule,
    MatIconButton,
    TitleCasePipe,
    NgIf,
    NgForOf,
    TypingIndicatorComponent,


  ],
  templateUrl: './chat-sidebar.component.html',
  styles: ``
})
export class ChatSidebarComponent implements OnInit {
  trackByIndex: any;
  authService = inject(AuthService);
  chatService = inject(ChatService);
  router = inject(Router);

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
    this.chatService.disConnectConnection();

  }

  ngOnInit() {
    this.chatService.startConnection(this.authService.getAccessToken!)
  }

  openChatWindow(user: User) {
    this.chatService.currentOpenedChat.set(user);
    this.chatService.loadMessages(1);
  }
}
