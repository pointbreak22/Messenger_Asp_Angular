import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { ApiResponse } from '../models/api-response';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  isLoading = signal(false);
  private baseUrl = '/api/account';
  private token = "token";
  // Внедрение через конструктор
  private httpClient = inject(HttpClient);

  get getAccessToken(): string | null {
    return localStorage.getItem(this.token) || '';

  }

  get currentLoggedUser(): User | null {
    const user: User = JSON.parse(localStorage.getItem('user') || '{}');
    return user;
  }

  register(data: FormData): Observable<ApiResponse<string>> {
    return this.httpClient.post<ApiResponse<string>>(
      `${this.baseUrl}/register`,
      data
    ).pipe(
      tap((response) => {
        localStorage.setItem(this.token, response.data);
      })
    );
  }

  login(email: string, password: string): Observable<ApiResponse<string>> {
    return this.httpClient.post<ApiResponse<string>>(`${this.baseUrl}/login`, {
      email,
      password
    }).pipe(tap((response) => {
      if (response.isSuccess) {
        localStorage.setItem(this.token, response.data);
      }
      return response;
    }))
  }

  me(): Observable<ApiResponse<User>> {
    return this.httpClient.get<ApiResponse<User>>(`${this.baseUrl}/me`,
      {
        headers: {
          Authorization: `Bearer ${this.getAccessToken}`
        }
      }
    ).pipe(tap((response) => {
      if (response.isSuccess) {
        localStorage.setItem("user", JSON.stringify(response.data));
      }

    }))
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem(this.token);
  }

  logout() {
    localStorage.removeItem(this.token);
    localStorage.removeItem('user')
  }
}
