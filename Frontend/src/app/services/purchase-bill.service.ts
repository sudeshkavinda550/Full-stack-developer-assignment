import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface PurchaseBillItem {
  Item: string;
  StandardCost: number;
  StandardPrice: number;
  Quantity: number;
  Discount: number;
  TotalCost: number;
  TotalSelling: number;
}

export interface PurchaseBill {
  BatchLocation: string;
  Items: PurchaseBillItem[];
  TotalItems: number;
  TotalQuantity: number;
  TotalCost: number;
  TotalSelling: number;
}

export interface LocationDetail {
  Location_Code: string;
  Location_Name: string;
}

@Injectable({
  providedIn: 'root'
})
export class PurchaseBillService {
  private apiUrl = 'http://localhost:5000/api/ExternalApi/POS_Api/Invoke';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  getLocationDetails(): Observable<LocationDetail[]> {
    const request = {
      API_Action: 'GetLocations',
      Device_Id: 'web-client',
      Sync_Time: new Date().toISOString(),
      Company_Code: 'COMP001',
      API_Body: {}
    };

    return this.http.post<any>(this.apiUrl, request, {
      headers: this.getHeaders()
    }).pipe(
      map(response => {
        if (response.success) {
          return response.data;
        }
        return [];
      })
    );
  }

  createPurchaseBill(purchaseBill: PurchaseBill): Observable<any> {
    const userId = localStorage.getItem('userId');
    
    const request = {
      API_Action: 'SavePurchaseBill',
      Device_Id: 'web-client',
      Sync_Time: new Date().toISOString(),
      Company_Code: 'COMP001',
      API_Body: {
        UserId: parseInt(userId || '0'),
        BatchLocation: purchaseBill.BatchLocation,
        Items: purchaseBill.Items
      }
    };

    return this.http.post<any>(this.apiUrl, request, {
      headers: this.getHeaders()
    });
  }

  getPurchaseBills(): Observable<PurchaseBill[]> {
    // This endpoint might need to be implemented in your backend
    // For now, returning empty array
    return new Observable(observer => {
      observer.next([]);
      observer.complete();
    });
  }
}