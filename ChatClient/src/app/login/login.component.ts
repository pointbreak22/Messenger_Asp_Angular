import {Component, inject, signal} from '@angular/core';
import {MatInputModule} from '@angular/material/input';
import {MatIconModule} from '@angular/material/icon';
import {FormsModule} from '@angular/forms';
import {AuthService} from '../services/auth.service';
import {MatSnackBar} from '@angular/material/snack-bar';
import {HttpErrorResponse} from '@angular/common/http';
import {ApiResponse} from '../models/api-response';
import {Router, RouterLink} from '@angular/router';
import {NgIf} from '@angular/common';
import {MatButtonModule} from '@angular/material/button';
import {ButtonComponent} from '../components/button/button.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [MatInputModule,
    MatIconModule,
    FormsModule, RouterLink, NgIf, MatButtonModule, ButtonComponent,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  email!: string;

  password!: string;
  hide = signal(false);
  authService = inject(AuthService)
  private snackBar = inject(MatSnackBar);
  private router = inject(Router)

  login() {
    this.authService.isLoading.set(true);
    this.authService.login(this.email, this.password).subscribe(
      {
        next: () => {
          this.authService.me().subscribe();
          this.snackBar.open("Logged in successfully", 'Close', {
            duration: 500,
          });
          this.authService.isLoading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          let error = err.error as ApiResponse<string>;

          this.snackBar.open(error.error, "Close",
            {
              duration: 3000
            });
          this.authService.isLoading.set(false);
        },
        complete: () => {
          this.router.navigate(['/']);

          this.authService.isLoading.set(false);
        }

      });
  }

  togglePassword(event: MouseEvent) {
    this.hide.set(!this.hide());
    event.stopPropagation();
  }
}
