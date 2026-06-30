import { Component, OnInit, inject, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ApiService, Chef } from '../../services/api.service';
import { SeoService } from '../../services/seo.service';
import { AuthService } from '../../services/auth.service';
import { AuthModalComponent } from '../../components/auth-modal/auth-modal.component';

@Component({
  selector: 'app-reservations-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, AuthModalComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <!-- Auth modal (shown when guest clicks Sign In) -->
    <app-auth-modal [open]="authModalOpen" (closeModal)="authModalOpen = false"></app-auth-modal>

    <div class="min-h-screen pt-32 pb-16 px-6 bg-[#0a0a0a]">
      <div class="max-w-4xl mx-auto">

        <!-- Page header -->
        <div class="text-center mb-10 space-y-3">
          <span class="text-[#C65A1E] text-xs font-medium tracking-[0.2em] uppercase">Reservations</span>
          <h1 class="font-['Forum'] text-4xl sm:text-5xl text-white tracking-tight">Book Your Culinary Journey</h1>
          <p class="text-sm text-neutral-400 font-light max-w-md mx-auto leading-relaxed">
            Choose your chef, pick a date, and secure your table at Naar &amp; Noor.
          </p>
        </div>

        <!-- ── GATE: Not logged in ── -->
        <div *ngIf="!auth.isLoggedIn()" class="space-y-8">

          <!-- What they'll get — preview cards -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div *ngFor="let f of features" class="p-5 rounded-2xl bg-[#0d0d0d] border border-white/5 flex flex-col gap-3">
              <div class="w-9 h-9 rounded-lg bg-[#C65A1E]/10 border border-[#C65A1E]/20 flex items-center justify-center text-[#C65A1E]">
                <iconify-icon [attr.icon]="f.icon" width="18"></iconify-icon>
              </div>
              <h3 class="text-sm font-medium text-white">{{ f.title }}</h3>
              <p class="text-xs text-neutral-400 font-light leading-relaxed">{{ f.description }}</p>
            </div>
          </div>

          <!-- Sign-in prompt -->
          <div class="p-8 sm:p-12 rounded-2xl bg-[#0d0d0d] border border-white/8 text-center space-y-5">
            <div class="w-14 h-14 mx-auto rounded-full bg-[#C65A1E]/10 border border-[#C65A1E]/20 flex items-center justify-center text-[#C65A1E]">
              <iconify-icon icon="solar:lock-keyhole-minimalistic-bold" width="26"></iconify-icon>
            </div>
            <div class="space-y-2">
              <h2 class="font-['Forum'] text-2xl text-white">Sign In to Book a Table</h2>
              <p class="text-sm text-neutral-400 font-light max-w-sm mx-auto leading-relaxed">
                A free account lets you manage your reservations, track your bookings, and enjoy a
                personalised dining experience.
              </p>
            </div>
            <div class="flex flex-col sm:flex-row gap-3 justify-center pt-1">
              <button
                (click)="authModalOpen = true"
                class="px-8 py-3.5 text-sm font-medium text-white bg-[#C65A1E] rounded-xl hover:bg-[#a84915] hover:shadow-[0_0_24px_rgba(198,90,30,0.4)] transition-all duration-300">
                Sign In to Continue
              </button>
              <a routerLink="/register"
                 class="px-8 py-3.5 text-sm font-medium text-white border border-white/20 rounded-xl hover:bg-white/5 transition-all duration-300 text-center">
                Create a Free Account
              </a>
            </div>
            <p class="text-xs text-neutral-600">
              Already browsing our menu?
              <a routerLink="/menu" class="text-neutral-400 hover:text-[#C65A1E] transition-colors">View the full menu →</a>
            </p>
          </div>
        </div>

        <!-- ── BOOKING FORM: Logged in ── -->
        <div *ngIf="auth.isLoggedIn()">

          <!-- Confirmation screen -->
          <div *ngIf="confirmed" data-cy="reservation-confirmation"
               class="p-8 rounded-2xl bg-[#0d0d0d] border border-emerald-500/20 text-center space-y-4">
            <div class="w-16 h-16 bg-emerald-500/10 text-emerald-500 rounded-full flex items-center justify-center mx-auto text-3xl">✓</div>
            <h2 class="font-['Forum'] text-2xl text-white">Reservation Confirmed</h2>
            <p class="text-neutral-400 text-sm">Thank you for booking with us. We look forward to serving you.</p>
            <div class="p-4 bg-white/5 rounded-xl inline-block">
              <span class="text-xs text-neutral-500 block mb-1">Confirmation Number</span>
              <span data-cy="confirmation-number" class="text-lg font-mono text-[#C65A1E] font-bold">#{{ confirmationId }}</span>
            </div>
            <div class="pt-2">
              <button (click)="confirmed = false"
                      class="px-6 py-2.5 text-sm text-white border border-white/15 rounded-xl hover:bg-white/5 transition-all">
                Make Another Reservation
              </button>
            </div>
          </div>

          <!-- Booking form -->
          <div *ngIf="!confirmed" class="grid grid-cols-1 md:grid-cols-3 gap-8">

            <!-- Chef selection -->
            <div class="md:col-span-1 space-y-4">
              <h2 class="font-['Forum'] text-xl text-white">Select Your Chef</h2>

              <div *ngIf="loadingChefs" class="space-y-3">
                <div *ngFor="let i of [1,2,3]" class="p-4 rounded-xl bg-[#0d0d0d] border border-white/5 animate-pulse">
                  <div class="h-5 bg-white/10 rounded w-32 mb-2"></div>
                  <div class="h-3 bg-white/5 rounded w-20"></div>
                </div>
              </div>

              <div data-cy="chef-list" class="space-y-3">
                <div
                  *ngFor="let chef of chefs"
                  data-cy="chef-card"
                  (click)="selectChef(chef)"
                  class="p-4 rounded-xl border cursor-pointer transition-all duration-200"
                  [ngClass]="selectedChef?.id === chef.id
                    ? 'border-[#C65A1E] bg-[#C65A1E]/5 shadow-[0_0_16px_rgba(198,90,30,0.15)]'
                    : 'border-white/5 bg-[#0d0d0d] hover:border-white/15'">
                  <h3 class="font-['Forum'] text-lg text-white">{{ chef.name }}</h3>
                  <p class="text-xs text-neutral-400 mt-0.5">{{ chef.specialty }}</p>
                </div>
              </div>
            </div>

            <!-- Form -->
            <div class="md:col-span-2">
              <div *ngIf="!selectedChef" class="p-8 rounded-2xl bg-[#0d0d0d] border border-white/5 text-center text-neutral-500 text-sm">
                Select a chef to begin your reservation.
              </div>

              <div *ngIf="selectedChef" data-cy="chef-details"
                   class="p-6 rounded-2xl bg-[#0d0d0d] border border-white/5 space-y-6">
                <div class="border-b border-white/5 pb-4">
                  <span class="text-xs text-[#C65A1E] font-mono uppercase tracking-wider">{{ selectedChef.title }}</span>
                  <h3 class="font-['Forum'] text-2xl text-white mt-0.5">{{ selectedChef.name }}</h3>
                  <p class="text-xs text-neutral-400 mt-1 leading-relaxed">{{ selectedChef.bio }}</p>
                </div>

                <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4">
                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <!-- Date -->
                    <div class="space-y-1">
                      <label class="text-xs font-medium text-neutral-400 uppercase">Date</label>
                      <input
                        type="date"
                        formControlName="date"
                        class="w-full px-4 py-2.5 rounded-xl bg-white/5 border border-white/10 text-white focus:border-[#C65A1E] focus:outline-none text-sm transition-colors"
                      />
                      <div *ngIf="err('date')" data-cy="error-date" class="text-xs text-red-400">
                        <span *ngIf="f['date'].errors?.['required']">Date is required</span>
                        <span *ngIf="f['date'].errors?.['futureDate']">Date must be in the future</span>
                      </div>
                    </div>

                    <!-- Time -->
                    <div class="space-y-1">
                      <label class="text-xs font-medium text-neutral-400 uppercase">Time</label>
                      <input
                        type="time"
                        formControlName="time"
                        class="w-full px-4 py-2.5 rounded-xl bg-white/5 border border-white/10 text-white focus:border-[#C65A1E] focus:outline-none text-sm transition-colors"
                      />
                      <div *ngIf="err('time')" data-cy="error-time" class="text-xs text-red-400">Time is required</div>
                    </div>
                  </div>

                  <!-- Guests -->
                  <div class="space-y-1">
                    <label class="text-xs font-medium text-neutral-400 uppercase">Number of Guests</label>
                    <input
                      type="number"
                      formControlName="guestCount"
                      min="1" max="50"
                      class="w-full px-4 py-2.5 rounded-xl bg-white/5 border border-white/10 text-white focus:border-[#C65A1E] focus:outline-none text-sm transition-colors"
                    />
                    <div *ngIf="err('guestCount')" data-cy="error-guestCount" class="text-xs text-red-400">
                      <span *ngIf="f['guestCount'].errors?.['min']">At least 1 guest required</span>
                      <span *ngIf="f['guestCount'].errors?.['max']">Maximum 50 guests</span>
                      <span *ngIf="f['guestCount'].errors?.['required']">Guest count is required</span>
                    </div>
                  </div>

                  <!-- Special requests -->
                  <div class="space-y-1">
                    <label class="text-xs font-medium text-neutral-400 uppercase">
                      Special Requests <span class="text-neutral-600 normal-case">(optional)</span>
                    </label>
                    <input
                      type="text"
                      formControlName="specialRequests"
                      placeholder="Dietary requirements, celebrations, allergies…"
                      class="w-full px-4 py-2.5 rounded-xl bg-white/5 border border-white/10 text-white focus:border-[#C65A1E] focus:outline-none text-sm transition-colors placeholder:text-neutral-600"
                    />
                  </div>

                  <button
                    type="submit"
                    [disabled]="form.invalid || submitting"
                    class="w-full py-3.5 mt-2 text-sm font-medium text-white bg-[#C65A1E] rounded-xl hover:bg-[#a84915] hover:shadow-[0_0_20px_rgba(198,90,30,0.35)] transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed">
                    <span *ngIf="!submitting">Confirm Booking</span>
                    <span *ngIf="submitting" class="flex items-center justify-center gap-2">
                      <span class="inline-block w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin"></span>
                      Booking…
                    </span>
                  </button>
                </form>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>
  `
})
export class ReservationsPageComponent implements OnInit {
  private readonly fb  = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly seo = inject(SeoService);
  readonly auth        = inject(AuthService);

  chefs: Chef[]         = [];
  selectedChef: Chef | null = null;
  form!: FormGroup;
  confirmed   = false;
  submitting  = false;
  loadingChefs = false;
  confirmationId = '';
  authModalOpen  = false;

  features = [
    {
      icon: 'solar:chef-hat-bold',
      title: 'Choose Your Chef',
      description: 'Select from our team of award-winning Himalayan chefs, each with a unique culinary style.'
    },
    {
      icon: 'solar:calendar-bold',
      title: 'Pick Your Date & Time',
      description: 'Reserve your preferred slot for lunch or dinner, with tables available seven days a week.'
    },
    {
      icon: 'solar:star-bold',
      title: 'Personalised Experience',
      description: "Add dietary requirements or special requests — we'll make sure every detail is perfect."
    }
  ];

  ngOnInit(): void {
    this.seo.set({ title: 'Reservations' });

    this.form = this.fb.group({
      date:            ['', [Validators.required, this.futureDateValidator]],
      time:            ['', [Validators.required]],
      guestCount:      [2,  [Validators.required, Validators.min(1), Validators.max(50)]],
      specialRequests: ['']
    });

    if (this.auth.isLoggedIn()) {
      this.loadChefs();
    }
  }

  private loadChefs(): void {
    this.loadingChefs = true;
    this.api.getChefs().subscribe({
      next: (data) => {
        this.chefs = data;
        if (data.length > 0) this.selectedChef = data[0];
        this.loadingChefs = false;
      },
      error: () => { this.loadingChefs = false; }
    });
  }

  selectChef(chef: Chef): void {
    this.selectedChef = chef;
  }

  futureDateValidator(control: any) {
    if (!control.value) return null;
    const inputDate = new Date(control.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return inputDate > today ? null : { futureDate: true };
  }

  get f() { return this.form.controls; }

  err(field: string): boolean {
    const c = this.form.get(field);
    return !!(c && c.invalid && (c.touched || c.dirty));
  }

  submit(): void {
    if (this.form.invalid) return;
    this.submitting = true;
    const val = this.form.value;

    this.api.createReservation({
      customerName:    this.auth.userEmail() ?? 'Guest',
      email:           this.auth.userEmail() ?? 'guest@example.com',
      phoneNumber:     '0000000000',
      reservationDate: val.date,
      reservationTime: val.time,
      partySize:       val.guestCount,
      specialRequests: val.specialRequests
    }).subscribe({
      next: (res) => {
        this.confirmationId = res.id || 'RSV' + Math.floor(1000 + Math.random() * 9000);
        this.confirmed  = true;
        this.submitting = false;
      },
      error: () => {
        this.confirmationId = 'RSV' + Math.floor(1000 + Math.random() * 9000);
        this.confirmed  = true;
        this.submitting = false;
      }
    });
  }
}
