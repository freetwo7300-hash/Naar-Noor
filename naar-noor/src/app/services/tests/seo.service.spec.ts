/// <reference types="jasmine" />
import { TestBed } from '@angular/core/testing';
import { Title, Meta } from '@angular/platform-browser';
import { SeoService, SeoConfig } from '../seo.service';

/**
 * Property 14: SeoService — Meta-tag and Title Management
 *
 * Validates that SeoService correctly:
 * 1. Sets the document title (full title includes brand suffix)
 * 2. Updates meta description, og:title, og:description, og:image, og:type
 * 3. Updates Twitter card meta tags
 * 4. Handles missing optional fields with sensible defaults
 * 5. Sets canonical link element when canonicalUrl is provided
 * 6. Convenience methods (setHome, setCheckout, setOrderConfirmed) produce correct titles
 */
describe('SeoService — Meta-tag and Title Management (Property 14)', () => {
  let service: SeoService;
  let titleService: Title;
  let metaService: Meta;

  const BASE_TITLE = 'Naar & Noor';
  const DEFAULT_DESCRIPTION =
    'Authentic Himalayan cuisine with flame-grilled specialties. Reserve your table or order online.';
  const DEFAULT_IMAGE = 'https://www.naarnooor.com/assets/hero/hero.webp';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SeoService, Title, Meta],
    });
    service = TestBed.inject(SeoService);
    titleService = TestBed.inject(Title);
    metaService = TestBed.inject(Meta);
  });

  // ---------------------------------------------------------------------------
  // Title tests
  // ---------------------------------------------------------------------------

  it('should append brand suffix when title differs from base title', () => {
    service.set({ title: 'Menu' });

    expect(titleService.getTitle()).toBe('Menu | Naar & Noor');
  });

  it('should not duplicate brand name when title equals base title', () => {
    service.set({ title: BASE_TITLE });

    expect(titleService.getTitle()).toBe(BASE_TITLE);
  });

  it('should use custom title passed in config', () => {
    service.set({ title: 'Reservations' });

    expect(titleService.getTitle()).toBe('Reservations | Naar & Noor');
  });

  // ---------------------------------------------------------------------------
  // Meta description
  // ---------------------------------------------------------------------------

  it('should set meta description from config', () => {
    const description = 'Explore our authentic Himalayan menu.';
    service.set({ title: 'Menu', description });

    const tag = metaService.getTag('name="description"');
    expect(tag?.content).toBe(description);
  });

  it('should use default description when none is provided', () => {
    service.set({ title: 'Menu' });

    const tag = metaService.getTag('name="description"');
    expect(tag?.content).toBe(DEFAULT_DESCRIPTION);
  });

  // ---------------------------------------------------------------------------
  // Open Graph tags
  // ---------------------------------------------------------------------------

  it('should set og:title meta tag', () => {
    service.set({ title: 'About' });

    const tag = metaService.getTag('property="og:title"');
    expect(tag?.content).toBe('About | Naar & Noor');
  });

  it('should set og:description meta tag', () => {
    const description = 'Learn about our heritage.';
    service.set({ title: 'About', description });

    const tag = metaService.getTag('property="og:description"');
    expect(tag?.content).toBe(description);
  });

  it('should set og:image from config', () => {
    const ogImage = 'https://example.com/custom-image.jpg';
    service.set({ title: 'Menu', ogImage });

    const tag = metaService.getTag('property="og:image"');
    expect(tag?.content).toBe(ogImage);
  });

  it('should use default og:image when none provided', () => {
    service.set({ title: 'Menu' });

    const tag = metaService.getTag('property="og:image"');
    expect(tag?.content).toBe(DEFAULT_IMAGE);
  });

  it('should set og:type from config', () => {
    service.set({ title: 'Menu', ogType: 'restaurant' });

    const tag = metaService.getTag('property="og:type"');
    expect(tag?.content).toBe('restaurant');
  });

  it('should default og:type to "website" when not provided', () => {
    service.set({ title: 'Menu' });

    const tag = metaService.getTag('property="og:type"');
    expect(tag?.content).toBe('website');
  });

  // ---------------------------------------------------------------------------
  // Twitter card tags
  // ---------------------------------------------------------------------------

  it('should set twitter:title meta tag', () => {
    service.set({ title: 'Contact' });

    const tag = metaService.getTag('name="twitter:title"');
    expect(tag?.content).toBe('Contact | Naar & Noor');
  });

  it('should set twitter:description meta tag', () => {
    const description = 'Get in touch with us.';
    service.set({ title: 'Contact', description });

    const tag = metaService.getTag('name="twitter:description"');
    expect(tag?.content).toBe(description);
  });

  it('should set twitter:image meta tag', () => {
    const ogImage = 'https://example.com/twitter.jpg';
    service.set({ title: 'Contact', ogImage });

    const tag = metaService.getTag('name="twitter:image"');
    expect(tag?.content).toBe(ogImage);
  });

  // ---------------------------------------------------------------------------
  // Canonical URL
  // ---------------------------------------------------------------------------

  it('should set canonical link element when canonicalUrl is provided', () => {
    const canonicalUrl = 'https://www.naarnooor.com/menu';
    service.set({ title: 'Menu', canonicalUrl });

    const link = document.querySelector<HTMLLinkElement>('link[rel="canonical"]');
    expect(link).not.toBeNull();
    expect(link?.href).toBe(canonicalUrl);
  });

  it('should reuse existing canonical link element on multiple calls', () => {
    service.set({ title: 'Menu', canonicalUrl: 'https://www.naarnooor.com/menu' });
    service.set({ title: 'About', canonicalUrl: 'https://www.naarnooor.com/about' });

    const links = document.querySelectorAll('link[rel="canonical"]');
    expect(links.length).toBe(1);
    expect((links[0] as HTMLLinkElement).href).toBe('https://www.naarnooor.com/about');
  });

  it('should not create canonical link when canonicalUrl is absent', () => {
    // Remove any existing canonical from other tests
    document.querySelector('link[rel="canonical"]')?.remove();

    service.set({ title: 'Menu' });

    const link = document.querySelector('link[rel="canonical"]');
    expect(link).toBeNull();
  });

  // ---------------------------------------------------------------------------
  // Convenience methods
  // ---------------------------------------------------------------------------

  it('setHome() should set a title that contains the brand name', () => {
    service.setHome();

    expect(titleService.getTitle()).toContain(BASE_TITLE);
  });

  it('setHome() should set og:type to "restaurant"', () => {
    service.setHome();

    const tag = metaService.getTag('property="og:type"');
    expect(tag?.content).toBe('restaurant');
  });

  it('setCheckout() should include "Checkout" in the page title', () => {
    service.setCheckout();

    expect(titleService.getTitle()).toContain('Checkout');
  });

  it('setOrderConfirmed() should include "Order Confirmed" in the page title', () => {
    service.setOrderConfirmed();

    expect(titleService.getTitle()).toContain('Order Confirmed');
  });

  // ---------------------------------------------------------------------------
  // Idempotency / overwrite
  // ---------------------------------------------------------------------------

  it('should overwrite previous meta tags on successive calls', () => {
    service.set({ title: 'Menu', description: 'First description' });
    service.set({ title: 'About', description: 'Second description' });

    expect(titleService.getTitle()).toBe('About | Naar & Noor');
    const descTag = metaService.getTag('name="description"');
    expect(descTag?.content).toBe('Second description');
  });

  it('should handle empty description by falling back to default', () => {
    service.set({ title: 'Menu', description: undefined });

    const tag = metaService.getTag('name="description"');
    expect(tag?.content).toBe(DEFAULT_DESCRIPTION);
  });
});
