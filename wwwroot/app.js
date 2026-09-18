const state = {
  token: localStorage.getItem('token') || '',
  user: JSON.parse(localStorage.getItem('user') || 'null')
};

const api = {
  authRegister: '/api/auth/register',
  authLogin: '/api/auth/login',
  tickets: '/api/tickets',
  dashboard: '/api/dashboard/summary'
};

function showAuth() {
  document.getElementById('authSection').classList.remove('hidden');
  document.getElementById('dashboardSection').classList.add('hidden');
}

function showDashboard() {
  document.getElementById('authSection').classList.add('hidden');
  document.getElementById('dashboardSection').classList.remove('hidden');
}

function updateUserUI() {
  const userName = document.getElementById('userName');
  const logoutBtn = document.getElementById('logoutBtn');

  if (state.user) {
    userName.textContent = state.user.fullName || state.user.email;
    logoutBtn.classList.remove('hidden');
    showDashboard();
  } else {
    userName.textContent = 'Misafir';
    logoutBtn.classList.add('hidden');
    showAuth();
  }
}

async function apiFetch(url, options = {}) {
  const headers = { ...(options.headers || {}) };
  if (state.token) {
    headers.Authorization = `Bearer ${state.token}`;
  }
  if (options.body && !headers['Content-Type']) {
    headers['Content-Type'] = 'application/json';
  }

  const response = await fetch(url, { ...options, headers });
  const text = await response.text();
  let payload = null;
  try { payload = text ? JSON.parse(text) : null; } catch {}

  if (!response.ok) {
    throw new Error(payload?.message || payload?.title || 'İstek başarısız oldu.');
  }

  return payload;
}

async function loadSummary() {
  try {
    const data = await apiFetch(api.dashboard);
    document.getElementById('totalCount').textContent = data.total ?? 0;
    document.getElementById('openCount').textContent = data.open ?? 0;
    document.getElementById('assignedCount').textContent = data.assigned ?? 0;
    document.getElementById('resolvedCount').textContent = data.resolved ?? 0;
  } catch (error) {
    console.error(error);
  }
}

async function loadTickets() {
  const status = document.getElementById('statusFilter').value;
  const params = new URLSearchParams();
  if (status) params.set('status', status);

  try {
    const data = await apiFetch(`${api.tickets}?${params.toString()}`);
    const list = document.getElementById('ticketList');
    list.innerHTML = '';

    if (!data || data.length === 0) {
      list.innerHTML = '<p>Gösterilecek talep bulunmuyor.</p>';
      return;
    }

    data.forEach(ticket => {
      const item = document.createElement('div');
      item.className = 'ticket-item';
      item.innerHTML = `
        <div><strong>#${ticket.id}</strong> - ${ticket.title}</div>
        <div>${ticket.description}</div>
        <div style="margin-top:8px;">
          <span class="badge">${ticket.status}</span>
          <span class="badge">${ticket.priority}</span>
          <span class="badge">${ticket.category}</span>
        </div>
      `;
      list.appendChild(item);
    });
  } catch (error) {
    console.error(error);
  }
}

function bindRegister() {
  document.getElementById('registerForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const payload = {
      fullName: document.getElementById('registerName').value,
      email: document.getElementById('registerEmail').value,
      password: document.getElementById('registerPassword').value
    };

    try {
      await apiFetch(api.authRegister, { method: 'POST', body: JSON.stringify(payload) });
      alert('Kayıt başarılı. Giriş yapabilirsiniz.');
      document.getElementById('registerForm').reset();
    } catch (error) {
      alert(error.message);
    }
  });
}

function bindLogin() {
  document.getElementById('loginForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const payload = {
      email: document.getElementById('loginEmail').value,
      password: document.getElementById('loginPassword').value
    };

    try {
      const data = await apiFetch(api.authLogin, { method: 'POST', body: JSON.stringify(payload) });
      state.token = data.token;
      state.user = data.user;
      localStorage.setItem('token', state.token);
      localStorage.setItem('user', JSON.stringify(state.user));
      updateUserUI();
      await loadSummary();
      await loadTickets();
      document.getElementById('loginForm').reset();
    } catch (error) {
      alert(error.message);
    }
  });
}

function bindTicketCreate() {
  document.getElementById('ticketForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const payload = {
      title: document.getElementById('ticketTitle').value,
      description: document.getElementById('ticketDescription').value,
      categoryId: Number(document.getElementById('ticketCategory').value),
      priority: document.getElementById('ticketPriority').value,
      deviceSerialNumber: document.getElementById('ticketDevice').value || null
    };

    try {
      await apiFetch(api.tickets, { method: 'POST', body: JSON.stringify(payload) });
      alert('Talep oluşturuldu.');
      document.getElementById('ticketForm').reset();
      await loadSummary();
      await loadTickets();
    } catch (error) {
      alert(error.message);
    }
  });
}

function bindLogout() {
  document.getElementById('logoutBtn').addEventListener('click', () => {
    state.token = '';
    state.user = null;
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    updateUserUI();
  });
}

function bindRefresh() {
  document.getElementById('refreshBtn').addEventListener('click', async () => {
    await loadSummary();
    await loadTickets();
  });
}

document.getElementById('statusFilter').addEventListener('change', loadTickets);

bindRegister();
bindLogin();
bindTicketCreate();
bindLogout();
bindRefresh();
updateUserUI();
