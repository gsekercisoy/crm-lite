import React from 'react';
import { Routes, Route, Link, Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from './auth.jsx';
import Login from './pages/Login.jsx';
import Customers from './pages/Customers.jsx';
import CustomerDetail from './pages/CustomerDetail.jsx';
import Pipeline from './pages/Pipeline.jsx';
import DealDetail from './pages/DealDetail.jsx';

function Guard({ children }) {
  const { token } = useAuth();
  if (!token) return <Navigate to="/login" replace />;
  return children;
}

export default function App() {
  const { token, email, logout } = useAuth();
  const navigate = useNavigate();
  return (
    <>
      <nav>
        <strong>CRM Lite</strong>
        {token && (
          <>
            <Link to="/customers">Customers</Link>
            <Link to="/pipeline">Pipeline</Link>
            <span style={{ marginLeft: 'auto', fontSize: 13 }}>{email}</span>
            <button onClick={() => { logout(); navigate('/login'); }}>Logout</button>
          </>
        )}
      </nav>
      <div className="container">
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/customers" element={<Guard><Customers /></Guard>} />
          <Route path="/customers/:id" element={<Guard><CustomerDetail /></Guard>} />
          <Route path="/pipeline" element={<Guard><Pipeline /></Guard>} />
          <Route path="/deals/:id" element={<Guard><DealDetail /></Guard>} />
          <Route path="*" element={<Navigate to={token ? '/pipeline' : '/login'} replace />} />
        </Routes>
      </div>
    </>
  );
}
