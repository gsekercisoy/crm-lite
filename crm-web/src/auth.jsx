import React, { createContext, useContext, useState } from 'react';

const AuthCtx = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem('crm_token'));
  const [email, setEmail] = useState(() => localStorage.getItem('crm_email') || '');

  function save(t, e) {
    setToken(t);
    setEmail(e || '');
    if (t) localStorage.setItem('crm_token', t);
    else localStorage.removeItem('crm_token');
    if (e) localStorage.setItem('crm_email', e);
    else localStorage.removeItem('crm_email');
  }

  function logout() { save(null, ''); }

  return <AuthCtx.Provider value={{ token, email, save, logout }}>{children}</AuthCtx.Provider>;
}

export function useAuth() {
  return useContext(AuthCtx);
}
