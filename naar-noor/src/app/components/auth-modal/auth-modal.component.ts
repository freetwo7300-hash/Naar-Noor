import {
  Component, Input, Output, EventEmitter, inject, signal,
  OnChanges, SimpleChanges, HostListener, CUSTOM_ELEMENTS_SCHEMA,
  ElementRef, AfterViewChecked
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

const FOCUSABLE = 'button:not([disabled]),input:not([disabled]),select:not([disabled]),textarea:not([disabled]),[tabindex]:not([tabindex="-1"])';

@Component({
  selector: 'app-auth-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div *ngIf="open"
         class="fixed inset-0 z-[300] flex items-center justify-center p-4"
         role="dialog" aria-modal="true" aria-labelledby="auth-modal-title"
         (keydown)="onKeydown($event)">

      <!-- Backdrop -->
      <div class="absolute inset-0 bg-black/70 backdrop-blur-sm animate-fade-in"
           (click)="close()"></div>

      <!-- Modal card -->
      <div #modalCard
           class="relative w-full max-w-md rounded-2xl bg-[#0d0d0d] border border-white/10
                  shadow-[0_24px_80px_rgba(0,0,0,0.6)] animate-slide-up overflow-hidden">

        <!-- Close -->
        <button #closeBtn
                (click)="close()"
                aria-label="Close sign-in dialog"
                class="absolute top-4 right-4 z-10 w-8 h-8 flex items-center justify-center
                       text-neutral-500 hover:text-white transition-colors rounded-lg hover:bg-white/5">
          <iconify-icon icon="solar:close-linear" width="20"></iconify-icon>
        </button>

        <!-- Brand + Tabs -->
        <div class="px-6 pt-6 pb-0">
          <div class="flex items-center gap-2.5 mb-5">
            <div class="w-8 h-8 rounded-lg bg-gradient-to-br from-[#C65A1E] to-[#8B4513]
                        flex items-center justify-center shrink-0">
              <span class="font-['Forum'] text-sm font-bold text-white">2N</span>
            </div>
            <span id="auth-modal-title" class="font-['Forum'] text-lg text-white tracking-tight">
              Naar &amp; Noor
            </span>
          </div>

          <!-- Tabs -->
          <div class="flex border-b border-white/10 -mx-6 px-6" role="tablist">
            <button (click)="setTab('login')"
                    role="tab" [attr.aria-selected]="activeTab() === 'login'"
                    class="pb-3 px-1 mr-6 text-sm font-medium transition-all duration-200 border-b-2 -mb-px"
                    [class]="activeTab() === 'login'
                      ? 'text-white border-[#C65A1E]'
                      : 'text-neutral-500 border-transparent hover:text-neutral-300'">
              Sign In
            </button>
            <button (click)="setTab('register')"
                    role="tab" [attr.aria-selected]="activeTab() === 'register'"
                    class="pb-3 px-1 text-sm font-medium transition-all duration-200 border-b-2 -mb-px"
                    [class]="activeTab() === 'register'
                      ? 'text-white border-[#C65A1E]'
                      : 'text-neutral-500 border-transparent hover:text-neutral-300'">
              Create Account
            </button>
          </div>
        </div>

        <!-- ── LOGIN FORM ─────────────────────────────────────── -->
        <div *ngIf="activeTab() === 'login'" class="p-6" role="tabpanel">
          <p class="text-xs text-neutral-500 mb-5">Welcome back — sign in to your account</p>

          <form [formGroup]="loginForm" (ngSubmit)="submitLogin()" class="space-y-4" novalidate>

            <!-- Error banner -->
            <div *ngIf="loginError()" role="alert"
                 class="p-3 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-xs text-center">
              {{ loginError() }}
            </div>

            <!-- Email -->
            <div class="space-y-1.5">
              <label for="login-email"
                     class="text-xs font-medium text-neutral-400 uppercase tracking-wider">Email</label>
              <input id="login-email"
                     type="email" formControlName="email"
                     autocomplete="email"
                     placeholder="your@email.com"
                     class="w-full px-4 py-3 rounded-xl bg-white/5 border text-white
                            placeholder-neutral-600 focus:outline-none focus:ring-1 transition-all text-sm"
                     [class]="lf['email'].touched && lf['email'].errors
                       ? 'border-red-500/50 focus:ring-red-500/20'
                       : 'border-white/10 focus:border-[#C65A1E] focus:ring-[#C65A1E]/20'" />
              <p *ngIf="lf['email'].touched && lf['email'].errors" role="alert" class="text-xs text-red-400">
                <span *ngIf="lf['email'].errors?.['required']">Email is required</span>
                <span *ngIf="lf['email'].errors?.['email']">Enter a valid email</span>
              </p>
            </div>

            <!-- Password -->
            <div class="space-y-1.5">
              <label for="login-password"
                     class="text-xs font-medium text-neutral-400 uppercase tracking-wider">Password</label>
              <input id="login-password"
                     type="password" formControlName="password"
                     autocomplete="current-password"
                     placeholder="••••••••"
                     class="w-full px-4 py-3 rounded-xl bg-white/5 border text-white
                            placeholder-neutral-600 focus:outline-none focus:ring-1 transition-all text-sm"
                     [class]="lf['password'].touched && lf['password'].errors
                       ? 'border-red-500/50 focus:ring-red-500/20'
                       : 'border-white/10 focus:border-[#C65A1E] focus:ring-[#C65A1E]/20'" />
              <p *ngIf="lf['password'].touched && lf['password'].errors" role="alert" class="text-xs text-red-400">
                Password is required
              </p>
            </div>

            <!-- Submit -->
            <button type="submit" [disabled]="loginLoading()"
                    class="w-full py-3.5 mt-2 text-sm font-medium text-white rounded-xl
                           transition-all duration-300 flex items-center justify-center gap-2"
                    [class]="loginLoading()
                      ? 'bg-neutral-700 cursor-not-allowed'
                      : 'bg-[#C65A1E] hover:bg-[#a84915] hover:shadow-[0_0_24px_rgba(198,90,30,0.4)]'">
              <span *ngIf="loginLoading()"
                    class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></span>
              {{ loginLoading() ? 'Signing in…' : 'Sign In' }}
            </button>
          </form>

          <p class="mt-5 text-center text-xs text-neutral-600">
            No account yet?
            <button (click)="setTab('register')" class="text-[#C65A1E] hover:underline ml-1">Create one</button>
          </p>
        </div>

        <!-- ── REGISTER FORM ───────────────────────────────────── -->
        <div *ngIf="activeTab() === 'register'" class="p-6" role="tabpanel">
          <p class="text-xs text-neutral-500 mb-5">Join Naar &amp; Noor — order online &amp; reserve tables</p>

          <form [formGroup]="registerForm" (ngSubmit)="submitRegister()" class="space-y-4" novalidate>

            <!-- Error banner -->
            <div *ngIf="registerError()" role="alert"
                 class="p-3 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-xs text-center">
              {{ registerError() }}
            </div>

            <!-- Email -->
            <div class="space-y-1.5">
              <label for="reg-email"
                     class="text-xs font-medium text-neutral-400 uppercase tracking-wider">Email</label>
              <input id="reg-email"
                     type="email" formControlName="email"
                     autocomplete="email"
                     placeholder="your@email.com"
                     class="w-full px-4 py-3 rounded-xl bg-white/5 border text-white
                            placeholder-neutral-600 focus:outline-none focus:ring-1 transition-all text-sm"
                     [class]="rf['email'].touched && rf['email'].errors
                       ? 'border-red-500/50 focus:ring-red-500/20'
                       : 'border-white/10 focus:border-[#C65A1E] focus:ring-[#C65A1E]/20'" />
              <p *ngIf="rf['email'].touched && rf['email'].errors" role="alert" class="text-xs text-red-400">
                <span *ngIf="rf['email'].errors?.['required']">Email is required</span>
                <span *ngIf="rf['email'].errors?.['email']">Enter a valid email</span>
              </p>
            </div>

            <!-- Password -->
            <div class="space-y-1.5">
              <label for="reg-password"
                     class="text-xs font-medium text-neutral-400 uppercase tracking-wider">Password</label>
              <input id="reg-password"
                     type="password" formControlName="password"
                     autocomplete="new-password"
                     placeholder="••••••••"
                     class="w-full px-4 py-3 rounded-xl bg-white/5 border text-white
                            placeholder-neutral-600 focus:outline-none focus:ring-1 transition-all text-sm"
                     [class]="rf['password'].touched && rf['password'].errors
                       ? 'border-red-500/50 focus:ring-red-500/20'
                       : 'border-white/10 focus:border-[#C65A1E] focus:ring-[#C65A1E]/20'" />
              <p *ngIf="rf['password'].touched && rf['password'].errors" role="alert" class="text-xs text-red-400">
                <span *ngIf="rf['password'].errors?.['required']">Password is required</span>
                <span *ngIf="rf['password'].errors?.['minlength']">At least 6 characters</span>
              </p>
            </div>

            <!-- Confirm Password -->
            <div class="space-y-1.5">
              <label for="reg-confirm"
                     class="text-xs font-medium text-neutral-400 uppercase tracking-wider">Confirm Password</label>
              <input id="reg-confirm"
                     type="password" formControlName="confirmPassword"
                     autocomplete="new-password"
                     placeholder="••••••••"
                     class="w-full px-4 py-3 rounded-xl bg-white/5 border text-white
                            placeholder-neutral-600 focus:outline-none focus:ring-1 transition-all text-sm"
                     [class]="rf['confirmPassword'].touched && (rf['confirmPassword'].errors || registerForm.errors?.['mismatch'])
                       ? 'border-red-500/50 focus:ring-red-500/20'
                       : 'border-white/10 focus:border-[#C65A1E] focus:ring-[#C65A1E]/20'" />
              <p *ngIf="rf['confirmPassword'].touched && (rf['confirmPassword'].errors || registerForm.errors?.['mismatch'])"
                 role="alert" class="text-xs text-red-400">
                <span *ngIf="rf['confirmPassword'].errors?.['required']">Please confirm your password</span>
                <span *ngIf="registerForm.errors?.['mismatch']">Passwords do not match</span>
              </p>
            </div>

            <!-- Submit -->
            <button type="submit" [disabled]="registerLoading()"
                    class="w-full py-3.5 mt-2 text-sm font-medium text-white rounded-xl
                           transition-all duration-300 flex items-center justify-center gap-2"
                    [class]="registerLoading()
                      ? 'bg-neutral-700 cursor-not-allowed'
                      : 'bg-[#C65A1E] hover:bg-[#a84915] hover:shadow-[0_0_24px_rgba(198,90,30,0.4)]'">
              <span *ngIf="registerLoading()"
                    class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></span>
              {{ registerLoading() ? 'Creating account…' : 'Create Account' }}
            </button>
          </form>

          <p class="mt-5 text-center text-xs text-neutral-600">
            Already have an account?
            <button (click)="setTab('login')" class="text-[#C65A1E] hover:underline ml-1">Sign in</button>
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    @keyframes slideUp {
      from { opacity: 0; transform: translateY(24px) scale(0.97); }
      to   { opacity: 1; transform: translateY(0) scale(1); }
    }
    .animate-slide-up { animation: slideUp 0.25s cubic-bezier(0.32,0.72,0,1) both; }
  `]
})
export class AuthModalComponent implements OnChanges, AfterViewChecked {
  @Input() open = false;
  @Output() closeModal = new EventEmitter<void>();

  private readonly fb      = inject(FormBuilder);
  private readonly auth    = inject(AuthService);
  private readonly toast   = inject(ToastService);
  private readonly el      = inject(ElementRef);

  activeTab       = signal<'login' | 'register'>('login');
  loginLoading    = signal(false);
  loginError      = signal<string | null>(null);
  registerLoading = signal(false);
  registerError   = signal<string | null>(null);

  loginForm!: FormGroup;
  registerForm!: FormGroup;

  /** Track the element that triggered the modal so we can restore focus */
  private triggerEl: HTMLElement | null = null;
  /** Ensure we only focus the first field once per open */
  private focusSet = false;

  constructor() {
    this.buildForms();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['open']) {
      if (changes['open'].currentValue === true) {
        // Save trigger element before modal renders
        this.triggerEl = document.activeElement as HTMLElement;
        this.focusSet = false;
        this.buildForms();
        this.loginError.set(null);
        this.registerError.set(null);
        // Prevent background scroll
        document.body.style.overflow = 'hidden';
      } else {
        document.body.style.overflow = '';
        // Restore focus to trigger element
        setTimeout(() => this.triggerEl?.focus(), 50);
      }
    }
  }

  ngAfterViewChecked(): void {
    // Set initial focus to the first input once after modal opens
    if (this.open && !this.focusSet) {
      const firstInput = this.el.nativeElement.querySelector('input') as HTMLElement | null;
      if (firstInput) {
        firstInput.focus();
        this.focusSet = true;
      }
    }
  }

  /** Tab-trap: keep focus inside the modal */
  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') { this.close(); return; }
    if (event.key !== 'Tab') return;

    const card = this.el.nativeElement.querySelector('[role="dialog"] > *:last-child') as HTMLElement
              || this.el.nativeElement;
    const focusable = Array.from(card.querySelectorAll<HTMLElement>(FOCUSABLE))
      .filter(el => !el.closest('[disabled]'));

    if (!focusable.length) { event.preventDefault(); return; }

    const first = focusable[0];
    const last  = focusable[focusable.length - 1];

    if (event.shiftKey) {
      if (document.activeElement === first) { event.preventDefault(); last.focus(); }
    } else {
      if (document.activeElement === last) { event.preventDefault(); first.focus(); }
    }
  }

  @HostListener('document:keydown.escape')
  onEscape() { if (this.open) this.close(); }

  setTab(tab: 'login' | 'register') {
    this.activeTab.set(tab);
    this.loginError.set(null);
    this.registerError.set(null);
    this.focusSet = false; // re-focus first field on tab switch
  }

  close() { this.closeModal.emit(); }

  get lf() { return this.loginForm.controls; }
  get rf() { return this.registerForm.controls; }

  submitLogin(): void {
    if (this.loginForm.invalid) { this.loginForm.markAllAsTouched(); return; }
    this.loginLoading.set(true);
    this.loginError.set(null);
    const { email, password } = this.loginForm.value;

    this.auth.login(email, password).subscribe({
      next: (ok) => {
        this.loginLoading.set(false);
        if (ok) {
          this.toast.success('Welcome back!');
          this.close();
        } else {
          this.loginError.set('Invalid email or password. Please try again.');
        }
      },
      error: () => {
        this.loginLoading.set(false);
        this.loginError.set('Something went wrong. Please try again.');
      }
    });
  }

  submitRegister(): void {
    if (this.registerForm.invalid) { this.registerForm.markAllAsTouched(); return; }
    this.registerLoading.set(true);
    this.registerError.set(null);
    const { email, password } = this.registerForm.value;

    this.auth.register(email, password).subscribe({
      next: (ok) => {
        this.registerLoading.set(false);
        if (ok) {
          this.toast.success('Account created! Please sign in.');
          this.setTab('login');
          this.loginForm.patchValue({ email });
        } else {
          this.registerError.set('Registration failed. Email may already be in use.');
        }
      },
      error: () => {
        this.registerLoading.set(false);
        this.registerError.set('Something went wrong. Please try again.');
      }
    });
  }

  private buildForms(): void {
    this.loginForm = this.fb.group({
      email:    ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
    this.registerForm = this.fb.group({
      email:           ['', [Validators.required, Validators.email]],
      password:        ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatch });
  }

  private passwordMatch(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }
}
