import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../api.js';

export default function Customers() {
  const [items, setItems] = useState([]);
  const [form, setForm] = useState({ name: '', email: '', phone: '', company: '' });

  async function load() {
    const res = await api.get('/customers');
    setItems(res.data);
  }
  useEffect(() => { load(); }, []);

  async function create(e) {
    e.preventDefault();
    await api.post('/customers', form);
    setForm({ name: '', email: '', phone: '', company: '' });
    load();
  }

  return (
    <>
      <div className="card">
        <h2>New customer</h2>
        <form onSubmit={create} className="row">
          <input placeholder="Name*" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          <input placeholder="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
          <input placeholder="Phone" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
          <input placeholder="Company" value={form.company} onChange={(e) => setForm({ ...form, company: e.target.value })} />
          <button type="submit">Add</button>
        </form>
      </div>
      <div className="card">
        <h2>Customers ({items.length})</h2>
        {items.map((c) => (
          <div key={c.id}>
            <Link to={`/customers/${c.id}`}>{c.name}</Link> — {c.company} {c.email}
          </div>
        ))}
      </div>
    </>
  );
}
