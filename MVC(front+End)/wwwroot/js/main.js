'use strict';
// ==========================================
// INITIALIZATION (concise grouping)
// ==========================================
document.addEventListener('DOMContentLoaded', () => {
  [initNavbar, initHeroAnimations, initSearch, initExperienceFilters, initCounters, initBackToTop, initForms, initCarousels, initScrollAnimations, initTooltips, initDataDrivenPages, initModalFromQuery, initReviewsPage].forEach(fn => fn());
}
);
// ==========================================
// NAVBAR FUNCTIONALITY
// ==========================================
function initNavbar() {
  const navbar = document.querySelector('.navbar');
  const navbarToggler = document.querySelector('.navbar-toggler');
  const navbarCollapse = document.querySelector('.navbar-collapse');
  window.addEventListener('scroll', () => navbar.classList.toggle('scrolled', window.scrollY > 50));
  document.addEventListener('click', event => {
    if (!navbar.contains(event.target) && navbarCollapse.classList.contains('show')) navbarToggler.click();
  }
  );
  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function(e) {
      const href = this.getAttribute('href');
      if (!['#', '#loginModal', '#signupModal', '#becomeHostModal', '#bookingModal'].includes(href)) {
        e.preventDefault();
        const target = document.querySelector(href);
        if (target) {
          window.scrollTo( {
            top: target.offsetTop - 80,
            behavior: 'smooth'
          }
          );
          if (navbarCollapse.classList.contains('show')) navbarToggler.click();
        }
      }
    }
    );
  }
  );
  // Ensure right-side auth UI exists on every page, then keep it current
  ensureAuthUiSlots();
  renderNavUserArea();
}
// ==========================================
// HERO ANIMATIONS & SCROLL INDICATOR
// ==========================================
function initHeroAnimations() {
  const today = new Date().toISOString().split('T')[0];
  document.querySelectorAll('input[type="date"]').forEach(input => input.setAttribute('min', today));
  document.querySelector('.scroll-indicator')?.addEventListener('click', () => {
    document.getElementById('experiences')?.scrollIntoView( {
      behavior: 'smooth'
    }
    );
  }
  );
}
// ==========================================
// SEARCH FUNCTIONALITY
// ==========================================
function initSearch() {
  const searchForm = document.getElementById('searchForm');
  if (searchForm) {
    searchForm.addEventListener('submit', function(e) {
      e.preventDefault();
      const location = document.getElementById('locationInput').value;
      const date = document.getElementById('dateInput').value;
      const category = document.getElementById('categorySelect').value;
      const submitBtn = searchForm.querySelector('button[type="submit"]');
      const originalText = submitBtn.innerHTML;
      submitBtn.innerHTML = '<span class="loading"></span> Searching...';
      submitBtn.disabled = true;
      setTimeout(() => {
        filterExperiences(location, date, category);
        document.getElementById('experiences').scrollIntoView( {
          behavior: 'smooth'
        }
        );
        submitBtn.innerHTML = originalText;
        submitBtn.disabled = false;
        showNotification('success', `Found experiences${location ? ' in ' + location : ''}!`);
      }
      , 1500);
    }
    );
  }
}
function filterExperiences(location, date, category) {
  let cards = document.querySelectorAll('.experience-card');
  // Fallback to featured cards if generic cards do not exist
  const usingFeatured = cards.length === 0;
  if (usingFeatured) cards = document.querySelectorAll('.experience-featured-card');
  // Helper: get card title/description text safely
  const getText = (el, sel) => el.querySelector(sel)?.textContent?.toLowerCase() || '';
  // Infer categories for featured cards if not present
  if (usingFeatured) {
    cards.forEach(card => {
      if (!card.dataset.category) {
        const title = getText(card, '.experience-featured-title');
        // Simple mapping by keywords
        let inferred = '';
        if (/diving|red sea|dahab/.test(title)) inferred = 'water';
        else if (/balloon|luxor/.test(title)) inferred = 'adventure';
        else if (/bedouin|sinai|desert/.test(title)) inferred = 'desert';
        card.dataset.category = inferred;
        // may be empty if unknown
      }
    }
    );
  }
  let visible = 0;
  cards.forEach(card => {
    const cardCategory = (card.getAttribute('data-category') || '').toLowerCase();
    const textBlob = usingFeatured
    ? (getText(card, '.experience-featured-title') + ' ' + getText(card, '.experience-featured-description'))
    : (card.querySelector('.card-text.text-muted')?.textContent?.toLowerCase() || '');
    let matches = true;
    if (location && !textBlob.includes(location.toLowerCase())) matches = false;
    if (category && cardCategory !== category) matches = false;
    card.style.display = matches ? 'block' : 'none';
    if (matches) visible++;
  }
  );
  // Show message if no results (guard against missing grid container)
  const grid = document.getElementById('experiencesGrid') || document.querySelector('#experiences .row.g-4');
  let msg = document.getElementById('noResultsMessage');
  if (visible === 0) {
    if (!msg && grid) {
      msg = document.createElement('div');
      msg.id = 'noResultsMessage';
      msg.className = 'col-12 text-center py-5';
      msg.innerHTML = `<i class="bi bi-search" style="font-size: 4rem;
      color: #9ca3af;
      "></i>
      <h4 class="mt-3">No experiences found</h4>
      <p class="text-muted">Try adjusting your search filters</p>
      <button class="btn btn-primary mt-3" onclick="resetFilters()">Reset Filters</button>`;
      grid.appendChild(msg);
    }
  }
  else if (msg) {
    msg.remove();
  }
}
function resetFilters() {
  ['locationInput', 'dateInput', 'categorySelect'].forEach(id => document.getElementById(id).value = '');
  document.querySelectorAll('.experience-card').forEach(card => card.style.display = 'block');
  document.getElementById('noResultsMessage')?.remove();
  const btns = document.querySelectorAll('.category-filter .btn');
  btns.forEach(btn => btn.classList.remove('active'));
  btns[0]?.classList.add('active');
}
// ==========================================
// EXPERIENCE FILTERS
// ==========================================
function initExperienceFilters() {
  const filterButtons = document.querySelectorAll('.category-filter .btn');
  filterButtons.forEach(button => {
    button.addEventListener('click', function() {
      filterButtons.forEach(btn => btn.classList.remove('active'));
      this.classList.add('active');
      const filter = this.getAttribute('data-filter');
      document.querySelectorAll('.experience-card').forEach(card => {
        card.style.display = (filter === 'all' || card.getAttribute('data-category') === filter) ? 'block' : 'none';
      }
      );
    }
    );
  }
  );
}
// ==========================================
// COUNTER ANIMATIONS (unchanged, already concise)
// ==========================================
function initCounters() {
  const counters = document.querySelectorAll('.counter');
  const animate = c => {
    const target = +c.getAttribute('data-target');
    const inc = target / (2000/16);
    let cur = 0;
    const upd = () => {
      cur += inc;
      if (cur < target) {
        c.textContent = Math.floor(cur) + '+';
        requestAnimationFrame(upd);
      }
      else {
        c.textContent = target + '+';
      }
    };
    upd();
  };
  const obs = new IntersectionObserver(es => es.forEach(entry => {
    if (entry.isIntersecting) {
      animate(entry.target);
      obs.unobserve(entry.target);
    }
  }
  ), {
    threshold:0.5
  }
  );
  counters.forEach(c => obs.observe(c));
}
// ==========================================
// BACK TO TOP
// ==========================================
function initBackToTop() {
  const btn = document.getElementById('backToTop');
  if (btn) {
    window.addEventListener('scroll', () => btn.classList.toggle('show', window.scrollY > 300));
    btn.addEventListener('click', () => window.scrollTo( {
      top: 0, behavior: 'smooth'
    }
    ));
  }
}
// ==========================================
// FORM HANDLERS
// ==========================================
function initForms() {
  [ {
    form:'loginForm', btnText:'Login', cb:()=> {
      const identifier = (document.getElementById('loginEmail')?.value || '').trim();
      const pass = (document.getElementById('loginPassword')?.value || '').trim();
      const fallbackName = identifier?.split('@')?.[0] || 'Traveler';
      const current = loadStore(STORAGE_KEYS.user, {}) || {};
      saveStore(STORAGE_KEYS.user, { ...current, name: current.name || fallbackName, email: identifier || current.email || '', password: pass || current.password || '', role: current.role || 'Traveler' });
      setAuth(true);
      showNotification('success','Logged in.');
      const pending = loadStore(STORAGE_KEYS.redirect, null);
      if (pending?.to === 'dashboard') {
        localStorage.removeItem(STORAGE_KEYS.redirect);
        setTimeout(()=> window.location.href='dashboard.html', 400);
      }
    }
  }
  , {
    form:'signupForm', btnText:'Sign Up', cb:()=> {
      const firstName = (document.getElementById('signupFirstName')?.value || '').trim();
      const lastName = (document.getElementById('signupLastName')?.value || '').trim();
      const fullNameFallback = (document.getElementById('signupName')?.value || '').trim();
      const name = (firstName || lastName) ? [firstName,lastName].filter(Boolean).join(' ') : (fullNameFallback || 'Traveler');
      const email = (document.getElementById('signupEmail')?.value || '').trim();
      const phone = (document.getElementById('signupPhone')?.value || '').trim();
      const nationality = (document.getElementById('signupNationality')?.value || '').trim();
      const dateOfBirth = (document.getElementById('signupDOB')?.value || '').trim();
      const gender = (document.getElementById('signupGender')?.value || '').trim();
      const pw = (document.getElementById('signupPassword')?.value || '').trim();
      const pwc = (document.getElementById('signupPasswordConfirm')?.value || '').trim();
      const roleInput = document.getElementById('signupRole');
      const role = (roleInput?.value || 'Traveler').trim() || 'Traveler';
      // No client-side validation; backend will validate
      const newUser = {
        id: 'u-'+Date.now(),
        name,
        firstName: firstName || (fullNameFallback.split(' ')[0] || ''),
        lastName: lastName || (fullNameFallback.split(' ').slice(1).join(' ') || ''),
        email,
        password: pw || pwc || '',
        avatar: 'https://i.pravatar.cc/100?img=13',
        role,
        phone,
        nationality,
        dateOfBirth,
        gender,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        registeredAt: new Date().toISOString()
      };
      const user = loadStore(STORAGE_KEYS.user, {});
      saveStore(STORAGE_KEYS.user, { ...user, id:newUser.id, name, email, password: pw, role, avatar: newUser.avatar, firstName:newUser.firstName, lastName:newUser.lastName, phone, nationality, dateOfBirth, gender, createdAt:newUser.createdAt, updatedAt:newUser.updatedAt });
      showNotification('success','Account created.');
      setAuth(true);
      const pending = loadStore(STORAGE_KEYS.redirect, null);
      if (pending?.to === 'dashboard') {
        localStorage.removeItem(STORAGE_KEYS.redirect);
        setTimeout(()=> window.location.href='dashboard.html', 400);
      }
    }
  }
  , {
    form:'becomeHostForm', btnText:'Submit Application', cb:()=> {
      showNotification('success','Application submitted! We will review and contact you soon.');
    }
  }
  , {
    form:'bookingForm', btnText:'Proceed to Payment', cb:()=> {
      showNotification('success','Booking confirmed! Check your email for details.');
    }
  }
  , {
    form:'contactForm', btnText:'Send', cb:()=> {
      showNotification('success','Message sent! We will get back to you soon.');
    }
  }
  ,
  ].forEach(( {
    form, cb
  }
  ) => {
    const f = document.getElementById(form);
    if (!f) return;
    // Disable HTML5 validation; backend will validate
    try{ f.setAttribute('novalidate','novalidate'); }catch(_e){}
    f.addEventListener('submit', function(e) {
      e.preventDefault();
      const btn = f.querySelector('button[type="submit"]');
      const origText = btn.textContent;
      btn.innerHTML = '<span class="loading"></span> ' + origText+'...';
      btn.disabled = true;
      setTimeout(() => {
        btn.textContent = origText;
        btn.disabled = false;
        if(['loginForm','signupForm','becomeHostForm','bookingForm'].includes(form)) {
          const modal = bootstrap.Modal.getInstance(document.getElementById(form.replace('Form','Modal')));
          modal?.hide();
        }
        cb();
        f.reset();
      }
      , form==='bookingForm'?2000:1500);
    }
    );
    if(form==='bookingForm') {
      const guestsInput = document.getElementById('bookingGuests');
      guestsInput?.addEventListener('change',function () {
        const guests = parseInt(this.value);
        const price = 49;
        const totalInput = f.querySelector('input[readonly]');
        if (totalInput) totalInput.value = `$${guests * price}.00`;
      }
      );
    }
  }
  );
}
// demo users export removed
// ==========================================
// CAROUSELS
// ==========================================
function initCarousels() {
  const el = document.getElementById('testimonialsCarousel');
  if (el) new bootstrap.Carousel(el, {
    interval: 5000, wrap: true, keyboard: true
  }
  );
}
// ==========================================
// SCROLL ANIMATIONS
// ==========================================
function initScrollAnimations() {
  const els = document.querySelectorAll('.card, .how-it-works-card, .feature-item, .contact-info-card, .place-card, .why-card, .experience-featured-card, .testimonial-card-new, .cta-image-card');
  const obs = new IntersectionObserver((entries) => {
    entries.forEach((entry, i) => {
      if (entry.isIntersecting) {
        entry.target.style.opacity = '0';
        entry.target.style.transform = 'translateY(30px)';
        setTimeout(() => {
          entry.target.style.transition = 'opacity 0.6s, transform 0.6s';
          entry.target.style.opacity = '1';
          entry.target.style.transform = 'translateY(0)';
        }
        , 100 * i);
        obs.unobserve(entry.target);
      }
    }
    );
  }
  , {
    threshold: 0.1, rootMargin: '0px 0px -50px 0px'
  }
  );
  els.forEach(el => obs.observe(el));
}
// ==========================================
// TOOLTIPS
// ==========================================
function initTooltips() {
  [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
  .map(el => new bootstrap.Tooltip(el));
}
// ==========================================
// NOTIFICATION SYSTEM (minimal, reused)
// ==========================================
function showNotification(type, message) {
  const n = document.createElement('div');
  n.className = `alert alert-${type==='error'?'danger':type} notification-toast`;
  n.style.cssText = `position: fixed;
  top: 100px;
  right: 20px;
  z-index: 9999;
  min-width: 300px;
  animation: slideInRight 0.3s;
  box-shadow:0 10px 15px -3px rgba(0,0,0,0.1);
  `;
  let icon = '';
  switch(type) {
    case 'success':icon='<i class="bi bi-check-circle-fill me-2"></i>';
    break;
    case 'error':icon='<i class="bi bi-x-circle-fill me-2"></i>';
    break;
    case 'info':icon='<i class="bi bi-info-circle-fill me-2"></i>';
    break;
    default:icon='<i class="bi bi-bell-fill me-2"></i>';
  }
  n.innerHTML = `<div class="d-flex align-items-center">${icon}<span>${message}</span><button type="button" class="btn-close ms-auto" onclick="this.parentElement.parentElement.remove()"></button></div>`;
  document.body.appendChild(n);
  setTimeout(() => {
    n.style.animation='slideOutRight 0.3s';
    setTimeout(()=>n.remove(),300);
  }
  , 5000);
}
// Add CSS for notification animations (collapse to one-time setup)
if (!window._notifStyles) {
  const style = document.createElement('style');
  style.textContent = `@keyframes slideInRight {
    from {
      transform:translateX(400px);
      opacity:0;
    }
    to {
      transform:translateX(0);
      opacity:1;
    }
  }
  @keyframes slideOutRight {
    from {
      transform:translateX(0);
      opacity:1;
    }
    to {
      transform:translateX(400px);
      opacity:0;
    }
  }
  .notification-toast {
    border-radius:0.5rem;
    border:none;
  }
  `;
  document.head.appendChild(style);
  window._notifStyles=1;
}
// ==========================================
// UTILITY FUNCTIONS
// ==========================================
// Debounce function for performance
function debounce(func, wait) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}
// Format currency
function formatCurrency(amount) {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }
  ).format(amount);
}
// Format date
function formatDate(dateString) {
  const options = {
    year: 'numeric', month: 'long', day: 'numeric'
  };
  return new Date(dateString).toLocaleDateString('en-US', options);
}
// ==========================================
// PERFORMANCE OPTIMIZATION
// ==========================================
// Lazy load images
if ('IntersectionObserver' in window) {
  const imageObserver = new IntersectionObserver((entries, observer) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const img = entry.target;
        img.src = img.dataset.src;
        img.classList.remove('lazy');
        imageObserver.unobserve(img);
      }
    }
    );
  }
  );
  const lazyImages = document.querySelectorAll('img.lazy');
  lazyImages.forEach(img => imageObserver.observe(img));
}

// ==========================================
// ERROR HANDLING
// ==========================================
window.addEventListener('error', function(e) {
  console.error('An error occurred:', e.error);
  // In production, you might want to log this to an error tracking service
}
);
// ==========================================
// CONSOLE MESSAGE
// ==========================================

console.log('%c Egyptian Experiences ', 'background: linear-gradient(135deg, #2563eb, #10b981); color: white; font-size: 20px; padding: 10px; font-weight: bold;');
console.log(
  '%c Developed with ❤️ for tourists and hosts ',
  'color: #2563eb; font-size: 14px;'
);
window.resetFilters = resetFilters;
window.showNotification = showNotification;
// ==========================================
// REVIEWS PAGE INTERACTIONS
// ==========================================
function initReviewsPage(){
  const wrapper = document.querySelector('.reviews-wrapper');
  if (!wrapper) return; // not on reviews page
  // Star rating groups
  wrapper.querySelectorAll('.reviews-stars').forEach(group=>{
    const targetSel = group.getAttribute('data-target-input');
    const input = targetSel ? document.querySelector(targetSel) : null;
    const buttons = Array.from(group.querySelectorAll('button'));
    const setActive = (n)=>{
      buttons.forEach((btn, i)=>btn.classList.toggle('active', i < n));
      if (input) input.value = String(n);
      if (targetSel==='#overallRating'){
        const hint = document.getElementById('overallHint');
        if (hint){
          hint.textContent = n ? `${n}.0 / 5.0` : 'Select a rating';
        }
      }
    };
    buttons.forEach((btn, idx)=>{
      btn.addEventListener('click', ()=> setActive(idx+1));
    });
  });
  // Character counter
  const ta = document.getElementById('reviewText');
  const cc = document.getElementById('charCount');
  if (ta && cc){
    const limit = 1000;
    const update = ()=>{
      const len = Math.min(limit, (ta.value||'').length);
      cc.textContent = String(len);
      if (len>limit) ta.value = ta.value.slice(0, limit);
    };
    ta.addEventListener('input', update);
    update();
  }
  // Photo upload preview (demo)
  const input = document.getElementById('photoInput');
  const preview = document.getElementById('photoPreview');
  const uploader = wrapper.querySelector('.reviews-uploader');
  if (uploader){
    uploader.addEventListener('click', ()=> document.getElementById('photoInput')?.click());
  }
  if (input && preview){
    input.addEventListener('change', ()=>{
      preview.innerHTML='';
      const files = Array.from(input.files||[]).slice(0,10);
      files.forEach(file=>{
        if (!file.type.startsWith('image/')) return;
        const url = URL.createObjectURL(file);
        const img = document.createElement('img');
        img.src = url;
        img.alt = file.name;
        img.style.width='84px'; img.style.height='84px'; img.style.objectFit='cover';
        img.className = 'rounded border';
        preview.appendChild(img);
      });
    });
  }
  // Submit
  const submit = document.getElementById('submitReview');
  if (submit){
    submit.addEventListener('click', ()=>{
      const overall = parseInt(document.getElementById('overallRating')?.value||'0',10);
      const text = (document.getElementById('reviewText')?.value||'').trim();
      if (overall<1){ showNotification('error','Please add an overall rating.'); return; }
      if (text.length < 50){ showNotification('error','Please write at least 50 characters.'); return; }
      showNotification('success','Thanks for sharing your experience!');
      setTimeout(()=> window.location.href='dashboard.html', 800);
    });
  }
}
function initModalFromQuery() {
  const params = new URLSearchParams(window.location.search);
  const modalParam = params.get('modal');
  if (!modalParam) {
    return;
  }
  const map = {
    login:'loginModal', signup:'signupModal', host:'becomeHostModal'
  };
  const id = map[modalParam];
  if (!id) return;
  const el = document.getElementById(id);
  if (el && window.bootstrap?.Modal) {
    const m = new bootstrap.Modal(el);
    m.show();
  }
}
// ==========================================
// DATA-DRIVEN PAGES (stays/property/booking)
// ==========================================
function getQueryParam(key) {
  const url = new URL(window.location.href);
  return url.searchParams.get(key);
}
// Local data source with graceful fallback to embedded demo data
async function fetchListings() {
  const demo = [
    {
      id:'egy-cairo-loft',
      title:'Contemporary Loft near the Nile',
      location:'Cairo, Egypt',
      price:89,
      beds:2, baths:1, guests:3,
      rating:4.8,
      amenities:['WiFi','Kitchen','AC','Balcony','Parking'],
      images:['assets/photos/cairo.png','assets/photos/alexandria.png','assets/photos/dahab.png'],
      summary:'Modern loft with views towards the Nile, close to museums and cafes.'
    },
    {
      id:'egy-luxor-balloon',
      title:'Hot Air Balloon over Luxor',
      location:'Luxor, Egypt',
      price:149,
      beds:1, baths:1, guests:2,
      rating:4.9,
      amenities:['Breakfast','Transport','WiFi'],
      images:['assets/photos/Hot Air Balloon over Luxor.png','assets/photos/luxor.png'],
      summary:'Sunrise balloon ride with breathtaking views of the Valley of the Kings.'
    },
    {
      id:'egy-dahab-diving',
      title:'Red Sea Diving in Dahab',
      location:'Dahab, Egypt',
      price:129,
      beds:1, baths:1, guests:2,
      rating:4.7,
      amenities:['Equipment','Guide','Water activities'],
      images:['assets/photos/Red Sea Diving in Dahab.png','assets/photos/dahab.png'],
      summary:'Guided diving experience at the Blue Hole with certified instructors.'
    },
    {
      id:'egy-sinai-bedouin',
      title:'Bedouin Night in Sinai',
      location:'South Sinai, Egypt',
      price:79,
      beds:1, baths:1, guests:4,
      rating:4.6,
      amenities:['Campfire dinner','Stargazing','Transport'],
      images:['assets/photos/Bedouin Night in Sinai.png','assets/photos/dahab.png'],
      summary:'Authentic Bedouin camp evening with traditional dinner and stargazing.'
    },
    {
      id:'egy-aswan-nile',
      title:'Nile Felucca Adventure',
      location:'Aswan, Egypt',
      price:99,
      beds:1, baths:1, guests:4,
      rating:4.7,
      amenities:['Guide','Snacks','Life jackets'],
      images:['assets/photos/aswan.png','assets/photos/siwa.png'],
      summary:'Sail the Nile on a traditional felucca at golden hour.'
    }
  ];
  try {
    const res = await fetch('assets/data/listings.json', {cache:'no-store'});
    if (!res.ok) throw new Error('bad status');
    const data = await res.json();
    if (Array.isArray(data) && data.length) return data;
    return demo;
  } catch (e) {
    // Use embedded demo when fetch is blocked (file:// or CORS) or empty
    return demo;
  }
}
function cardHtml(listing) {
  const img = listing.images?.[0] || 'assets/photos/cairo.png';
  return `
  <div class="col-md-6 col-xl-4">
  <a class="text-decoration-none text-reset" href="property.html?id=${encodeURIComponent(listing.id)}">
  <div class="experience-featured-card h-100">
  <div class="experience-featured-image">
  <img src="${img}" alt="${listing.title}" loading="lazy">
  </div>
  <div class="experience-featured-content">
  <h4 class="experience-featured-title mb-1">${listing.title}</h4>
  <p class="experience-featured-description mb-2 small text-muted">${listing.beds} beds · ${listing.baths} bath · ${listing.amenities?.slice(0,2).join(' · ') || ''}</p>
  <div class="experience-featured-footer">
  <span class="experience-featured-price">${formatCurrency(listing.price)}</span>
  <span class="btn-book-now">View</span>
  </div>
  </div>
  </div>
  </a>
  </div>
  `.trim();
}
async function renderListingsInStays() {
  const grid = document.getElementById('listingsGrid');
  if (!grid) return;
  const listings = await fetchListings();
  window._allListings = listings;
  window._currentView = 'grid';
  applyStaysFiltersAndRender();
}
// Render featured experiences on home page using same listings source
async function renderFeaturedOnHome() {
  const grid = document.getElementById('experiencesGridHome');
  if (!grid) return;
  const listings = await fetchListings();
  if (!Array.isArray(listings) || listings.length===0){
    grid.innerHTML = `<div class="col-12 text-center text-muted">Experiences will appear here once available.</div>`;
    return;
  }
  const featured = [...listings].sort((a,b)=>b.rating-a.rating).slice(0,3);
  grid.innerHTML = featured.map(cardHtml).join('');
}
function inferPropertyType(listing) {
  if (listing.beds >= 2) return 'entire';
  if (listing.beds === 1) return 'private';
  return 'shared';
}
function getStaysFiltersFromUI() {
  const where = (document.querySelector('input[name="where"]')?.value || '').trim().toLowerCase();
  const typeSelect = (document.querySelector('select[name="type"]')?.value || '').toLowerCase();
  const priceMin = parseInt((document.getElementById('priceMin')?.value || '0').replace(/[^0-9]/g,'')) || 0;
  const priceMax = parseInt((document.getElementById('priceMax')?.value || '100000').replace(/[^0-9]/g,'')) || 100000;
  const priceRange = parseInt(document.getElementById('priceRange')?.value || '0',10);
  const propEntire = document.getElementById('entirePlace')?.checked;
  const propPrivate = document.getElementById('privateRoom')?.checked;
  const propShared = document.getElementById('sharedRoom')?.checked;
  const amenities = ['wifi','kitchen','ac','parking','pool'].filter(id => document.getElementById(id)?.checked);
  let ratingMin = 0;
  if (document.getElementById('fourFive')?.checked) ratingMin = 4.5;
  else if (document.getElementById('four')?.checked) ratingMin = 4.0;
  const sortBy = document.getElementById('sortBy')?.value || 'recommended';
  return {
    where, typeSelect, priceMin, priceMax, priceRange, propEntire, propPrivate, propShared, amenities, ratingMin, sortBy
  };
}
function listingMatchesFilters(listing, f) {
  const titleLoc = `${listing.title} ${listing.location}`.toLowerCase();
  if (f.where && !titleLoc.includes(f.where)) return false;
  if (listing.price < Math.max(f.priceMin, 0)) return false;
  if (listing.price > Math.min(f.priceMax, f.priceRange || Infinity)) return false;
  const t = inferPropertyType(listing);
  if (!((f.propEntire && t==='entire') || (f.propPrivate && t==='private') || (f.propShared && t==='shared') || (!f.propEntire && !f.propPrivate && !f.propShared))) return false;
  if (f.amenities.length) {
    const a = (listing.amenities||[]).map(s=>s.toLowerCase());
    const ok = f.amenities.every(x => a.some(v=>v.includes(x)));
    if (!ok) return false;
  }
  if (listing.rating < f.ratingMin) return false;
  // typeSelect from top bar is abstract categories;
  return true;
}
function sortListings(list, key) {
  if (key === 'price-asc') return list.sort((a,b)=>a.price-b.price);
  if (key === 'price-desc') return list.sort((a,b)=>b.price-a.price);
  if (key === 'rating-desc') return list.sort((a,b)=>b.rating-a.rating);
  return list;
  // recommended: original order
}
function listItemHtml(listing) {
  const img = listing.images?.[0] || 'assets/photos/cairo.png';
  return `
  <a href="property.html?id=${encodeURIComponent(listing.id)}" class="list-group-item list-group-item-action py-3">
  <div class="d-flex w-100 align-items-center">
  <img src="${img}" class="rounded me-3" style="width:92px;
  height:72px;
  object-fit:cover;
  " alt="${listing.title}">
  <div class="flex-grow-1">
  <div class="d-flex justify-content-between">
  <h6 class="mb-1">${listing.title}</h6>
  <small class="text-muted">${formatCurrency(listing.price)} /night</small>
  </div>
  <small class="text-muted">${listing.location} · ${listing.beds} beds · ★ ${listing.baths} bath · ${listing.rating.toFixed(1)}</small>
  </div>
  </div>
  </a>`;
}
function renderListingsToView(list) {
  const grid = document.getElementById('listingsGrid');
  const listEl = document.getElementById('listingsList');
  const mapView = document.getElementById('mapView');
  if (!grid) return;
  const view = window._currentView || 'grid';
  if (view==='grid') {
    grid.style.display='flex';
    listEl.style.display='none';
    mapView.style.display='none';
    grid.innerHTML = list.map(cardHtml).join('');
  }
  else if (view==='list') {
    grid.style.display='none';
    listEl.style.display='block';
    mapView.style.display='none';
    listEl.innerHTML = list.map(listItemHtml).join('');
  }
  else {
    grid.style.display='none';
    listEl.style.display='none';
    mapView.style.display='block';
    mapView.querySelector('h5')?.scrollIntoView( {
      behavior:'smooth',block:'center'
    }
    );
  }
}
function applyStaysFiltersAndRender() {
  const all = window._allListings || [];
  const f = getStaysFiltersFromUI();
  let filtered = all.filter(l => listingMatchesFilters(l,f));
  filtered = sortListings(filtered, f.sortBy);
  renderListingsToView(filtered);
}
// Event bindings for stays page
document.addEventListener('DOMContentLoaded', () => {
  if (!document.getElementById('listingsGrid')) return;
  const reapply = debounce(applyStaysFiltersAndRender, 150);
  ['priceMin','priceMax','priceRange','entirePlace','privateRoom','sharedRoom','wifi','kitchen','ac','parking','pool','four','fourFive','any','sortBy']
  .forEach(id => document.getElementById(id)?.addEventListener('input', reapply));
  document.querySelector('form[action="stays.html"]')?.addEventListener('submit', e => {
    e.preventDefault();
    applyStaysFiltersAndRender();
  }
  );
  document.getElementById('clearFilters')?.addEventListener('click', (e)=> {
    e.preventDefault();
    ['priceMin','priceMax'].forEach(id=> {
      const el=document.getElementById(id);
      if(el) el.value='';
    }
    );
    const pr=document.getElementById('priceRange');
    if(pr) pr.value=200;
    ['entirePlace','privateRoom','sharedRoom','wifi','kitchen','ac','parking','pool'].forEach(id=> {
      const el=document.getElementById(id);
      if(el) el.checked=false;
    }
    );
    document.getElementById('any')?.click();
    applyStaysFiltersAndRender();
  }
  );
  // View toggles
  [['viewGrid','grid'],['viewList','list'],['viewMap','map']].forEach(([id,view])=> {
    document.getElementById(id)?.addEventListener('click', ()=> {
      window._currentView = view;
      ['viewGrid','viewList','viewMap'].forEach(btnId=>document.getElementById(btnId)?.classList.remove('active'));
      document.getElementById(id)?.classList.add('active');
      applyStaysFiltersAndRender();
    }
    );
  }
  );
}
);
async function populatePropertyPage() {
  const id = getQueryParam('id');
  const titleEl = document.getElementById('propTitle');
  if (!titleEl) return;
  // not on property page
  const listings = await fetchListings();
  if (!Array.isArray(listings) || listings.length===0) return;
  const item = listings.find(l => l.id === id) || listings[0];
  // Title and meta
  titleEl.textContent = item.title;
  document.getElementById('propRating').innerHTML = `<i class="bi bi-star-fill text-warning me-1"></i>${item.rating.toFixed(1)}`;
  document.getElementById('propLocation').innerHTML = `<i class="bi bi-geo-alt me-1"></i>${item.location}`;
  document.getElementById('propHosted').textContent = 'Entire place hosted by Local Superhost';
  document.getElementById('propMeta').textContent = `${item.guests} guests • ${item.beds} bedrooms • ${item.baths} baths`;
  document.getElementById('propSummary').textContent = item.summary || '';
  document.getElementById('propPrice').textContent = formatCurrency(item.price);
  // Dynamic highlights derived from amenities
  const hiWrap = document.getElementById('propHighlights');
  if (hiWrap) {
    const amen = (item.amenities || []).map(a=>a.toLowerCase());
    const map = [ {
      key:'pool', icon:'bi-water', title:'Private pool', desc:'Enjoy a refreshing swim during your stay.'
    }
    , {
      key:'kitchen', icon:'bi-egg-fried', title:'Full kitchen', desc:'Cook meals with essential appliances.'
    }
    , {
      key:'wifi', icon:'bi-wifi', title:'Fast Wi‑Fi', desc:'Reliable internet for work and streaming.'
    }
    , {
      key:'parking', icon:'bi-car-front', title:'Free parking', desc:'On-site parking for your convenience.'
    }
    , {
      key:'balcony', icon:'bi-building', title:'Balcony/terrace', desc:'Outdoor space to relax and enjoy views.'
    }
    , {
      key:'air conditioning', match:'air conditioning', icon:'bi-snow', title:'Air conditioning', desc:'Stay cool and comfortable.'
    }
    , {
      key:'ac', match:'ac', icon:'bi-snow', title:'Air conditioning', desc:'Stay cool and comfortable.'
    }
    ,
    ];
    const chosen = [];
    map.forEach(m => {
      const matchKey = (m.match || m.key).toLowerCase();
      if (amen.some(a => a.includes(matchKey)) && chosen.length < 3) chosen.push(m);
    }
    );
    if (chosen.length < 3) {
      chosen.push( {
        key:'hosted', icon:'bi-emoji-smile', title:'Dedicated host', desc:'Helpful local host for tips and support.'
      }
      );
    }
    hiWrap.innerHTML = chosen.map(m => `
    <li class="d-flex align-items-start mb-3">
    <i class="bi ${m.icon} me-3 mt-1"></i>
    <div>
    <div class="fw-semibold">${m.title}</div>
    <div class="text-muted small">${m.desc}</div>
    </div>
    </li>`).join('');
  }
  // Images
  const imgs = item.images || [];
  const setImg = (id, src) => {
    const el = document.getElementById(id);
    if (el && src) el.src = src;
  };
  setImg('propImgMain', imgs[0]);
  setImg('propImg1', imgs[1] || imgs[0]);
  setImg('propImg2', imgs[2] || imgs[0]);
  setImg('propImg3', imgs[3] || imgs[0]);
  setImg('propImg4', imgs[4] || imgs[0]);
  // Amenities
  const amenWrap = document.getElementById('propAmenities');
  if (amenWrap) {
    const allAmenities = (item.amenities || []);
    let showAll = false;
    const renderAmenities = () => {
      const list = showAll ? allAmenities : allAmenities.slice(0, 8);
      amenWrap.innerHTML = list.map(a => `<div class="col"><i class="bi bi-check2-circle me-2 text-success"></i>${a}</div>`).join('');
      const btn = document.getElementById('showAllAmenities');
      if (btn) btn.textContent = showAll ? 'Show fewer' : 'Show all amenities';
      if (btn) btn.style.display = allAmenities.length > 8 ? 'inline-block' : 'none';
    };
    renderAmenities();
    document.getElementById('showAllAmenities')?.addEventListener('click', () => {
      showAll = !showAll;
      renderAmenities();
    }
    );
  }
  // Reviews + meters (synthetic based on rating, styled with site colors)
  const setMeters = () => {
    const score = item.rating || 4.8;
    const metrics = {
      clean: Math.min(5, +(score - 0.1).toFixed(1)),
      comm: Math.min(5, +(score + 0.1 > 5 ? 5 : score + 0.1).toFixed(1)),
      checkin: Math.min(5, +(score).toFixed(1)),
      acc: Math.min(5, +(score - 0.2).toFixed(1)),
      loc: Math.min(5, +(score - 0.1).toFixed(1)),
      val: Math.min(5, +(score - 0.3).toFixed(1)),
    };
    const pct = v => `${Math.max(0, Math.min(100, (v/5)*100))}%`;
    const byId=(id)=>document.getElementById(id);
    byId('revScore') && (byId('revScore').textContent = score.toFixed(2).replace(/0$/,''));
    const reviewsCount = 120 + (Math.floor(item.price)%40);
    byId('revCount') && (byId('revCount').textContent = `${reviewsCount} reviews`);
    [['mClean','barClean',metrics.clean],['mComm','barComm',metrics.comm],['mCheckin','barCheckin',metrics.checkin],['mAcc','barAcc',metrics.acc],['mLoc','barLoc',metrics.loc],['mVal','barVal',metrics.val]].forEach(([m,b,v])=>{
      if (byId(m)) byId(m).textContent = v.toFixed(1);
      if (byId(b)) byId(b).style.width = pct(v);
    });
    // simple synthetic reviews list
    const list = document.getElementById('reviewsList');
    if (list) {
      const samples = [
        {name:'Michael Chen', when:'March 2024', text:`Fantastic stay near ${item.location}. Views were even better than the photos.`},
        {name:'Emma Rodriguez', when:'February 2024', text:`Perfect for our vacation. Clean, bright, and close to everything.`},
        {name:'David Park', when:'January 2024', text:`Loved the design and location. Great amenities and super responsive host.`},
        {name:'Sophie Williams', when:'December 2023', text:`A dream spot! We’ll definitely return to ${item.location.split(',')[0]}.`},
      ];
      list.innerHTML = samples.map((r,i)=>`
      <div class="col-md-6">
        <div class="d-flex align-items-center mb-2">
          <img src="https://i.pravatar.cc/80?img=${5+i}" class="rounded-circle me-3" width="44" height="44" alt="${r.name}">
          <div>
            <div class="fw-semibold">${r.name}</div>
            <div class="text-muted small">${r.when}</div>
          </div>
        </div>
        <p class="text-muted mb-0">${r.text}</p>
      </div>`).join('');
    }
  };
  setMeters();
  // Where you'll be section
  const placeLoc = document.getElementById('placeLocation');
  if (placeLoc) {
    placeLoc.textContent = item.location;
    // Support both an <img id="placeMap"> and a styled placeholder <div id="placeMapPlaceholder">
    const imgEl = document.getElementById('placeMap');
    if (imgEl) {
      imgEl.src = item.images?.[0] || 'assets/photos/cairo.png';
    } else {
      const ph = document.getElementById('placeMapPlaceholder');
      if (ph) {
        const url = (item.images?.[0] || 'assets/photos/cairo.png').replace(/"/g, '%22');
        ph.style.background = `center/cover no-repeat url("${url}")`;
      }
    }
    const aboutEl = document.getElementById('placeAbout');
    if (aboutEl) {
      const city = (item.location || '').split(',')[0] || 'this area';
      aboutEl.textContent = item.summary || `Located in the heart of ${city}, with easy access to local cafes, markets, and landmarks.`;
    }
  }
  // Price calc preview
  const calcLine = document.getElementById('calcLine');
  const calcSubtotal = document.getElementById('calcSubtotal');
  const updatePreview = () => {
    const ci = document.getElementById('checkin').value;
    const co = document.getElementById('checkout').value;
    const guestsSel = document.getElementById('guests');
    const guests = guestsSel ? parseInt((guestsSel.value || '2').replace(/\D/g,''),10)||2 : 2;
    const nights = (ci && co) ? Math.max(1, Math.ceil((new Date(co) - new Date(ci)) / (1000*60*60*24))) : 1;
    const base = item.price * nights * guests;
    const cleaning = 150;
    const service = Math.round(base * 0.2);
    const total = base + cleaning + service;
    calcLine.textContent = `${formatCurrency(item.price)} × ${nights} night${nights>1?'s':''} × ${guests} guest${guests>1?'s':''}`;
    calcSubtotal.textContent = formatCurrency(base);
    document.getElementById('calcCleaning').textContent = formatCurrency(cleaning);
    document.getElementById('calcService').textContent = formatCurrency(service);
    document.getElementById('calcTotal').textContent = formatCurrency(total);
  };
  updatePreview();
  document.getElementById('checkin')?.addEventListener('change', updatePreview);
  document.getElementById('checkout')?.addEventListener('change', updatePreview);
  document.getElementById('guests')?.addEventListener('change', updatePreview);
  // Book Now navigation
  const form = document.getElementById('bookingFormProperty');
  if (form) {
    const submitBtn = form.querySelector('button[type="submit"]');
    const ciInput = document.getElementById('checkin');
    const coInput = document.getElementById('checkout');
    // Keep inputs constrained so Checkout cannot be before Check-in
    const syncDateConstraints = () => {
      const today = new Date().toISOString().split('T')[0];
      // Always prevent selecting past dates
      if (ciInput) ciInput.min = today;
      // Checkout must be on/after check-in (and not before today)
      const ciVal = ciInput?.value || '';
      const minCheckout = ciVal || today;
      if (coInput) coInput.min = minCheckout;
      // Optional: keep check-in not after already chosen checkout
      const coVal = coInput?.value || '';
      if (ciInput) ciInput.max = coVal || '';
      // Auto-correct an invalid checkout picked before check-in
      if (ciVal && coVal && new Date(coVal) <= new Date(ciVal)) {
        // Set checkout to the same day as check-in plus 1 day
        const next = new Date(ciVal);
        next.setDate(next.getDate() + 1);
        const nextStr = next.toISOString().split('T')[0];
        coInput.value = nextStr;
      }
    };
    const validateDatesPresent = () => {
      const ci = ciInput.value;
      const co = coInput.value;
      const ok = ci && co && new Date(co) > new Date(ci);
      if (submitBtn) submitBtn.disabled = !ok;
    };
    syncDateConstraints();
    validateDatesPresent();
    const onDatesChange = () => {
      syncDateConstraints();
      validateDatesPresent();
    };
    ciInput?.addEventListener('change', onDatesChange);
    coInput?.addEventListener('change', onDatesChange);
    ciInput?.addEventListener('input', onDatesChange);
    coInput?.addEventListener('input', onDatesChange);
    form.addEventListener('submit', (e) => {
      e.preventDefault();
      const ci = ciInput.value;
      const co = coInput.value;
      const guests = (document.getElementById('guests').value || '2').replace(/\D/g,'');
      const url = new URL('booking.html', window.location.origin);
      url.searchParams.set('id', item.id);
      url.searchParams.set('checkin', ci);
      url.searchParams.set('checkout', co);
      url.searchParams.set('guests', guests || '2');
      window.location.href = url.toString();
    }
    );
  }
}
async function populateBookingPage() {
  const sumLine = document.getElementById('sumLine');
  if (!sumLine) return;
  // not on booking page
  const id = getQueryParam('id');
  const listings = await fetchListings();
  if (!Array.isArray(listings) || listings.length===0) return;
  const item = listings.find(l => l.id === id) || listings[0];
  const ci = getQueryParam('checkin');
  const co = getQueryParam('checkout');
  const nights = (ci && co) ? Math.max(1, Math.ceil((new Date(co) - new Date(ci)) / (1000*60*60*24))) : 1;
  const guestsInitial = parseInt(getQueryParam('guests') || '2', 10);
  const sumGuests = document.getElementById('sumGuests');
  // Populate property summary section
  const imgs = item.images || [];
  const setText = (id, html) => {
    const el = document.getElementById(id);
    if (el) el.innerHTML = html;
  };
  const setImg = (id, src) => {
    const el = document.getElementById(id);
    if (el && src) el.src = src;
  };
  setImg('sumThumb', imgs[0]);
  setText('sumTitle', item.title);
  setText('sumLocation', `<i class="bi bi-geo-alt me-1"></i>${item.location}`);
  setText('sumMeta',
  `<span><i class="bi bi-door-open me-1"></i>${item.beds} Beds</span>
  <span><i class="bi bi-droplet me-1"></i>${item.baths} Baths</span>
  <span><i class="bi bi-people me-1"></i>${item.guests} Guests</span>
  <span><i class="bi bi-star-fill text-warning me-1"></i>${item.rating.toFixed(1)}</span>`);
  setText('sumCheckin', ci ? formatDate(ci) : '—');
  setText('sumCheckout', co ? formatDate(co) : '—');
  setText('sumGuestsText', `${guestsInitial} guest${guestsInitial>1?'s':''}`);
  if (sumGuests) {
    // populate options if empty
    if (!sumGuests.options.length) {
      for (let g=1;
      g<=10;
      g++) {
        const opt = document.createElement('option');
        opt.value = String(g);
        opt.textContent = `${g} guest${g>1?'s':''}`;
        sumGuests.appendChild(opt);
      }
    }
    sumGuests.value = String(guestsInitial);
  }
  const updateTotals = () => {
    const guests = sumGuests ? parseInt(sumGuests.value,10) : guestsInitial;
    const base = item.price * nights * guests;
    const cleaning = 150;
    const service = Math.round(base * 0.2);
    // scalable service fee
    const total = base + cleaning + service;
    document.getElementById('sumLine').textContent = `${formatCurrency(item.price)} × ${nights} night${nights>1?'s':''} × ${guests} guest${guests>1?'s':''}`;
    document.getElementById('sumBase').textContent = formatCurrency(base);
    document.getElementById('sumCleaning').textContent = formatCurrency(cleaning);
    document.getElementById('sumService').textContent = formatCurrency(service);
    document.getElementById('sumTotal').textContent = formatCurrency(total);
    setText('sumGuestsText', `${guests} guest${guests>1?'s':''}`);
  };
  updateTotals();
  sumGuests?.addEventListener('change', updateTotals);
  const back = document.getElementById('backToListing');
  if (back) back.href = `property.html?id=${encodeURIComponent(item.id)}`;
  document.getElementById('confirmBtn')?.addEventListener('click', (e) => {
    e.preventDefault();
    showNotification('success', 'Your booking has been confirmed!');
    // Persist booking and navigate to dashboard
    const ciVal = getQueryParam('checkin');
    const coVal = getQueryParam('checkout');
    const guests = parseInt(getQueryParam('guests')||'2',10);
    const nights = (ciVal && coVal) ? Math.max(1, Math.ceil((new Date(coVal) - new Date(ciVal)) / (1000*60*60*24))) : 1;
    const total = (item.price||0) * nights * guests + 150 + Math.round((item.price||0) * nights * guests * 0.2);
    addBookingAndNotify({
      id:'b-'+Date.now(),
      propertyId:item.id,
      title:item.title,
      location:item.location,
      img:item.images?.[0] || 'assets/photos/cairo.png',
      checkin:ciVal, checkout:coVal, guests, total, status:'confirmed'
    });
    setTimeout(()=>{ window.location.href = 'dashboard.html'; }, 600);
  }
  );
}
function initDataDrivenPages() {
  // Render stays grid if present
  renderListingsInStays();
  // Render featured on home if present
  renderFeaturedOnHome();
  // Host dashboard if present
  initHostDashboardPage();
  // Populate property details if present
  populatePropertyPage();
  // Populate booking summary if present
  populateBookingPage();
  // Populate dashboard if present
  initDashboardPage();
  // Auth guard for dashboard visibility
  guardDashboardIfNeeded();
}
// ==========================================
// HOST DASHBOARD (listings management)
// ==========================================
const HOST_KEYS = { listings:'stayEase:host:listings' };
function getHostListings(){
  try{ return JSON.parse(localStorage.getItem(HOST_KEYS.listings)) || []; } catch { return []; }
}
function setHostListings(list){ localStorage.setItem(HOST_KEYS.listings, JSON.stringify(list)); }
function seedHostListingsIfEmpty(){
  const cur = getHostListings();
  if (cur.length) return;
  const demo = [
    {id:'hl1', title:'Downtown Luxury Apartment', beds:2, price:150, status:'active', location:'Cairo, Egypt', image:'assets/photos/alexandria.png'},
    {id:'hl2', title:'Cozy Mountain Cottage', beds:1, price:95, status:'pending', location:'South Sinai, Egypt', image:'assets/photos/dahab.png'},
    {id:'hl3', title:'Beachfront Villa', beds:4, price:280, status:'active', location:'Luxor, Egypt', image:'assets/photos/luxor.png'},
  ];
  setHostListings(demo);
}
function renderHostKpis(){
  const list = getHostListings();
  const active = list.filter(l=>l.status==='active').length;
  const bookings = 28; // demo
  const earnings = 8420; // demo
  const rating = 4.8; // demo
  const set = (id,val)=>{ const el=document.getElementById(id); if(el) el.textContent = typeof val==='number' && id!=='kpiRating' ? formatCurrency(val).replace('$','') : String(val); };
  const elListings=document.getElementById('kpiListings'); if(elListings) elListings.textContent = active;
  const elBookings=document.getElementById('kpiBookings'); if(elBookings) elBookings.textContent = bookings;
  const elEarn=document.getElementById('kpiEarnings'); if(elEarn) elEarn.textContent = formatCurrency(earnings);
  const elRating=document.getElementById('kpiRating'); if(elRating) elRating.textContent = rating.toFixed(1);
}
function listingBadge(status){
  if (status==='active') return '<span class="badge bg-success-subtle text-success">Active</span>';
  return '<span class="badge bg-warning-subtle text-warning">Pending</span>';
}
function renderHostListingsCards(){
  const wrap = document.getElementById('hostListingsList');
  if (!wrap) return;
  const list = getHostListings();
  wrap.innerHTML = list.map(l=>`
    <div class="list-group-item d-flex align-items-center">
      <img src="${l.image||'assets/photos/cairo.png'}" class="rounded me-3" style="width:64px;height:48px;object-fit:cover;" alt="${l.title}">
      <div class="flex-grow-1">
        <div class="fw-semibold">${l.title}</div>
        <div class="text-muted small">${l.beds} bedroom${l.beds>1?'s':''} • $${l.price}/night</div>
        ${listingBadge(l.status)}
      </div>
      <div class="ms-2">
        <button class="btn btn-sm btn-outline-secondary me-1" data-edit="${l.id}"><i class="bi bi-pencil-square"></i></button>
        <button class="btn btn-sm btn-outline-danger" data-del="${l.id}"><i class="bi bi-trash"></i></button>
      </div>
    </div>
  `).join('');
  wrap.querySelectorAll('[data-edit]').forEach(btn=>btn.addEventListener('click',()=>openListingModal(btn.getAttribute('data-edit'))));
  wrap.querySelectorAll('[data-del]').forEach(btn=>btn.addEventListener('click',()=>deleteHostListing(btn.getAttribute('data-del'))));
}
function renderHostListingsTable(){
  const body = document.getElementById('hostListingsTbody');
  if (!body) return;
  const list = getHostListings();
  body.innerHTML = list.map(l=>`
    <tr>
      <td class="d-flex align-items-center"><img src="${l.image||'assets/photos/cairo.png'}" class="rounded me-2" style="width:56px;height:42px;object-fit:cover;">${l.title}</td>
      <td>${l.beds}</td>
      <td>$${l.price}</td>
      <td>${listingBadge(l.status)}</td>
      <td class="text-end">
        <button class="btn btn-sm btn-outline-secondary me-1" data-edit="${l.id}"><i class="bi bi-pencil-square"></i></button>
        <button class="btn btn-sm btn-outline-danger" data-del="${l.id}"><i class="bi bi-trash"></i></button>
      </td>
    </tr>
  `).join('');
  body.querySelectorAll('[data-edit]').forEach(btn=>btn.addEventListener('click',()=>openListingModal(btn.getAttribute('data-edit'))));
  body.querySelectorAll('[data-del]').forEach(btn=>btn.addEventListener('click',()=>deleteHostListing(btn.getAttribute('data-del'))));
}
function deleteHostListing(id){
  if (!confirm('Delete this listing?')) return;
  const list = getHostListings().filter(l=>l.id!==id);
  setHostListings(list);
  refreshHostViews();
  showNotification('success','Listing deleted.');
}
function openListingModal(id=null){
  const modalEl = document.getElementById('listingModal');
  if (!modalEl || !window.bootstrap?.Modal) return;
  const title = document.getElementById('listingModalTitle');
  const idEl = document.getElementById('listingId');
  const t = document.getElementById('listingTitle');
  const beds = document.getElementById('listingBeds');
  const price = document.getElementById('listingPrice');
  const status = document.getElementById('listingStatus');
  const loc = document.getElementById('listingLocation');
  const img = document.getElementById('listingImage');
  if (id){
    const l = getHostListings().find(x=>x.id===id);
    title.textContent = 'Edit Listing';
    idEl.value = id;
    t.value = l?.title || '';
    beds.value = l?.beds || 1;
    price.value = l?.price || 100;
    status.value = l?.status || 'active';
    loc.value = l?.location || '';
    img.value = l?.image || '';
  } else {
    title.textContent = 'Create Listing';
    idEl.value = '';
    t.value = ''; beds.value = 1; price.value = 100; status.value = 'active'; loc.value=''; img.value='';
  }
  new bootstrap.Modal(modalEl).show();
}
function bindListingForm(){
  const form = document.getElementById('formListing');
  if (!form) return;
  form.addEventListener('submit',(e)=>{
    e.preventDefault();
    const id = document.getElementById('listingId').value || ('hl_'+Date.now());
    const entry = {
      id,
      title: document.getElementById('listingTitle').value.trim(),
      beds: parseInt(document.getElementById('listingBeds').value,10)||1,
      price: parseInt(document.getElementById('listingPrice').value,10)||0,
      status: document.getElementById('listingStatus').value,
      location: document.getElementById('listingLocation').value.trim(),
      image: document.getElementById('listingImage').value.trim() || 'assets/photos/cairo.png'
    };
    let list = getHostListings();
    const i = list.findIndex(l=>l.id===id);
    if (i>-1) list[i]=entry; else list.push(entry);
    setHostListings(list);
    refreshHostViews();
    bootstrap.Modal.getInstance(document.getElementById('listingModal'))?.hide();
    showNotification('success','Listing saved.');
  });
}
function refreshHostViews(){
  renderHostKpis();
  renderHostListingsCards();
  renderHostListingsTable();
}
function initHostDashboardPage(){
  const root = document.getElementById('hostRoot');
  if (!root) return;
  seedHostListingsIfEmpty();
  // Nav bindings
  const menu = document.getElementById('dashMenuHost');
  const sections = {
    dashboard: document.getElementById('secDash'),
    listings: document.getElementById('secListings'),
    bookings: document.getElementById('secBookings'),
    earnings: document.getElementById('secEarnings'),
    messages: document.getElementById('secMessages'),
    settings: document.getElementById('secSettings'),
  };
  const showSection = (key)=>{
    Object.values(sections).forEach(el=>el?.classList.add('d-none'));
    sections[key]?.classList.remove('d-none');
  };
  if (menu){
    menu.addEventListener('click',(e)=>{
      const link = e.target.closest('[data-section]');
      if(!link) return;
      e.preventDefault();
      menu.querySelectorAll('.list-group-item').forEach(i=>i.classList.remove('active'));
      link.classList.add('active');
      showSection(link.getAttribute('data-section'));
    });
  }
  // Buttons
  ['btnOpenCreateListing','btnOpenCreateListing2','btnOpenCreateListing3'].forEach(id=>{
    const b=document.getElementById(id); if(b) b.addEventListener('click',()=>openListingModal());
  });
  document.getElementById('openAccountSettings')?.addEventListener('click',()=>{
    const modalEl = document.getElementById('settingsModal');
    if (modalEl && window.bootstrap?.Modal){
      populateSettingsModal?.();
      new bootstrap.Modal(modalEl).show();
    }
  });
  // Quick actions
  document.getElementById('qaCreate')?.addEventListener('click',(e)=>{e.preventDefault();openListingModal();});
  document.getElementById('qaEarnings')?.addEventListener('click',(e)=>{e.preventDefault(); showSection('earnings');});
  document.getElementById('qaMessages')?.addEventListener('click',(e)=>{e.preventDefault(); showSection('messages');});
  document.getElementById('qaNotifs')?.addEventListener('click',(e)=>{e.preventDefault(); showNotification('info','No new notifications.');});
  bindListingForm();
  refreshHostViews();
}
const STORAGE_KEYS = {
  user:'stayEase:user',
  bookings:'stayEase:bookings',
  notifications:'stayEase:notifs',
  payments:'stayEase:payments',
  auth:'stayEase:auth',
  redirect:'stayEase:redirect'
};
function loadStore(key, fallback) {
  try {
    return JSON.parse(localStorage.getItem(key)) ?? fallback;
  } catch { return fallback; }
}
function saveStore(key, value) {
  localStorage.setItem(key, JSON.stringify(value));
}
function isLoggedIn() {
  const a = loadStore(STORAGE_KEYS.auth, {loggedIn:false});
  if (!a?.loggedIn) return false;
  const maxAge = 7*24*60*60*1000; // 7 days
  return Date.now() - (a.at||0) < maxAge;
}
function setAuth(loggedIn=true){
  saveStore(STORAGE_KEYS.auth, {loggedIn, at: Date.now()});
}
function clearAuth(){
  localStorage.removeItem(STORAGE_KEYS.auth);
}
function guardDashboardIfNeeded(){
  const root = document.getElementById('dashboardRoot');
  if (!root) return;
  // Always allow access in frontend; backend will enforce auth later
  root.classList.remove('d-none');
}
// Render user avatar pill in navbar and toggle login/signup buttons
function renderNavUserArea(){
  const area = document.getElementById('navUserArea');
  const liLogin = document.getElementById('navLoginItem');
  const liSignup = document.getElementById('navSignupItem');
  if (!area && !liLogin && !liSignup) return;
  const logged = isLoggedIn();
  if (logged){
    const user = loadStore(STORAGE_KEYS.user, {name:'Traveler', avatar:''});
    const avatar = user.avatar || 'https://i.pravatar.cc/100?img=13';
    if (area) {
      area.innerHTML = `<button class="nav-user-pill" type="button" data-bs-toggle="offcanvas" data-bs-target="#navUserDrawer" aria-controls="navUserDrawer">
        <span class="nav-user-burger"><i class="bi bi-list"></i></span>
        <img id="pillAvatar" src="${avatar}" alt="Profile">
      </button>`;
    }
    if (liLogin) liLogin.style.display = 'none';
    if (liSignup) liSignup.style.display = 'none';
    // keep drawer data fresh
    const name = (user.name||'User');
    const dn = document.getElementById('drawerName'); if (dn) dn.textContent = name;
    const da = document.getElementById('drawerAvatar'); if (da) da.src = avatar;
  } else {
    if (area) area.innerHTML = '';
    if (liLogin) liLogin.style.display = '';
    if (liSignup) liSignup.style.display = '';
  }
}

// Create auth UI slots if the current page's navbar doesn't have them
function ensureAuthUiSlots(){
  const hasArea = document.getElementById('navUserArea');
  const hasLogin = document.getElementById('navLoginItem');
  const hasSignup = document.getElementById('navSignupItem');
  if (hasArea && hasLogin && hasSignup) return;
  const rightGroup = document.querySelector('.navbar .navbar-collapse .navbar-nav.ms-auto.align-items-center') ||
                     document.querySelector('.navbar .navbar-collapse .navbar-nav.ms-auto') ||
                     document.querySelectorAll('.navbar .navbar-collapse .navbar-nav')?.[1];
  if (!rightGroup) return;
  // Avoid duplicates
  if (!hasLogin){
    const li = document.createElement('li');
    li.className = 'nav-item me-2';
    li.id = 'navLoginItem';
    li.innerHTML = `<a class="nav-link-auth" href="#" data-bs-toggle="modal" data-bs-target="#loginModal">Login</a>`;
    rightGroup.appendChild(li);
  }
  if (!hasSignup){
    const li = document.createElement('li');
    li.className = 'nav-item';
    li.id = 'navSignupItem';
    li.innerHTML = `<a class="btn btn-signup" href="#" data-bs-toggle="modal" data-bs-target="#signupModal">Sign Up</a>`;
    rightGroup.appendChild(li);
  }
  if (!hasArea){
    const li = document.createElement('li');
    li.className = 'nav-item ms-2';
    li.id = 'navUserArea';
    rightGroup.appendChild(li);
  }
}
// global logout for drawer button
window.logoutUser = function logoutUser(){
  localStorage.removeItem(STORAGE_KEYS.user);
  localStorage.removeItem(STORAGE_KEYS.bookings);
  localStorage.removeItem(STORAGE_KEYS.notifications);
  localStorage.removeItem(STORAGE_KEYS.payments);
  clearAuth();
  showNotification('success','Logged out.');
  renderNavUserArea();
  setTimeout(()=> window.location.href='index.html', 400);
};

// drawer "Account Settings" handler
document.addEventListener('DOMContentLoaded', ()=>{
  const link = document.getElementById('drawerOpenSettings');
  if (link){
    link.addEventListener('click', (e)=>{
      e.preventDefault();
      const modalEl = document.getElementById('settingsModal');
      if (modalEl && window.bootstrap?.Modal){
        populateSettingsModal?.();
        const m = new bootstrap.Modal(modalEl);
        m.show();
        const tabBtn = document.getElementById('tab-profile');
        if (tabBtn && window.bootstrap?.Tab){
          const t = new bootstrap.Tab(tabBtn);
          t.show();
        }
      } else {
        window.location.href = 'dashboard.html?open=settings';
      }
    });
  }
  // drawer open payments specifically
  const linkPayments = document.getElementById('drawerOpenPayments');
  if (linkPayments){
    linkPayments.addEventListener('click', (e)=>{
      e.preventDefault();
      const modalEl = document.getElementById('settingsModal');
      if (modalEl && window.bootstrap?.Modal){
        populateSettingsModal?.();
        const m = new bootstrap.Modal(modalEl);
        m.show();
        const tabBtn = document.getElementById('tab-payments');
        if (tabBtn && window.bootstrap?.Tab){
          const t = new bootstrap.Tab(tabBtn);
          t.show();
        }
      } else {
        window.location.href = 'dashboard.html?open=settings&tab=payments';
      }
    });
  }
  // when drawer opens, hydrate data
  const drawer = document.getElementById('navUserDrawer');
  if (drawer){
    drawer.addEventListener('show.bs.offcanvas', ()=>{
      const user = loadStore(STORAGE_KEYS.user, {name:'User', avatar:''});
      const dn = document.getElementById('drawerName'); if (dn) dn.textContent = user.name||'User';
      const da = document.getElementById('drawerAvatar'); if (da) da.src = user.avatar || 'https://i.pravatar.cc/80?img=13';
    });
  }
});
function seedIfEmpty() {
  // no-op: demo seeds removed
}
function addBookingAndNotify(b) {
  const bookings = loadStore(STORAGE_KEYS.bookings, []);
  bookings.push(b);
  saveStore(STORAGE_KEYS.bookings, bookings);
  const notifs = loadStore(STORAGE_KEYS.notifications, []);
  notifs.unshift({
    id:'n-'+Date.now(), type:'success', title:'Booking Confirmed',
    body:`Your booking at ${b.title} has been confirmed for ${formatDate(b.checkin)} – ${formatDate(b.checkout)}.`,
    time:'just now'
  });
  saveStore(STORAGE_KEYS.notifications, notifs);
}
function renderDashboardUpcoming(container, bookings) {
  const upcoming = bookings.filter(b => b.status!=='past')
    .sort((a,b)=> new Date(a.checkin)-new Date(b.checkin));
  if (!upcoming.length) {
    container.innerHTML = '<div class="text-muted small">No upcoming bookings yet.</div>';
    return;
  }
  container.innerHTML = upcoming.map((b,i)=>`
    <div class="booking-item d-md-flex align-items-center ${i>0?'mt-3 pt-3 border-top':''}">
      <img class="booking-image rounded" src="${b.img}" alt="${b.title}" loading="lazy">
      <div class="flex-grow-1 ms-md-3 mt-3 mt-md-0">
        <div class="d-flex justify-content-between">
          <div>
            <h6 class="mb-1">${b.title}</h6>
            <div class="text-muted small">
              <i class="bi bi-geo-alt me-1"></i> ${b.location}
              <span class="ms-3"><i class="bi bi-calendar3 me-1"></i>${formatDate(b.checkin)} – ${formatDate(b.checkout)}</span>
              <span class="ms-3"><i class="bi bi-people me-1"></i>${b.guests} Guests</span>
            </div>
          </div>
          <span class="badge status-badge ${b.status==='confirmed'?'success':'warning'} align-self-start">${b.status==='confirmed'?'Confirmed':'Pending'}</span>
        </div>
        <div class="d-flex justify-content-between align-items-center mt-3">
          <div class="fw-semibold fs-6">${formatCurrency(b.total)} <small class="text-muted">/ total</small></div>
          <div>
            <a href="property.html?id=${encodeURIComponent(b.propertyId||'')}" class="btn btn-outline-secondary btn-sm me-2">View Details</a>
            <button class="btn btn-outline-danger btn-sm" data-cancel="${b.id}">Cancel</button>
          </div>
        </div>
      </div>
    </div>
  `).join('');
  container.querySelectorAll('[data-cancel]').forEach(btn=>{
    btn.addEventListener('click',()=>{
      const id = btn.getAttribute('data-cancel');
      const list = loadStore(STORAGE_KEYS.bookings,[]);
      const i = list.findIndex(x=>x.id===id);
      if (i>-1) {
        list.splice(i,1);
        saveStore(STORAGE_KEYS.bookings,list);
        showNotification('success','Booking cancelled.');
        renderDashboard(); // refresh
      }
    });
  });
}
function renderDashboardPast(container, bookings) {
  const past = bookings.filter(b => b.status==='past' || new Date(b.checkout) < new Date());
  container.innerHTML = past.slice(0,4).map(b=>`
    <div class="col-md-6">
      <div class="past-card">
        <img src="${b.img}" alt="${b.title}">
        <div class="past-content">
          <h6 class="mb-1">${b.title}</h6>
          <div class="text-muted small mb-2">
            <i class="bi bi-geo-alt me-1"></i> ${b.location} · ${formatDate(b.checkin)} – ${formatDate(b.checkout)}
          </div>
          <div class="d-flex justify-content-between align-items-center">
            <span class="text-muted small">No review yet</span>
            <a href="#" class="btn btn-outline-primary btn-sm">Leave Review</a>
          </div>
        </div>
      </div>
    </div>
  `).join('');
}
function renderDashboardNotifs(container, notifs) {
  container.innerHTML = notifs.slice(0,5).map(n=>`
    <div class="notif-item">
      <span class="notif-icon ${n.type==='success'?'success':n.type==='warning'?'warning':'info'}"><i class="bi ${n.type==='success'?'bi-check2-circle':n.type==='warning'?'bi-hourglass-split':'bi-chat-dots'}"></i></span>
      <div class="ms-3">
        <div class="fw-semibold">${n.title}</div>
        <div class="text-muted small">${n.body}</div>
      </div>
      <span class="ms-auto text-muted small">${n.time||''}</span>
    </div>
  `).join('');
}
function renderDashboard() {
  const up = document.getElementById('dashUpcoming');
  const past = document.getElementById('dashPast');
  const notif = document.getElementById('dashNotifs');
  if (!up && !past && !notif) return; // not on dashboard
  const bookings = loadStore(STORAGE_KEYS.bookings,[]);
  const notifs = loadStore(STORAGE_KEYS.notifications,[]);
  if (up) renderDashboardUpcoming(up, bookings);
  if (past) renderDashboardPast(past, bookings);
  if (notif) renderDashboardNotifs(notif, notifs);
  const nameEl = document.getElementById('dashUserName');
  const user = loadStore(STORAGE_KEYS.user, {name:'John Smith', role:'Traveler'});
  if (nameEl) nameEl.textContent = user.name || 'John Smith';
  const roleEl = document.getElementById('dashUserRole');
  if (roleEl) roleEl.textContent = user.role || 'Traveler';
  const welcomeEl = document.getElementById('dashWelcomeName');
  if (welcomeEl) {
    const first = (user.name||'John').split(' ')[0];
    welcomeEl.textContent = first;
  }
}
function initDashboardPage() { renderDashboard(); }

// ==========================================
// DASHBOARD SIDEBAR INTERACTIONS
// ==========================================
document.addEventListener('DOMContentLoaded', () => {
  // deep-link to settings from ?open=settings
  const params = new URLSearchParams(window.location.search);
  if (params.get('open') === 'settings') {
    const modalEl = document.getElementById('settingsModal');
    if (modalEl && window.bootstrap?.Modal){
      populateSettingsModal?.();
      const m = new bootstrap.Modal(modalEl);
      m.show();
      const tab = params.get('tab');
      if (tab && window.bootstrap?.Tab){
        const btn = document.getElementById(`tab-${tab}`);
        if (btn){
          const t = new bootstrap.Tab(btn);
          t.show();
        }
      }
    }
  }
  const menu = document.getElementById('dashMenu');
  if (!menu) return; // not on dashboard
  const items = Array.from(menu.querySelectorAll('.list-group-item'));
  const setActive = (el) => {
    items.forEach(i=>i.classList.remove('active'));
    el.classList.add('active');
  };
  const scrollToEl = (id) => {
    const el = document.getElementById(id);
    if (!el) return;
    window.scrollTo({
      top: el.getBoundingClientRect().top + window.scrollY - 90,
      behavior:'smooth'
    });
  };
  menu.addEventListener('click', (e) => {
    const link = e.target.closest('.list-group-item');
    if (!link) return;
    const section = link.getAttribute('data-section');
    if (!section) return;
    e.preventDefault();
    setActive(link);
    switch(section){
      case 'top':
        window.scrollTo({top:0, behavior:'smooth'});
        break;
      case 'bookings':
        scrollToEl('secUpcoming');
        break;
      case 'notifications':
        scrollToEl('secNotifications');
        break;
      case 'wishlist':
        // Navigate to listings with a "wish" view or show a placeholder
        window.location.href = 'stays.html';
        break;
      case 'account':
        // Open account settings modal
        seedIfEmpty();
        populateSettingsModal();
        const settingsModal = document.getElementById('settingsModal');
        if (settingsModal && window.bootstrap?.Modal) new bootstrap.Modal(settingsModal).show();
        break;
      case 'payments':
        // Open settings modal focused on Payments tab
        seedIfEmpty();
        populateSettingsModal();
        const mEl = document.getElementById('settingsModal');
        if (mEl && window.bootstrap?.Modal){
          const m = new bootstrap.Modal(mEl);
          m.show();
          const tabBtn = document.getElementById('tab-payments');
          if (tabBtn && window.bootstrap?.Tab){
            const t = new bootstrap.Tab(tabBtn);
            t.show();
          }
        }
        break;
    }
  });
});

// ==========================================
// ACCOUNT SETTINGS MODAL LOGIC
// ==========================================
function maskCard(num){
  const clean = String(num).replace(/\D/g,'');
  return '•••• •••• •••• ' + clean.slice(-4);
}
function populateSettingsModal(){
  const user = loadStore(STORAGE_KEYS.user, {name:'Traveler', email:'', avatar:''});
  const nameEl = document.getElementById('settingsName');
  const emailEl = document.getElementById('settingsEmail');
  const avatarPrev = document.getElementById('settingsAvatarPreview');
  if (nameEl) nameEl.value = user.name || '';
  if (emailEl) emailEl.value = user.email || '';
  if (avatarPrev) avatarPrev.src = user.avatar || avatarPrev.src;
  // payments list
  renderPaymentsList();
}
function renderPaymentFields(method){
  const container = document.getElementById('dynamicPaymentFields');
  if (!container) return;
  const htmlByMethod = {
    card: `
      <div class="row g-2">
        <div class="col-12">
          <label class="form-label">Cardholder name</label>
          <input type="text" id="cardName" class="form-control" required>
        </div>
        <div class="col-12">
          <label class="form-label">Card number</label>
          <input type="text" id="cardNumber" class="form-control" inputmode="numeric" pattern="[0-9 ]{12,23}" placeholder="4242 4242 4242 4242" required>
        </div>
        <div class="col-6">
          <label class="form-label">Expiry (MM/YY)</label>
          <input type="text" id="cardExpiry" class="form-control" placeholder="12/29" required>
        </div>
        <div class="col-6">
          <label class="form-label">CVC</label>
          <input type="text" id="cardCvc" class="form-control" inputmode="numeric" pattern="\\d{3,4}" placeholder="123" required>
        </div>
        <div class="col-12">
          <label class="form-label">Brand</label>
          <select id="cardBrand" class="form-select">
            <option>Visa</option>
            <option>Mastercard</option>
            <option>Amex</option>
          </select>
        </div>
      </div>`,
    paypal: `
      <div class="row g-2">
        <div class="col-12">
          <label class="form-label">PayPal email</label>
          <input type="email" id="ppEmail" class="form-control" placeholder="you@example.com" required>
        </div>
        <div class="col-12">
          <label class="form-label">PayPal password</label>
          <input type="password" id="ppPassword" class="form-control" minlength="6" required>
        </div>
        <div class="col-12 text-muted small">We’ll securely link your PayPal for future payments.</div>
      </div>`,
    applePay: `
      <div class="row g-2">
        <div class="col-12 text-muted small">Authenticate with Apple Pay on your device during checkout. No setup required here.</div>
      </div>`,
    googlePay: `
      <div class="row g-2">
        <div class="col-12 text-muted small">Authenticate with Google Pay on your device during checkout. No setup required here.</div>
      </div>`,
    bank: `
      <div class="row g-2">
        <div class="col-12">
          <label class="form-label">Bank name</label>
          <input type="text" id="bankName" class="form-control" placeholder="Your Bank" required>
        </div>
        <div class="col-12">
          <label class="form-label">Account holder</label>
          <input type="text" id="bankHolder" class="form-control" placeholder="Full name" required>
        </div>
        <div class="col-12">
          <label class="form-label">IBAN / Account number</label>
          <input type="text" id="bankIban" class="form-control" placeholder="EG12 3456 7890 1234 5678 9012 3456" required>
        </div>
        <div class="col-12 text-muted small">We will show transfer instructions with a unique reference on booking.</div>
      </div>`,
    cash: `
      <div class="row g-2">
        <div class="col-12">
          <label class="form-label">Contact phone (optional)</label>
          <input type="tel" id="cashPhone" class="form-control" placeholder="+20 10 1234 5678">
        </div>
        <div class="col-12 text-muted small">You’ll pay the host in cash at check-in.</div>
      </div>`
  };
  container.innerHTML = htmlByMethod[method] || '';
}
function renderPaymentsList(){
  const listEl = document.getElementById('paymentsList');
  if (!listEl) return;
  const methods = loadStore(STORAGE_KEYS.payments, []);
  if (!methods.length){
    listEl.innerHTML = '<div class="text-muted small">No payment methods saved.</div>';
    return;
  }
  const iconFor = (m)=>{
    if (m.method==='card') return '<i class="bi bi-credit-card-2-front"></i>';
    if (m.method==='paypal') return '<span class="pay-logo pp">PP</span>';
    if (m.method==='applePay') return '<span class="pay-logo ap"></span>';
    if (m.method==='googlePay') return '<span class="pay-logo gp">G</span>';
    if (m.method==='bank') return '<span class="pay-logo bank"><i class="bi bi-bank2"></i></span>';
    if (m.method==='cash') return '<span class="pay-logo cash"><i class="bi bi-cash-coin"></i></span>';
    return '<i class="bi bi-wallet2"></i>';
  };
  const titleFor = (m)=>{
    if (m.method==='card') return `${m.brand} • ${m.last4}`;
    if (m.method==='paypal') return 'PayPal Account';
    if (m.method==='applePay') return 'Apple Pay';
    if (m.method==='googlePay') return 'Google Pay';
    if (m.method==='bank') return 'Bank Transfer';
    if (m.method==='cash') return 'Cash on Arrival';
    return 'Payment Method';
  };
  const subFor = (m)=>{
    if (m.method==='card') return `${m.name} · exp ${m.expiry}`;
    if (m.method==='paypal') return m.email ? m.email : 'Linked wallet';
    if (m.method==='applePay' || m.method==='googlePay') return 'Linked wallet';
    if (m.method==='bank') return m.bankName ? `${m.bankName} · ${m.accountLast4?('••••'+m.accountLast4):''}` : 'Manual transfer';
    if (m.method==='cash') return 'Pay at property';
    return '';
  };
  listEl.innerHTML = methods.map(c=>`
    <div class="list-group-item d-flex align-items-center gap-2">
      <div class="me-2 d-inline-flex align-items-center">${iconFor(c)}</div>
      <div class="flex-grow-1">
        <div class="small fw-semibold">${titleFor(c)} ${c.isDefault?'<span class="badge bg-success ms-2">Default</span>':''}</div>
        <div class="text-muted small">${subFor(c)}</div>
      </div>
      <div class="ms-2">
        ${c.isDefault ? '' : `<button class="btn btn-sm btn-outline-primary" data-set-default="${c.id}">Set default</button>`}
        <button class="btn btn-sm btn-outline-danger ms-1" data-del-card="${c.id}">Delete</button>
      </div>
    </div>
  `).join('');
  // bind actions
  listEl.querySelectorAll('[data-set-default]').forEach(btn=>{
    btn.addEventListener('click',()=>{
      const id = btn.getAttribute('data-set-default');
      const all = loadStore(STORAGE_KEYS.payments, []);
      all.forEach(c=>c.isDefault = (c.id===id));
      saveStore(STORAGE_KEYS.payments, all);
      showNotification('success','Default payment method updated.');
      renderPaymentsList();
    });
  });
  listEl.querySelectorAll('[data-del-card]').forEach(btn=>{
    btn.addEventListener('click',()=>{
      const id = btn.getAttribute('data-del-card');
      let all = loadStore(STORAGE_KEYS.payments, []);
      all = all.filter(c=>c.id!==id);
      if (all.length && !all.some(c=>c.isDefault)) all[0].isDefault = true;
      saveStore(STORAGE_KEYS.payments, all);
      showNotification('success','Payment method removed.');
      renderPaymentsList();
    });
  });
}
// Profile form
document.addEventListener('DOMContentLoaded', ()=>{
  // helper: render details for advanced methods grid
  const renderMethodDetails = (method)=>{
    const wrap = document.getElementById('paymentMethodDetails');
    if (!wrap) return;
    const blocks = {
      paypal: `
        <div class="card border-0 shadow-sm">
          <div class="card-body">
            <div class="d-flex align-items-center mb-2 gap-2"><span class="pay-logo pp">PP</span><strong>PayPal</strong></div>
            <div class="row g-2">
              <div class="col-12">
                <label class="form-label">PayPal email</label>
                <input type="email" class="form-control" placeholder="you@example.com">
              </div>
              <div class="col-12">
                <label class="form-label">PayPal password</label>
                <input type="password" class="form-control" placeholder="••••••••">
              </div>
              <div class="col-12 text-muted small">We’ll securely link your PayPal for future payments.</div>
            </div>
          </div>
        </div>`,
      applePay: `
        <div class="card border-0 shadow-sm">
          <div class="card-body">
            <div class="d-flex align-items-center mb-2 gap-2"><span class="pay-logo ap"></span><strong>Apple Pay</strong></div>
            <div class="text-muted small">Authenticate with Apple Pay on your device during checkout. No setup is required here.</div>
          </div>
        </div>`,
      googlePay: `
        <div class="card border-0 shadow-sm">
          <div class="card-body">
            <div class="d-flex align-items-center mb-2 gap-2"><span class="pay-logo gp">G</span><strong>Google Pay</strong></div>
            <div class="text-muted small">Authenticate with Google Pay on your device during checkout. No setup is required here.</div>
          </div>
        </div>`,
      bank: `
        <div class="card border-0 shadow-sm">
          <div class="card-body">
            <div class="d-flex align-items-center mb-2 gap-2"><span class="pay-logo bank"><i class="bi bi-bank2"></i></span><strong>Bank Transfer</strong></div>
            <div class="row g-2">
              <div class="col-12">
                <label class="form-label">Bank name</label>
                <input type="text" class="form-control" placeholder="Your Bank">
              </div>
              <div class="col-12">
                <label class="form-label">Account holder</label>
                <input type="text" class="form-control" placeholder="Full name">
              </div>
              <div class="col-12">
                <label class="form-label">IBAN / Account number</label>
                <input type="text" class="form-control" placeholder="EG12 3456 7890 1234 5678 9012 3456">
              </div>
              <div class="col-12 text-muted small">On booking, you’ll receive transfer instructions with a unique reference.</div>
            </div>
          </div>
        </div>`,
      cash: `
        <div class="card border-0 shadow-sm">
          <div class="card-body">
            <div class="d-flex align-items-center mb-2 gap-2"><span class="pay-logo cash"><i class="bi bi-cash-coin"></i></span><strong>Cash on Arrival</strong></div>
            <div class="row g-2">
              <div class="col-12">
                <label class="form-label">Contact phone (optional)</label>
                <input type="tel" class="form-control" placeholder="+20 10 1234 5678">
              </div>
              <div class="col-12 text-muted small">You’ll pay the host in cash at check-in.</div>
            </div>
          </div>
        </div>`
    };
    wrap.innerHTML = blocks[method] || '';
    wrap.scrollIntoView({behavior:'smooth', block:'start'});
  };
  const profileForm = document.getElementById('formProfile');
  if (profileForm){
    profileForm.addEventListener('submit', (e)=>{
      e.preventDefault();
      const name = document.getElementById('settingsName').value.trim();
      const email = document.getElementById('settingsEmail').value.trim();
      const user = loadStore(STORAGE_KEYS.user, {});
      saveStore(STORAGE_KEYS.user, {...user, name, email});
      document.getElementById('dashUserName') && (document.getElementById('dashUserName').textContent = name);
      const welcomeEl = document.getElementById('dashWelcomeName');
      if (welcomeEl) {
        const first = (name||'').split(' ')[0] || 'Traveler';
        welcomeEl.textContent = first;
      }
      const drawerName = document.getElementById('drawerName');
      if (drawerName) drawerName.textContent = name || 'User';
      showNotification('success','Profile updated.');
    });
    // avatar upload
    const avatarInput = document.getElementById('avatarInput');
    if (avatarInput){
      avatarInput.addEventListener('change', ()=>{
        const file = avatarInput.files?.[0];
        if (!file) return;
        if (file.size > 2*1024*1024){ showNotification('error','Image larger than 2MB.'); return; }
        const reader = new FileReader();
        reader.onload = ()=> {
          const data = reader.result;
          const user = loadStore(STORAGE_KEYS.user, {});
          saveStore(STORAGE_KEYS.user, {...user, avatar:data});
          const prev = document.getElementById('settingsAvatarPreview');
          if (prev) prev.src = data;
          showNotification('success','Profile picture updated.');
        };
        reader.readAsDataURL(file);
      });
    }
  }
  // security
  const secForm = document.getElementById('formSecurity');
  if (secForm){
    secForm.addEventListener('submit',(e)=>{
      e.preventDefault();
      const np = (document.getElementById('newPassword').value || '').trim();
      const user = loadStore(STORAGE_KEYS.user, {});
      saveStore(STORAGE_KEYS.user, {...user, password: np});
      showNotification('success','Password updated.');
      secForm.reset();
    });
  }
  // add payment
  const addPayForm = document.getElementById('formAddPayment');
  if (addPayForm){
    // initialize dynamic fields
    const methodSel = document.getElementById('paymentMethod');
    if (methodSel){
      renderPaymentFields(methodSel.value || 'card');
      methodSel.addEventListener('change', ()=>{
        renderPaymentFields(methodSel.value || 'card');
      });
    }
    addPayForm.addEventListener('submit',(e)=>{
      e.preventDefault();
      const methods = loadStore(STORAGE_KEYS.payments, []);
      const isDefault = !methods.length;
      const selected = (document.getElementById('paymentMethod')?.value) || 'card';
      let entry = null;
      if (selected === 'card'){
        const name = document.getElementById('cardName')?.value.trim() || '';
        const number = (document.getElementById('cardNumber')?.value || '').replace(/\D/g,'');
        const expiry = document.getElementById('cardExpiry')?.value.trim() || '';
        const brand = document.getElementById('cardBrand')?.value || 'Card';
        const last4 = number.slice(-4) || '0000';
        entry = { id:'pm_'+Date.now(), method:'card', brand, last4, name, expiry, isDefault };
      } else if (selected === 'paypal'){
        const email = document.getElementById('ppEmail')?.value.trim() || '';
        entry = { id:'pm_'+Date.now(), method:'paypal', email, isDefault };
      } else if (selected === 'applePay'){
        entry = { id:'pm_'+Date.now(), method:'applePay', isDefault };
      } else if (selected === 'googlePay'){
        entry = { id:'pm_'+Date.now(), method:'googlePay', isDefault };
      } else if (selected === 'bank'){
        const bankName = document.getElementById('bankName')?.value.trim() || '';
        const holder = document.getElementById('bankHolder')?.value.trim() || '';
        const iban = document.getElementById('bankIban')?.value.trim() || '';
        const accountLast4 = iban.replace(/\s/g,'').slice(-4) || '0000';
        entry = { id:'pm_'+Date.now(), method:'bank', bankName, holder, accountLast4, isDefault };
      } else if (selected === 'cash'){
        const phone = document.getElementById('cashPhone')?.value.trim() || '';
        entry = { id:'pm_'+Date.now(), method:'cash', phone, isDefault };
      }
      if (!entry){ showNotification('error','Select a payment method.'); return; }
      methods.push(entry);
      saveStore(STORAGE_KEYS.payments, methods);
      renderPaymentsList();
      if (document.getElementById('paymentMethod')){
        document.getElementById('paymentMethod').value = 'card';
        renderPaymentFields('card');
      }
      showNotification('success','Payment method added.');
    });
  }
  // Advanced payment method buttons
  document.querySelectorAll('[data-add-pay]').forEach(btn=>{
    btn.addEventListener('click', ()=>{
      const method = btn.getAttribute('data-add-pay') || 'paypal';
      // If the dynamic form exists (older UI), prefer that; otherwise render inline details
      if (typeof renderPaymentFields === 'function' && document.getElementById('paymentMethod')){
        const methodSel = document.getElementById('paymentMethod');
        methodSel.value = method;
        renderPaymentFields(method);
        methodSel.scrollIntoView({behavior:'smooth', block:'center'});
      } else {
        // New simplified UI: show method constraints/details panel
        renderMethodDetails(method);
      }
    });
  });
  // logout/delete
  const btnLogout = document.getElementById('btnLogout');
  if (btnLogout){
    btnLogout.addEventListener('click', ()=>{
      localStorage.removeItem(STORAGE_KEYS.user);
      clearAuth();
      showNotification('success','Logged out.');
      renderNavUserArea();
      setTimeout(()=> window.location.href='index.html', 600);
    });
  }
  const btnDelete = document.getElementById('btnDeleteAccount');
  if (btnDelete){
    btnDelete.addEventListener('click', ()=>{
      if (!confirm('Delete your account and all data? This cannot be undone.')) return;
      localStorage.removeItem(STORAGE_KEYS.user);
      localStorage.removeItem(STORAGE_KEYS.bookings);
      localStorage.removeItem(STORAGE_KEYS.notifications);
      localStorage.removeItem(STORAGE_KEYS.payments);
      clearAuth();
      showNotification('success','Account deleted.');
      renderNavUserArea();
      setTimeout(()=> window.location.href='index.html', 800);
    });
  }
});
