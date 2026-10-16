import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import api from '../api.js';

export default function CustomerDetail() {
  const { id } = useParams();
  const [customer, setCustomer] = useState(null);

  useEffect(() => {
    api.get(`/customers/${id}`).then((r) => setCustomer(r.data));
  }, [id]);

  if (!customer) return <p>Loading…</p>;
  return (
    <div className="card">
      <h2>{customer.name}</h2>
      <p>{customer.company} · {customer.email} · {customer.phone}</p>
      <h3>Deals ({(customer.deals || []).length})</h3>
      {(customer.deals || []).map((d) => (
        <div key={d.id}>
          <Link to={`/deals/${d.id}`}>{d.title}</Link> — {d.stage} — ${d.value}
        </div>
      ))}
    </div>
  );
}
