/// <reference types="cypress" />

describe('Reviews E2E Tests', () => {
  beforeEach(() => {
    cy.visit('/');
  });

  describe('Review Display', () => {
    it('should display reviews section', () => {
      cy.get('[data-cy="reviews-section"]').should('exist');
    });

    it('should display review cards', () => {
      cy.get('[data-cy="review-card"]').should('have.length.greaterThan', 0);
    });

    it('should display customer name', () => {
      cy.get('[data-cy="review-name"]').first().should('contain.text', '');
    });

    it('should display star rating', () => {
      cy.get('[data-cy="review-rating"]').first().should('exist');
    });

    it('should display review text', () => {
      cy.get('[data-cy="review-text"]').first().should('exist');
    });

    it('should display review date', () => {
      cy.get('[data-cy="review-date"]').first().should('exist');
    });
  });

  describe('Review Submission', () => {
    beforeEach(() => {
      cy.get('[data-cy="review-form"]').should('exist');
    });

    it('should display review form', () => {
      cy.get('[data-cy="review-form"]').should('be.visible');
    });

    it('should have name input', () => {
      cy.get('input[name="reviewerName"]').should('exist');
    });

    it('should have rating selector', () => {
      cy.get('[data-cy="rating-selector"]').should('exist');
    });

    it('should have comment textarea', () => {
      cy.get('textarea[name="comment"]').should('exist');
    });

    it('should have submit button', () => {
      cy.get('button').contains('Submit Review').should('exist');
    });
  });

  describe('Review Validation', () => {
    it('should require name field', () => {
      cy.get('textarea[name="comment"]').type('Great food!');
      cy.get('button').contains('Submit Review').click();
      cy.contains('Name is required').should('be.visible');
    });

    it('should require comment field', () => {
      cy.get('input[name="reviewerName"]').type('John Doe');
      cy.get('button').contains('Submit Review').click();
      cy.contains('Comment is required').should('be.visible');
    });

    it('should require rating selection', () => {
      cy.get('input[name="reviewerName"]').type('John Doe');
      cy.get('textarea[name="comment"]').type('Great food!');
      cy.get('button').contains('Submit Review').click();
      cy.contains('Please select a rating').should('be.visible');
    });

    it('should validate comment length', () => {
      cy.get('input[name="reviewerName"]').type('John Doe');
      cy.get('textarea[name="comment"]').type('a');
      cy.get('button').contains('Submit Review').click();
      cy.contains('Comment must be at least 10 characters').should('be.visible');
    });

    it('should validate maximum comment length', () => {
      cy.get('input[name="reviewerName"]').type('John Doe');
      const longText = 'a'.repeat(1001);
      cy.get('textarea[name="comment"]').type(longText);
      cy.contains('Comment must not exceed 1000 characters').should('be.visible');
    });
  });

  describe('Review Submission Success', () => {
    it('should submit review with valid data', () => {
      cy.get('input[name="reviewerName"]').type('Ahmed Hassan');
      cy.get('[data-cy="rating-selector"]').find('button').eq(4).click();
      cy.get('textarea[name="comment"]').type('Excellent food and service!');
      cy.get('button').contains('Submit Review').click();
      cy.contains('Review submitted successfully').should('be.visible');
    });

    it('should show confirmation message', () => {
      cy.get('input[name="reviewerName"]').type('Sara Ali');
      cy.get('[data-cy="rating-selector"]').find('button').eq(4).click();
      cy.get('textarea[name="comment"]').type('Amazing experience at Naar & Noor!');
      cy.get('button').contains('Submit Review').click();
      cy.get('[data-cy="success-message"]').should('be.visible');
    });

    it('should add review to list', () => {
      const initialCount = cy.get('[data-cy="review-card"]').then($reviews => $reviews.length);
      cy.get('input[name="reviewerName"]').type('Fatima Khan');
      cy.get('[data-cy="rating-selector"]').find('button').eq(4).click();
      cy.get('textarea[name="comment"]').type('Fantastic service and delicious food!');
      cy.get('button').contains('Submit Review').click();
      cy.get('[data-cy="review-card"]').then($reviews => {
        expect($reviews.length).to.be.greaterThan(initialCount);
      });
    });

    it('should clear form after submission', () => {
      cy.get('input[name="reviewerName"]').type('Test User');
      cy.get('[data-cy="rating-selector"]').find('button').eq(3).click();
      cy.get('textarea[name="comment"]').type('Good food and nice ambiance!');
      cy.get('button').contains('Submit Review').click();
      cy.get('input[name="reviewerName"]').should('have.value', '');
      cy.get('textarea[name="comment"]').should('have.value', '');
    });
  });

  describe('Rating Stars', () => {
    it('should display 5 star options', () => {
      cy.get('[data-cy="rating-selector"]').find('button').should('have.length', 5);
    });

    it('should highlight selected star', () => {
      cy.get('[data-cy="rating-selector"]').find('button').eq(2).click();
      cy.get('[data-cy="rating-selector"]').find('button').eq(2).should('have.class', 'selected');
    });

    it('should allow changing rating', () => {
      cy.get('[data-cy="rating-selector"]').find('button').eq(2).click();
      cy.get('[data-cy="rating-selector"]').find('button').eq(4).click();
      cy.get('[data-cy="rating-selector"]').find('button').eq(4).should('have.class', 'selected');
    });

    it('should show star count', () => {
      cy.get('[data-cy="rating-display"]').first().should('contain', '5');
    });
  });

  describe('Review Pagination', () => {
    it('should paginate reviews if many exist', () => {
      cy.get('[data-cy="review-pagination"]').then($pagination => {
        if ($pagination.length > 0) {
          cy.get('[data-cy="pagination-button"]').should('have.length.greaterThan', 1);
        }
      });
    });

    it('should navigate to next page', () => {
      cy.get('[data-cy="pagination-button"]').contains('Next').click();
      cy.get('[data-cy="review-card"]').should('have.length.greaterThan', 0);
    });

    it('should navigate to previous page', () => {
      cy.get('[data-cy="pagination-button"]').contains('Next').click();
      cy.get('[data-cy="pagination-button"]').contains('Previous').click();
      cy.get('[data-cy="review-card"]').should('have.length.greaterThan', 0);
    });
  });
});
