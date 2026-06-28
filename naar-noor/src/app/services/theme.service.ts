import { Injectable, signal, computed, effect } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _isDark = signal<boolean>(this.loadTheme());

  readonly isDark = computed(() => this._isDark());

  constructor() {
    // Apply theme immediately and on every change
    effect(() => {
      const dark = this._isDark();
      const html = document.documentElement;
      if (dark) {
        html.classList.remove('theme-light');
        html.classList.add('theme-dark');
      } else {
        html.classList.remove('theme-dark');
        html.classList.add('theme-light');
      }
      localStorage.setItem('nn_theme', dark ? 'dark' : 'light');
    });
  }

  toggle(): void {
    this._isDark.update(v => !v);
  }

  private loadTheme(): boolean {
    try {
      const stored = localStorage.getItem('nn_theme');
      if (stored) return stored === 'dark';
    } catch { /* ignore */ }
    return true; // default: dark
  }
}
