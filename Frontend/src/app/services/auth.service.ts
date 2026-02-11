import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginResponse {
  success: boolean;
  token: string;
  user: {
    Id: number;
    Username: string;
  };
  User_Locations: Array<{
    Location_Code: string;
    Location_Name: string;
  }>;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://cs.starline-api.azurewebsites.net/api/External_Api/POS_Api/Invoke';
  private localApiUrl = 'http://localhost:5000/api/ExternalApi/POS_Api/Invoke';
  
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    const userData = localStorage.getItem('currentUser');
    if (userData) {
      this.currentUserSubject.next(JSON.parse(userData));
    }
  }

  login(username: string, password: string): Observable<LoginResponse> {
    const loginData = {
      "API_Action": "GetLoginData",
      "Device_Id": "DD01",
      "Sync_Time": "",
      "Company_Code": "info@enhanzer.com",
      "API_Body": {
        "Username": username,
        "Pw": password
      }
    };

    return this.http.post<LoginResponse>(this.localApiUrl, loginData).pipe(
      tap(response => {
        if (response.success) {
          localStorage.setItem('token', response.token);
          localStorage.setItem('currentUser', JSON.stringify(response.user));
          localStorage.setItem('userLocations', JSON.stringify(response.User_Locations));
          this.currentUserSubject.next(response.user);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    localStorage.removeItem('userLocations');
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getUserLocations(): any[] {
    const locations = localStorage.getItem('userLocations');
    return locations ? JSON.parse(locations) : [];
  }
}