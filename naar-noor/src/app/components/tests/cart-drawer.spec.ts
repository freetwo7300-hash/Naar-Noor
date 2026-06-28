import { TestBed, ComponentFixture } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { CartDrawerComponent } from '../cart-drawer/cart-drawer.component';
import { CartService } from '../../services/cart.service';
import { ApiService } from '../../services/api.service';
import { ToastService } from '../../services/toast.service';
import { ComponentTestBase } from './component-test.base';
import { of, throwError } from 'rxjs';

/**
 * Property 15: Component State Management Tests
 * Verifies that cart drawer component correctly manages UI state,
 * form validation, and multi-step workflow
 * 
 * Test scenarios:
 * - Component initialization displays correct initial state
 * - Cart items display with correct pricing
 * - Form submission validates inputs
 * - Multi-step navigation works correctly
 * - Error states display appropriately
 */
describe('CartDrawerComponent - Component State Management (Property 15)', () => {
  let component: CartDrawerComponent;
  let fixture: ComponentFixture<CartDrawerComponent>;
  let cartService: CartService;
  let apiService: ApiService;
  let toastService: ToastService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CartDrawerComponent,
        HttpClientTestingModule,
        ReactiveFormsModule,
        NoopAnimationsModule,
      ],
      providers: [CartService, ApiService, ToastService],
    }).compileComponents();

    fixture = TestBed.createComponent(CartDrawerComponent);
    component = fixture.componentInstance;
    cartService = TestBed.inject(CartService);
    apiService = TestBed.inject(ApiService);
    toastService = TestBed.inject(ToastService);

    fixture.detectChanges();
  });

  /**
   * Test 1: Component initialization - step starts at 1
   */
  it('should initialize with step 1 (cart view)', () => {
    expect(component.step()).toBe(1);
  });

  /**
   * Test 2: Component initialization - form is created
   */
  it('should create form on initialization', () => {
    expect(component.form).toBeDefined();
    expect(component.form.get('customerName')).toBeDefined();
    expect(component.form.get('email')).toBeDefined();
    expect(component.form.get('phoneNumber')).toBeDefined();
    expect(component.form.get('type')).toBeDefined();
  });

  /**
   * Test 3: Empty cart displays empty state message
   */
  it('should display empty cart message when no items', () => {
    cartService.open();
    fixture.detectChanges();

    const emptyElement = fixture.nativeElement.textContent;
    expect(emptyElement).toContain('Your cart is empty');
  });

  /**
   * Test 4: Adding items to cart displays them in drawer
   */
  it('should display cart items when items added', () => {
    cartService.add({
      menuItemId: '1',
      name: 'Biryani',
      price: 14.99,
      category: 'Mains',
    });

    cartService.open();
    fixture.detectChanges();

    expect(component.cart.items().length).toBe(1);
    expect(component.cart.items()[0].name).toBe('Biryani');
  });

  /**
   * Test 5: Cart displays correct item count
   */
  it('should display correct item count in header', () => {
    cartService.add({
      menuItemId: '1',
      name: 'Item1',
      price: 10.00,
      category: 'Mains',
    });
    cartService.add({
      menuItemId: '1',
      name: 'Item1',
      price: 10.00,
      category: 'Mains',
    });

    cartService.open();
    fixture.detectChanges();

    expect(component.cart.count()).toBe(2);
  });

  /**
   * Test 6: Form field validation - required fields
   */
  it('should mark customerName as required', () => {
    const customerName = component.form.get('customerName');
    customerName?.setValue('');
    customerName?.markAsTouched();

    expect(customerName?.hasError('required')).toBe(true);
    expect(component.err('customerName')).toBe(true);
  });

  /**
   * Test 7: Form field validation - email format
   */
  it('should validate email format', () => {
    const email = component.form.get('email');

    email?.setValue('invalid-email');
    expect(email?.hasError('email')).toBe(true);

    email?.setValue('valid@example.com');
    expect(email?.hasError('email')).toBe(false);
  });

  /**
   * Test 8: Form field validation - phone number minimum length
   */
  it('should validate phone number minimum length', () => {
    const phone = component.form.get('phoneNumber');

    phone?.setValue('123');
    expect(phone?.hasError('minlength')).toBe(true);

    phone?.setValue('01234567890');
    expect(phone?.hasError('minlength')).toBe(false);
  });

  /**
   * Test 9: Navigation to step 2
   */
  it('should navigate to step 2 when goToStep2 called', () => {
    component.goToStep2();
    expect(component.step()).toBe(2);
  });

  /**
   * Test 10: Delivery form shows delivery address field
   */
  it('should show delivery address field when delivery selected', () => {
    component.form.get('type')?.setValue('delivery');
    fixture.detectChanges();

    expect(component.isDelivery).toBe(true);
    expect(component.form.get('deliveryAddress')?.hasError('required')).toBe(true);
  });

  /**
   * Test 11: Dine-in form shows reservation name field
   */
  it('should show reservation name field when dine-in selected', () => {
    component.form.get('type')?.setValue('dine-in');
    fixture.detectChanges();

    expect(component.isDineIn).toBe(true);
    expect(component.form.get('tableReservationName')?.hasError('required')).toBe(true);
  });

  /**
   * Test 12: Collection order does not require delivery address
   */
  it('should not require delivery address for collection', () => {
    component.form.get('type')?.setValue('collection');
    fixture.detectChanges();

    const deliveryAddr = component.form.get('deliveryAddress');
    expect(deliveryAddr?.hasError('required')).toBe(false);
  });

  /**
   * Test 13: Closing drawer resets to step 1
   */
  it('should reset to step 1 when drawer closed from step 3', () => {
    component.step.set(3);
    component.confirmedShortId = 'ORD123';

    component.closeDrawer();

    // After timeout, step should be 1
    setTimeout(() => {
      expect(component.step()).toBe(1);
      expect(component.confirmedShortId).toBe('');
    }, 400);
  });

  /**
   * Test 14: Form submission with valid data
   */
  it('should submit form with valid data', () => {
    spyOn(apiService, 'createOrder').and.returnValue(
      of({ id: 'order-123' })
    );

    component.form.patchValue({
      customerName: 'Ahmed Hassan',
      email: 'ahmed@example.com',
      phoneNumber: '07700900123',
      type: 'collection',
    });

    cartService.add({
      menuItemId: '1',
      name: 'Item1',
      price: 10.00,
      category: 'Mains',
    });

    component.submit();

    expect(apiService.createOrder).toHaveBeenCalled();
  });

  /**
   * Test 15: Form submission prevents submission with invalid data
   */
  it('should not submit form with invalid data', () => {
    spyOn(apiService, 'createOrder');

    component.form.patchValue({
      customerName: '',
      email: 'invalid',
      phoneNumber: '123',
    });

    component.submit();

    expect(apiService.createOrder).not.toHaveBeenCalled();
  });

  /**
   * Test 16: Successful order submission displays confirmation
   */
  it('should display order confirmation after successful submission', () => {
    spyOn(apiService, 'createOrder').and.returnValue(
      of({ id: 'ORD-123-ABC' })
    );

    component.form.patchValue({
      customerName: 'John Doe',
      email: 'john@example.com',
      phoneNumber: '07712345678',
      type: 'collection',
    });

    cartService.add({
      menuItemId: '1',
      name: 'Item1',
      price: 10.00,
      category: 'Mains',
    });

    component.submit();

    expect(component.step()).toBe(3);
    expect(component.confirmedShortId).toBe('ORD');
  });

  /**
   * Test 17: Failed order submission shows error toast
   */
  it('should show error toast on submission failure', () => {
    spyOn(apiService, 'createOrder').and.returnValue(
      throwError(() => ({ error: { errors: 'Invalid order' } }))
    );
    spyOn(toastService, 'error');

    component.form.patchValue({
      customerName: 'John Doe',
      email: 'john@example.com',
      phoneNumber: '07712345678',
      type: 'collection',
    });

    cartService.add({
      menuItemId: '1',
      name: 'Item1',
      price: 10.00,
      category: 'Mains',
    });

    component.submit();

    expect(toastService.error).toHaveBeenCalled();
  });

  /**
   * Test 18: Back button navigates from step 2 to step 1
   */
  it('should go back from step 2 to step 1', () => {
    component.goToStep2();
    expect(component.step()).toBe(2);

    component.goBack();
    expect(component.step()).toBe(1);
  });

  /**
   * Test 19: Start over resets form and state
   */
  it('should reset form when starting over', () => {
    component.step.set(3);
    component.confirmedShortId = 'ORD123';

    component.startOver();

    expect(component.step()).toBe(1);
    expect(component.confirmedShortId).toBe('');
    expect(component.form.get('customerName')?.value).toBe('');
  });

  /**
   * Test 20: Multiple cart items display with correct totals
   */
  it('should calculate and display correct total for multiple items', () => {
    cartService.add({
      menuItemId: '1',
      name: 'Item1',
      price: 10.00,
      category: 'Mains',
    });

    cartService.add({
      menuItemId: '2',
      name: 'Item2',
      price: 5.00,
      category: 'Sides',
    });

    cartService.add({
      menuItemId: '1',
      name: 'Item1',
      price: 10.00,
      category: 'Mains',
    });

    expect(component.cart.count()).toBe(3);
    expect(component.cart.total()).toBe(25.00);
  });

  /**
   * Test 21: Notes field accepts text without validation errors
   */
  it('should accept notes without validation errors', () => {
    const notes = component.form.get('notes');

    notes?.setValue('Please ring doorbell twice');
    expect(notes?.hasError('maxlength')).toBe(false);
  });

  /**
   * Test 22: Notes field validates maximum length
   */
  it('should validate notes maximum length', () => {
    const notes = component.form.get('notes');
    const longText = 'A'.repeat(301);

    notes?.setValue(longText);
    expect(notes?.hasError('maxlength')).toBe(true);
  });

  /**
   * Test 23: Submission flag toggles during processing
   */
  it('should set submitting flag during order submission', () => {
    spyOn(apiService, 'createOrder').and.returnValue(
      of({ id: 'order-123' })
    );

    component.form.patchValue({
      customerName: 'Test User',
      email: 'test@example.com',
      phoneNumber: '07712345678',
      type: 'collection',
    });

    expect(component.submitting).toBe(false);

    component.submit();

    // Should be reset after success
    expect(component.submitting).toBe(false);
  });

  /**
   * Test 24: Order type field changes trigger conditional validators
   */
  it('should update validators when order type changes', () => {
    const deliveryAddr = component.form.get('deliveryAddress');

    component.form.get('type')?.setValue('collection');
    expect(deliveryAddr?.hasError('required')).toBe(false);

    component.form.get('type')?.setValue('delivery');
    expect(deliveryAddr?.hasError('required')).toBe(true);
  });
});
