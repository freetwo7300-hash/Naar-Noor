/// <reference types="cypress" />
import { ReservationPage } from '../support/page-objects/ReservationPage';

describe('Reservation Workflow E2E Tests', () => {
  beforeEach(() => {
    cy.visit('/');
  });

  it('should display home page', () => {
    cy.title().should('contain', 'Naar & Noor');
  });

  it('should navigate to reservations page', () => {
    cy.contains('Reservations').click();
    cy.url().should('include', '/reservations');
  });

  it('should display available chefs', () => {
    cy.visit('/reservations');
    cy.get('[data-cy="chef-list"]').should('exist');
    cy.get('[data-cy="chef-card"]').should('have.length.greaterThan', 0);
  });

  it('should select a chef for reservation', () => {
    cy.visit('/reservations');
    cy.get('[data-cy="chef-card"]').first().click();
    cy.get('[data-cy="chef-details"]').should('exist');
  });

  it('should fill reservation form with valid data', () => {
    cy.visit('/reservations');
    cy.get('[data-cy="chef-card"]').first().click();
    
    cy.get('input[name="date"]').type('2026-07-15');
    cy.get('input[name="time"]').type('19:00');
    cy.get('input[name="guestCount"]').type('4');
    cy.get('input[name="specialRequests"]').type('Window seat preferred');
    
    cy.get('button[type="submit"]').should('be.enabled');
  });

  it('should submit reservation', () => {
    cy.visit('/reservations');
    cy.get('[data-cy="chef-card"]').first().click();
    
    cy.get('input[name="date"]').type('2026-07-15');
    cy.get('input[name="time"]').type('19:00');
    cy.get('input[name="guestCount"]').type('4');
    cy.get('button[type="submit"]').click();
    
    cy.get('[data-cy="reservation-confirmation"]').should('exist');
    cy.contains('Reservation confirmed').should('exist');
  });

  it('should display reservation number', () => {
    cy.visit('/reservations');
    cy.get('[data-cy="chef-card"]').first().click();
    
    cy.get('input[name="date"]').type('2026-07-15');
    cy.get('input[name="time"]').type('19:00');
    cy.get('input[name="guestCount"]').type('2');
    cy.get('button[type="submit"]').click();
    
    cy.get('[data-cy="confirmation-number"]').should('exist');
    cy.get('[data-cy="confirmation-number"]').should('contain.text', '#');
  });

  it('should validate required fields', () => {
    cy.visit('/reservations');
    cy.get('[data-cy="chef-card"]').first().click();
    
    cy.get('button[type="submit"]').click();
    cy.get('[data-cy="error-date"]').should('contain', 'Date is required');
    cy.get('[data-cy="error-time"]').should('contain', 'Time is required');
  });

  it('should reject past dates', () => {
    cy.visit('/reservations');
    cy.get('[data-cy="chef-card"]').first().click();
    
    cy.get('input[name="date"]').type('2024-01-01');
    cy.get('input[name="time"]').type('10:00');
    cy.get('[data-cy="error-date"]').should('contain', 'Date must be in future');
  });

  it('should validate guest count range', () => {
    cy.visit('/reservations');
    cy.get('[data-cy="chef-card"]').first().click();
    
    cy.get('input[name="guestCount"]').type('0');
    cy.get('[data-cy="error-guestCount"]').should('contain', 'At least 1 guest');
    
    cy.get('input[name="guestCount"]').clear().type('51');
    cy.get('[data-cy="error-guestCount"]').should('contain', 'Maximum 50 guests');
  });
});
