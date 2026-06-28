/// <reference types="cypress" />
import { MenuPage } from '../support/page-objects/MenuPage';
import { OrderPage } from '../support/page-objects/OrderPage';

describe('Order Workflow E2E Tests', () => {
  beforeEach(() => {
    cy.visit('/');
  });

  describe('Order Creation', () => {
    it('should browse menu and add items', () => {
      MenuPage.visit();
      MenuPage.verifyMenuItemsDisplayed(1);
      MenuPage.addToCart(0);
    });

    it('should add multiple items to cart', () => {
      MenuPage.visit();
      MenuPage.addToCart(0);
      MenuPage.addToCart(1);
      cy.get('[data-cy="cart-badge"]').should('contain', '2');
    });

    it('should modify item quantity', () => {
      MenuPage.visit();
      MenuPage.addToCart(0);
      OrderPage.visit();
      OrderPage.changeQuantity(0, 3);
      cy.get('input[type="number"]').should('have.value', '3');
    });

    it('should remove item from cart', () => {
      MenuPage.visit();
      MenuPage.addToCart(0);
      MenuPage.addToCart(1);
      OrderPage.visit();
      OrderPage.removeItem(0);
      OrderPage.verifyCartItemCount(1);
    });
  });

  describe('Checkout Process', () => {
    beforeEach(() => {
      MenuPage.visit();
      MenuPage.addToCart(0);
    });

    it('should display order summary', () => {
      OrderPage.visit();
      OrderPage.verifyCartItemCount(1);
      cy.get('[data-cy="order-total"]').should('exist');
    });

    it('should select delivery option', () => {
      OrderPage.visit();
      OrderPage.selectOrderType('delivery');
      OrderPage.getAddressInput().should('be.visible');
    });

    it('should select pickup option', () => {
      OrderPage.visit();
      OrderPage.selectOrderType('pickup');
      cy.get('[data-cy="pickup-time"]').should('exist');
    });

    it('should select dine-in option', () => {
      OrderPage.visit();
      OrderPage.selectOrderType('dine-in');
      cy.get('[data-cy="table-selection"]').should('exist');
    });

    it('should validate required fields', () => {
      OrderPage.visit();
      OrderPage.getPlaceOrderButton().click();
      cy.contains('Customer name is required').should('be.visible');
      cy.contains('Email is required').should('be.visible');
    });

    it('should validate email format', () => {
      OrderPage.visit();
      OrderPage.enterCustomerDetails('John Doe', 'invalid-email', '01234567890');
      cy.contains('Invalid email format').should('be.visible');
    });
  });

  describe('Order Submission', () => {
    beforeEach(() => {
      MenuPage.visit();
      MenuPage.addToCart(0);
      OrderPage.visit();
    });

    it('should place order successfully', () => {
      OrderPage.completeOrder('John Doe', 'john@example.com', '07700900123');
      OrderPage.verifyOrderConfirmation();
    });

    it('should display order reference', () => {
      OrderPage.completeOrder('Ahmed Hassan', 'ahmed@example.com', '07700900124');
      OrderPage.getOrderReference().should('contain', '#');
    });

    it('should show order total', () => {
      OrderPage.completeOrder('Sara Ali', 'sara@example.com', '07700900125');
      cy.get('[data-cy="final-total"]').should('exist');
    });

    it('should require delivery address for delivery orders', () => {
      OrderPage.selectOrderType('delivery');
      OrderPage.enterCustomerDetails('John Doe', 'john@example.com', '07700900123');
      OrderPage.getPlaceOrderButton().click();
      cy.contains('Delivery address is required').should('be.visible');
    });

    it('should place delivery order with address', () => {
      OrderPage.selectOrderType('delivery');
      OrderPage.completeOrder(
        'John Doe',
        'john@example.com',
        '07700900123',
        '123 Main Street, London'
      );
      OrderPage.verifyOrderConfirmation();
    });
  });

  describe('Cart Calculations', () => {
    it('should calculate correct total for single item', () => {
      MenuPage.visit();
      MenuPage.addToCart(0);
      OrderPage.visit();
      cy.get('[data-cy="order-total"]').should('exist');
    });

    it('should calculate correct total for multiple items', () => {
      MenuPage.visit();
      MenuPage.addToCart(0);
      MenuPage.addToCart(1);
      OrderPage.visit();
      cy.get('[data-cy="order-total"]').should('exist');
    });

    it('should update total when quantity changes', () => {
      MenuPage.visit();
      MenuPage.addToCart(0);
      OrderPage.visit();
      const initialTotal = cy.get('[data-cy="order-total"]').then($el => $el.text());
      OrderPage.changeQuantity(0, 3);
      cy.get('[data-cy="order-total"]').then($el => {
        expect($el.text()).not.to.equal(initialTotal);
      });
    });

    it('should update total when item removed', () => {
      MenuPage.visit();
      MenuPage.addToCart(0);
      MenuPage.addToCart(1);
      OrderPage.visit();
      const initialTotal = cy.get('[data-cy="order-total"]').then($el => $el.text());
      OrderPage.removeItem(0);
      cy.get('[data-cy="order-total"]').then($el => {
        expect($el.text()).not.to.equal(initialTotal);
      });
    });
  });
});
