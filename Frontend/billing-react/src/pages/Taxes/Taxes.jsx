import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { DashboardHeader } from '../../components/dashboard/DashboardHeader';

export function Taxes() {
  const [search, setSearch] = useState('');
  const navigate = useNavigate();

  const signOut = () => {
    localStorage.removeItem('billing_auth_token');
    localStorage.removeItem('billing_auth_user');
    navigate('/login');
  };

  return (
    <div className="tax-module">
      <DashboardHeader searchQuery={search} onSearch={setSearch} onSignOut={signOut} />
      <main aria-label="Taxes & GST" />
    </div>
  );
}
