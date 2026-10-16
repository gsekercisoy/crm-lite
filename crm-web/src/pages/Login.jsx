import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api.js';
import { useAuth } from '../auth.jsx';

export default function Login() {
  const [email, setEmail] = useState('admin@crm.local');
  const [password, setPassword] = useState('Pass123!');
  const [mode, setMode] = useState('login');
  const [error, setError] = useState('');
  const { save } = useAuth();
  const navigate = useNavigate();

  async function submit(e) {
    e.preventDefault();
    setError('');
    try {
      const url = mode === 'login' ? '/auth/login' : '/auth/register';
      const body = mode === 'login' ? { email, password } : { email, password, role: 'Sales' };
      const res = await api.post(url, body);
      save(res.data.token, res.data.email);
      navigate('/pipeline');
    } catch (err) {
      setError(err.response?.data?.error || 'Request failed');
    }
  }

  return (
    <div className="card" style={{ maxWidth: 400 }}>
      <h2>{mode === 'login' ? 'Login' : 'Register'}</h2>
      <form onSubmit={submit}>
        <input placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} style={{ width: '100%' }} />
        <input placeholder="Password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} style={{ width: '100%' }} />
        <div className="row">
          <button type="submit">{mode === 'login' ? 'Login' : 'Register'}</button>
          <button type="button" onClick={() => setMode(mode === 'login' ? 'register' : 'login')}>
            Switch to {mode === 'login' ? 'Register' : 'Login'}
          </button>
        </div>
      </form>
      {error && <p className="error">{error}</p>}
    </div>
  );
}
