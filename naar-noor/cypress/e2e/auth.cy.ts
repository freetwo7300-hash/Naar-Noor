/// <reference types="cypress" />
import { LoginPage } from '../support/page-objects/LoginPage';

describe('Authentication E2E Tests', () => {
  beforeEach(() => {
    cy.visit('/');
  });

  describe('Login Workflow', () => {
    it('should display login page', () => {
      LoginPage.visit();
      cy.title().should('contain', 'Login');
    });

    it('should login with valid credentials', () => {
      LoginPage.visit();
      LoginPage.login('demo@example.com', 'password123');
      LoginPage.verifyLoggedIn();
    });

    it('should reject invalid credentials', () => {
      LoginPage.visit();
      LoginPage.login('invalid@example.com', 'wrongpassword');
      LoginPage.verifyErrorMessage('Invalid credentials');
    });

    it('should require email field', () => {
      LoginPage.visit();
      LoginPage.enterPassword('password123');
      cy.get('button[type="submit"]').click();
      cy.contains('Email is required').should('be.visible');
    });

    it('should require password field', () => {
      LoginPage.visit();
      LoginPage.enterEmail('test@example.com');
      cy.get('button[type="submit"]').click();
      cy.contains('Password is required').should('be.visible');
    });

    it('should validate email format', () => {
      LoginPage.visit();
      LoginPage.enterEmail('invalid-email');
      cy.contains('Invalid email format').should('be.visible');
    });
  });

  describe('Logout Workflow', () => {
    beforeEach(() => {
      LoginPage.visit();
      LoginPage.login('demo@example.com', 'password123');
      LoginPage.verifyLoggedIn();
    });

    it('should logout successfully', () => {
      LoginPage.clickLogout();
      LoginPage.verifyLoggedOut();
    });

    it('should redirect to login after logout', () => {
      LoginPage.clickLogout();
      cy.url().should('include', '/login');
    });
  });

  describe('Session Management', () => {
    it('should persist session on page refresh', () => {
      LoginPage.visit();
      LoginPage.login('demo@example.com', 'password123');
      LoginPage.verifyLoggedIn();

      cy.reload();

      LoginPage.verifyLoggedIn();
    });

    it('should persist session on navigation', () => {
      LoginPage.visit();
      LoginPage.login('demo@example.com', 'password123');

      cy.visit('/menu');

      LoginPage.verifyLoggedIn();
    });

    it('should clear session after logout', () => {
      LoginPage.visit();
      LoginPage.login('demo@example.com', 'password123');
      LoginPage.clickLogout();

      cy.reload();

      LoginPage.verifyLoggedOut();
    });
  });
});
