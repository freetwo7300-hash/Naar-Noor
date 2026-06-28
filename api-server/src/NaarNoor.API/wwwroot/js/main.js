/* ====================================================
   Naar & Noor API — Shared Scripts
   ==================================================== */

/* ── Active nav link highlight ─────────────────── */
(function markActiveNavLink() {
  const links = document.querySelectorAll('.nav-link[data-page]');
  const current = location.pathname.replace(/\/$/, '') || '/';
  links.forEach(link => {
    const page = link.getAttribute('data-page');
    if (page === current || (current === '/' && page === '/')) {
      link.classList.add('active');
    }
  });
})();

/* ── Mobile nav toggle ──────────────────────────── */
const toggleBtn = document.querySelector('.nav-toggle');
const nav = document.querySelector('nav');
if (toggleBtn && nav) {
  toggleBtn.addEventListener('click', () => {
    const open = nav.classList.toggle('open');
    toggleBtn.setAttribute('aria-expanded', open);
  });
  // Close on outside click
  document.addEventListener('click', e => {
    if (!nav.contains(e.target) && !toggleBtn.contains(e.target)) {
      nav.classList.remove('open');
    }
  });
}

/* ── Intersection Observer — fade-up on scroll ──── */
(function initScrollReveal() {
  if (!window.IntersectionObserver) return;
  const els = document.querySelectorAll('[data-reveal]');
  if (!els.length) return;
  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const delay = entry.target.dataset.delay || 0;
        setTimeout(() => {
          entry.target.style.opacity = '1';
          entry.target.style.transform = 'translateY(0)';
        }, delay);
        observer.unobserve(entry.target);
      }
    });
  }, { threshold: 0.12 });

  els.forEach(el => {
    el.style.opacity = '0';
    el.style.transform = 'translateY(28px)';
    el.style.transition = 'opacity 0.5s ease-out, transform 0.5s ease-out';
    observer.observe(el);
  });
})();

/* ── Health / Status Check ──────────────────────── */
async function checkEndpointHealth(url, elStatus, elLatency) {
  const start = performance.now();
  try {
    const res = await fetch(url, { cache: 'no-store', signal: AbortSignal.timeout(6000) });
    const latency = Math.round(performance.now() - start);
    const ok = res.ok;

    if (elStatus) {
      elStatus.className = 'status-dot ' + (ok ? '' : 'error');
      elStatus.title = ok ? 'Online' : `HTTP ${res.status}`;
    }
    if (elLatency) {
      elLatency.textContent = latency + ' ms';
      elLatency.className = latency < 200 ? 'text-accent' : latency < 600 ? '' : 'text-dim';
    }
    return { ok, latency, status: res.status };
  } catch (err) {
    if (elStatus) { elStatus.className = 'status-dot error'; elStatus.title = 'Unreachable'; }
    if (elLatency) { elLatency.textContent = '—'; }
    return { ok: false, latency: null, status: null };
  }
}

/* ── Status page ────────────────────────────────── */
async function initStatusPage() {
  const grid = document.getElementById('status-grid');
  if (!grid) return;

  const services = [
    { id: 'svc-api',          label: 'API Server',       url: '/health',        desc: 'Core API health endpoint' },
    { id: 'svc-menu',         label: 'Menu Endpoint',    url: '/api/menu',      desc: 'GET /api/menu' },
    { id: 'svc-chefs',        label: 'Chefs Endpoint',   url: '/api/chefs',     desc: 'GET /api/chefs' },
    { id: 'svc-reviews',      label: 'Reviews Endpoint', url: '/api/reviews',   desc: 'GET /api/reviews' },
  ];

  services.forEach(svc => {
    const card = document.createElement('div');
    card.className = 'card';
    card.style.cssText = 'display:flex;flex-direction:column;gap:1rem;';
    card.innerHTML = `
      <div style="display:flex;align-items:center;justify-content:space-between;gap:0.5rem;">
        <div style="display:flex;align-items:center;gap:0.6rem;">
          <span class="status-dot checking" id="${svc.id}-dot"></span>
          <strong style="font-size:0.9rem;">${svc.label}</strong>
        </div>
        <span class="badge badge-checking" id="${svc.id}-badge">Checking…</span>
      </div>
      <div style="display:flex;align-items:center;justify-content:space-between;font-size:0.78rem;color:var(--text-dim)">
        <code style="background:none;border:none;padding:0;color:var(--text-dim);font-size:0.75rem;">${svc.url}</code>
        <span id="${svc.id}-latency" style="font-size:0.75rem;">—</span>
      </div>
      <p style="font-size:0.75rem;margin:0;">${svc.desc}</p>
    `;
    grid.appendChild(card);

    checkEndpointHealth(svc.url, document.getElementById(svc.id + '-dot'), document.getElementById(svc.id + '-latency'))
      .then(({ ok }) => {
        const badge = document.getElementById(svc.id + '-badge');
        if (badge) {
          badge.textContent = ok ? 'Online' : 'Degraded';
          badge.className = 'badge ' + (ok ? 'badge-online' : 'badge-error');
        }
        updateOverallStatus();
      });
  });
}

function updateOverallStatus() {
  const dots = document.querySelectorAll('[id$="-dot"]');
  const errors = Array.from(dots).filter(d => d.classList.contains('error')).length;
  const checking = Array.from(dots).filter(d => d.classList.contains('checking')).length;
  const overallBadge = document.getElementById('overall-badge');
  const overallDot   = document.getElementById('overall-dot');
  const overallText  = document.getElementById('overall-text');
  if (checking > 0) return;
  if (overallBadge) overallBadge.textContent = errors === 0 ? 'All Systems Operational' : `${errors} Service(s) Degraded`;
  if (overallBadge) overallBadge.className = 'badge ' + (errors === 0 ? 'badge-online' : 'badge-error');
  if (overallDot)   overallDot.className = 'status-dot ' + (errors > 0 ? 'error' : '');
  if (overallText)  overallText.textContent = errors === 0 ? 'All systems are operating normally.' : 'Some services may be impacted.';
}

document.addEventListener('DOMContentLoaded', initStatusPage);
