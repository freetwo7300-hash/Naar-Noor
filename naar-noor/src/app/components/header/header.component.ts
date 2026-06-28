import { Component, CUSTOM_ELEMENTS_SCHEMA, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { AuthModalComponent } from '../auth-modal/auth-modal.component';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, AuthModalComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {
  mobileMenuOpen = false;
  userMenuOpen   = signal(false);
  authModalOpen  = signal(false);

  readonly cart  = inject(CartService);
  readonly auth  = inject(AuthService);
  readonly theme = inject(ThemeService);
  private readonly router = inject(Router);

  get userInitial(): string {
    const email = this.auth.userEmail();
    return email ? email.charAt(0).toUpperCase() : '?';
  }

  toggleMobileMenu() {
    this.mobileMenuOpen = !this.mobileMenuOpen;
    if (this.mobileMenuOpen) this.userMenuOpen.set(false);
  }

  toggleUserMenu() {
    this.userMenuOpen.update(v => !v);
  }

  openAuthModal() {
    this.authModalOpen.set(true);
    this.mobileMenuOpen = false;
    this.userMenuOpen.set(false);
  }

  closeAuthModal() {
    this.authModalOpen.set(false);
  }

  logout() {
    this.auth.logout();
    this.userMenuOpen.set(false);
    this.router.navigate(['/']);
  }

  /** Close user dropdown when clicking outside */
  @HostListener('document:click', ['$event'])
  onDocumentClick(e: MouseEvent) {
    if (!(e.target as HTMLElement).closest('[data-user-menu]')) {
      this.userMenuOpen.set(false);
    }
  }
}
