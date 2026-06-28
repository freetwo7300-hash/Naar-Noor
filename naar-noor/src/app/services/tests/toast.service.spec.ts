/// <reference types="jasmine" />
import { TestBed } from '@angular/core/testing';
import { ToastService } from '../toast.service';

/**
 * Property 14: Service State Consistency and Message Handling Tests
 * Verifies that toast notifications maintain state consistency
 * and handle multiple messages correctly
 * 
 * Test scenarios:
 * - Toast messages queue correctly
 * - Messages display in FIFO order
 * - Message removal works properly
 * - Success/error/info types differentiate
 * - Auto-dismiss timing is consistent
 */
describe('ToastService - State Consistency (Property 14)', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ToastService],
    });
    service = TestBed.inject(ToastService);
  });

  /**
   * Test 1: Create success toast
   */
  it('should create success toast message', () => {
    service.success('Operation completed successfully');

    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].type).toBe('success');
    expect(service.toasts()[0].message).toBe('Operation completed successfully');
  });

  /**
   * Test 2: Create error toast
   */
  it('should create error toast message', () => {
    service.error('An error occurred');

    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].type).toBe('error');
    expect(service.toasts()[0].message).toBe('An error occurred');
  });

  /**
   * Test 3: Create info toast
   */
  it('should create info toast message', () => {
    service.info('Information message');

    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].type).toBe('info');
    expect(service.toasts()[0].message).toBe('Information message');
  });

  /**
   * Test 4: Create warning toast
   */
  it('should create warning toast message', () => {
    service.warning('Warning: Action cannot be undone');

    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].type).toBe('warning');
    expect(service.toasts()[0].message).toBe('Warning: Action cannot be undone');
  });

  /**
   * Test 5: Multiple messages queue in order
   */
  it('should queue multiple messages in FIFO order', () => {
    service.success('First message');
    service.error('Second message');
    service.info('Third message');

    expect(service.toasts().length).toBe(3);
    expect(service.toasts()[0].message).toBe('First message');
    expect(service.toasts()[1].message).toBe('Second message');
    expect(service.toasts()[2].message).toBe('Third message');
  });

  /**
   * Test 6: Message ID is unique for each toast
   */
  it('should assign unique ID to each message', () => {
    service.success('Message 1');
    service.success('Message 2');

    expect(service.toasts().length).toBe(2);
    expect(service.toasts()[0].id).not.toBe(service.toasts()[1].id);
  });

  /**
   * Test 7: Dismiss method removes specific toast
   */
  it('should dismiss toast by ID', () => {
    service.success('Test message 1');
    service.success('Test message 2');

    expect(service.toasts().length).toBe(2);

    const firstId = service.toasts()[0].id;
    service.dismiss(firstId);

    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].id).not.toBe(firstId);
  });

  /**
   * Test 8: Success type uses success class
   */
  it('should have success type for success toast', () => {
    service.success('Success');

    expect(service.toasts()[0].type).toBe('success');
  });

  /**
   * Test 9: Error type uses error class
   */
  it('should have error type for error toast', () => {
    service.error('Error');

    expect(service.toasts()[0].type).toBe('error');
  });

  /**
   * Test 10: Info type uses info class
   */
  it('should have info type for info toast', () => {
    service.info('Info');

    expect(service.toasts()[0].type).toBe('info');
  });

  /**
   * Test 11: Warning type uses warning class
   */
  it('should have warning type for warning toast', () => {
    service.warning('Warning');

    expect(service.toasts()[0].type).toBe('warning');
  });

  /**
   * Test 12: Empty message text is handled
   */
  it('should handle empty message text', () => {
    service.success('');

    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].message).toBe('');
  });

  /**
   * Test 13: Long message text is preserved
   */
  it('should preserve long message text', () => {
    const longText = 'This is a very long toast message that contains multiple sentences. It should be fully preserved without truncation.';

    service.success(longText);

    expect(service.toasts()[0].message).toBe(longText);
  });

  /**
   * Test 14: Special characters in message are preserved
   */
  it('should preserve special characters in message', () => {
    const specialText = 'Order #123: €50.00 @ "The Restaurant" & \'More\'';

    service.success(specialText);

    expect(service.toasts()[0].message).toBe(specialText);
  });

  /**
   * Test 15: Auto-dismiss clears toast after timeout
   */
  it('should auto-dismiss toast after timeout', () => {
    jest.useFakeTimers();

    service.success('Temporary message'); // 7000ms timeout
    expect(service.toasts().length).toBe(1);

    jest.advanceTimersByTime(7100);
    expect(service.toasts().length).toBe(0);

    jest.useRealTimers();
  });
});
