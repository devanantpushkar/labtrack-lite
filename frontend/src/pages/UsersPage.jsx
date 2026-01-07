import React, { useState, useEffect } from 'react';
import api from '../services/api';
import { Users, Shield, Mail, Calendar } from 'lucide-react';

const UsersPage = () => {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchUsers = async () => {
            try {
                const response = await api.get('/auth/users');
                setUsers(response.data);
                setError(null);
            } catch (err) {
                console.error("Failed to fetch users", err);
                setError("You may not have permission to view this page.");
            } finally {
                setLoading(false);
            }
        };
        fetchUsers();
    }, []);

    if (loading) return <div className="loading">Loading user database...</div>;
    if (error) return <div className="error-message">{error}</div>;

    return (
        <div className="users-page">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
                <h2 style={{ fontSize: '1.5rem', fontWeight: 'bold' }}>User Management</h2>
                <div className="badge badge-primary">
                    <Users size={14} style={{ marginRight: '4px' }} />
                    {users.length} Users Total
                </div>
            </div>

            <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
                <table className="table">
                    <thead>
                        <tr>
                            <th>User</th>
                            <th>Email</th>
                            <th>Role</th>
                            <th>Joined</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map(user => (
                            <tr key={user.id}>
                                <td>
                                    <div style={{ fontWeight: '600' }}>{user.username}</div>
                                    <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>ID: #{user.id}</div>
                                </td>
                                <td>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                        <Mail size={14} color="var(--text-secondary)" />
                                        {user.email}
                                    </div>
                                </td>
                                <td>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                                        <Shield size={14} color={user.role === 'Admin' ? '#ef4444' : '#3b82f6'} />
                                        <span className={`badge ${user.role === 'Admin' ? 'badge-danger' : 'badge-primary'}`}>
                                            {user.role}
                                        </span>
                                    </div>
                                </td>
                                <td>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px', color: 'var(--text-secondary)' }}>
                                        <Calendar size={14} />
                                        {new Date(user.createdAt).toLocaleDateString()}
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            <div style={{ marginTop: '2rem' }}>
                <div className="alert-box" style={{ backgroundColor: 'rgba(59, 130, 246, 0.1)', color: 'var(--primary-color)', padding: '1rem', borderRadius: '8px' }}>
                    <strong>Note:</strong> User editing and deletion are reserved for the next system update. Use the registration page to add new users.
                </div>
            </div>
        </div>
    );
};

export default UsersPage;
