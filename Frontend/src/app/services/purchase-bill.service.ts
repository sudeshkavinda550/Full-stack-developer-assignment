import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

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
  private apiUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  getLocationDetails(): Observable<LocationDetail[]> {
    return this.http.get<LocationDetail[]>(`${this.apiUrl}/LocationDetails`, {
      headers: this.getHeaders()
    });
  }

  createPurchaseBill(purchaseBill: PurchaseBill): Observable<any> {
    return this.http.post(`${this.apiUrl}/PurchaseBill`, purchaseBill, {
      headers: this.getHeaders()
    });
  }

  getPurchaseBills(): Observable<PurchaseBill[]> {
    return this.http.get<PurchaseBill[]>(`${this.apiUrl}/PurchaseBill`, {
      headers: this.getHeaders()
    });
  }
}