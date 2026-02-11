import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginRequest {
  Username: string;
  Pw: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: {
    Id: number;
    Username: string;
  };
  User_Locations?: any[];
  data?: any;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/ExternalApi/POS_Api/Invoke';

  constructor(private http: HttpClient) { }

  login(username: string, password: string): Observable<LoginResponse> {
    const request = {
      API_Action: 'GetLoginData',
      Device_Id: 'web-client',
      Sync_Time: new Date().toISOString(),
      Company_Code: 'COMP001',
      API_Body: {
        Username: username,
        Pw: password
      }
    };

    return this.http.post<LoginResponse>(this.apiUrl, request).pipe(
      tap(response => {
        if (response.success && response.token) {
          // Store authentication data
          localStorage.setItem('token', response.token);
          
          // Handle user ID - check both response.user and response.data
          const userId = response.user?.Id || response.data?.userId || 0;
          localStorage.setItem('userId', userId.toString());
          
          // Handle username
          const username = response.user?.Username || response.data?.username || '';
          localStorage.setItem('username', username);
          
          // Handle locations
          const locations = response.User_Locations || response.data?.locations || [];
          localStorage.setItem('userLocations', JSON.stringify(locations));
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('username');
    localStorage.removeItem('userLocations');
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('token');
  }

  getUserLocations(): any[] {
    const locations = localStorage.getItem('userLocations');
    return locations ? JSON.parse(locations) : [];
  }

  getUserId(): number {
    const userId = localStorage.getItem('userId');
    return userId ? parseInt(userId) : 0;
  }

  getUsername(): string {
    return localStorage.getItem('username') || '';
  }
}