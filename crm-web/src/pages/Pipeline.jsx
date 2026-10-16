import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../api.js';

const STAGES = ['Lead', 'Qualified', 'Proposal', 'Won', 'Lost'];

export default function Pipeline() {
  const [deals, setDeals] = useState([]);
  const [summary, setSummary] = useState(null);
  const [error, setError] = useState('');

  async function load() {
    const [d, s] = await Promise.all([api.get('/deals'), api.get('/pipeline')]);
    setDeals(d.data);
    setSummary(s.data);
  }
  useEffect(() => { load(); }, []);

  async function move(id, stage) {
    setError('');
    try {
      await api.post(`/deals/${id}/stage`, { stage });
      load();
    } catch (err) {
      setError(err.response?.data?.error || 'Move failed (closed deals cannot change stage)');
    }
  }

  const totals = {};
  if (summary) for (const s of summary.stages) totals[s.stage] = s;

  return (
    <>
      {error && <p className="error">{error}</p>}
      <div className="board">
        {STAGES.map((st) => (
          <div className="column" key={st}>
            <strong>{st}</strong>
            <div style={{ fontSize: 12 }}>
              {totals[st]?.count ?? 0} deals · ${totals[st]?.total ?? 0}
            </div>
            {deals.filter((d) => d.stage === st).map((d) => (
              <div className="deal-card" key={d.id}>
                <Link to={`/deals/${d.id}`}>{d.title}</Link>
                <div>${d.value}</div>
                <select value={d.stage} onChange={(e) => move(d.id, e.target.value)}>
                  {STAGES.map((s) => <option key={s} value={s}>{s}</option>)}
                </select>
              </div>
            ))}
          </div>
        ))}
      </div>
    </>
  );
}
