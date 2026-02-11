import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { PurchaseBillService, PurchaseBillItem, LocationDetail } from '../services/purchase-bill.service';

@Component({
  selector: 'app-purchase-bill',
  templateUrl: './purchase-bill.component.html',
  styleUrls: ['./purchase-bill.component.css']
})
export class PurchaseBillComponent implements OnInit {
  items: string[] = ['Mango', 'Apple', 'Banana', 'Orange', 'Grapes', 'Kiwi', 'Strawberry'];
  locations: LocationDetail[] = [];
  userLocations: any[] = [];

  selectedItem: string = '';
  standardCost: number = 100;
  standardPrice: number = 150;
  quantity: number = 5;
  discount: number = 20;
  selectedBatch: string = '';

  billItems: PurchaseBillItem[] = [];
  totalItems: number = 0;
  totalQuantity: number = 0;

  successMessage: string = '';
  errorMessage: string = '';

  constructor(
    private authService: AuthService,
    private purchaseBillService: PurchaseBillService,
    private router: Router
  ) { }

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    this.userLocations = this.authService.getUserLocations();
    this.loadLocationDetails();
  }

  loadLocationDetails(): void {
    this.purchaseBillService.getLocationDetails().subscribe({
      next: (data) => {
        this.locations = data;
      },
      error: (error) => {
        console.error('Error loading locations:', error);
      }
    });
  }

  calculateTotalCost(): number {
    return (this.standardCost * this.quantity) - this.discount;
  }

  calculateTotalSelling(): number {
    return this.standardPrice * this.quantity;
  }

  addItem(): void {
    if (!this.selectedItem) {
      this.errorMessage = 'Please select an item';
      return;
    }

    const newItem: PurchaseBillItem = {
      Item: this.selectedItem,
      StandardCost: this.standardCost,
      StandardPrice: this.standardPrice,
      Quantity: this.quantity,
      Discount: this.discount,
      TotalCost: this.calculateTotalCost(),
      TotalSelling: this.calculateTotalSelling()
    };

    this.billItems.push(newItem);
    this.updateSummary();
    
    // Reset form
    this.selectedItem = '';
    this.standardCost = 100;
    this.standardPrice = 150;
    this.quantity = 5;
    this.discount = 20;
    this.errorMessage = '';
  }

  removeItem(index: number): void {
    this.billItems.splice(index, 1);
    this.updateSummary();
  }

  updateSummary(): void {
    this.totalItems = this.billItems.length;
    this.totalQuantity = this.billItems.reduce((sum, item) => sum + item.Quantity, 0);
  }

  getTotalCost(): number {
    return this.billItems.reduce((sum, item) => sum + item.TotalCost, 0);
  }

  getTotalSelling(): number {
    return this.billItems.reduce((sum, item) => sum + item.TotalSelling, 0);
  }

  submitBill(): void {
    if (this.billItems.length === 0) {
      this.errorMessage = 'Please add at least one item';
      return;
    }

    if (!this.selectedBatch) {
      this.errorMessage = 'Please select a batch location';
      return;
    }

    const purchaseBill = {
      BatchLocation: this.selectedBatch,
      Items: this.billItems,
      TotalItems: this.totalItems,
      TotalQuantity: this.totalQuantity,
      TotalCost: this.getTotalCost(),
      TotalSelling: this.getTotalSelling()
    };

    this.purchaseBillService.createPurchaseBill(purchaseBill).subscribe({
      next: (response) => {
        this.successMessage = 'Purchase bill created successfully!';
        this.billItems = [];
        this.updateSummary();
        this.selectedBatch = '';
        this.errorMessage = '';
        
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (error) => {
        this.errorMessage = 'Error creating purchase bill. Please try again.';
        console.error('Error:', error);
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}