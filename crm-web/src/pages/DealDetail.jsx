import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import api from '../api.js';

export default function DealDetail() {
  const { id } = useParams();
  const [deal, setDeal] = useState(null);
  const [activities, setActivities] = useState([]);
  const [form, setForm] = useState({ kind: 'Note', text: '' });

  async function load() {
    const d = await api.get(`/deals/${id}`);
    setDeal(d.data);
    const a = await api.get(`/deals/${id}/activities`);
    setActivities(a.data);
  }
  useEffect(() => { load(); }, [id]);

  async function add(e) {
    e.preventDefault();
    await api.post(`/deals/${id}/activities`, form);
    setForm({ kind: 'Note', text: '' });
    load();
  }

  if (!deal) return <p>Loading…</p>;
  return (
    <>
      <div className="card">
        <h2>{deal.title}</h2>
        <p>Stage: {deal.stage} · Value: ${deal.value}</p>
      </div>
      <div className="card">
        <h3>Add activity</h3>
        <form onSubmit={add} className="row">
          <select value={form.kind} onChange={(e) => setForm({ ...form, kind: e.target.value })}>
            <option>Call</option><option>Meeting</option><option>Email</option><option>Note</option>
          </select>
          <input placeholder="Text" value={form.text} onChange={(e) => setForm({ ...form, text: e.target.value })} />
          <button type="submit">Add</button>
        </form>
      </div>
      <div className="card">
        <h3>Activities ({activities.length})</h3>
        {activities.map((a) => (
          <div key={a.id}>[{a.kind}] {a.text}</div>
        ))}
      </div>
    </>
  );
}
