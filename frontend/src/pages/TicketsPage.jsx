import React, { useState, useEffect } from 'react';
import api from '../services/api';
import Pagination from '../components/Pagination';
import TicketForm from '../components/TicketForm';
import { useAuth } from '../context/AuthContext';
import { Plus, Edit, Trash2, Search, MessageSquare, CheckCircle, Clock } from 'lucide-react';

const TicketsPage = () => {
    const { user } = useAuth();
    const [tickets, setTickets] = useState([]);
    const [loading, setLoading] = useState(true);
    const [totalPages, setTotalPages] = useState(1);
    const [currentPage, setCurrentPage] = useState(1);
    const [searchQuery, setSearchQuery] = useState('');

    const [showModal, setShowModal] = useState(false);
    const [editingTicket, setEditingTicket] = useState(null);
    const [error, setError] = useState(null);

    const STATUS_MAP = { 'Open': 0, 'InProgress': 1, 'Resolved': 2, 'Closed': 3 };
    const STATUS_REVERSE_MAP = ['Open', 'InProgress', 'Resolved', 'Closed'];

    const PRIORITY_MAP = { 'Low': 0, 'Medium': 1, 'High': 2, 'Critical': 3 };
    const PRIORITY_REVERSE_MAP = ['Low', 'Medium', 'High', 'Critical'];

    const fetchTickets = async () => {
        setLoading(true);
        try {
            const params = {
                pageNumber: currentPage,
                pageSize: 10,
                search: searchQuery
            };
            const response = await api.get('/tickets', { params });
            const mappedItems = response.data.items.map(item => ({
                ...item,
                status: STATUS_REVERSE_MAP[item.status] || item.status,
                priority: PRIORITY_REVERSE_MAP[item.priority] || item.priority
            }));
            setTickets(mappedItems);
            setTotalPages(response.data.totalPages);
            setError(null);
        } catch (err) {
            console.error("Failed to fetch tickets", err);
            setError("Failed to load tickets. Please try again.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchTickets();
    }, [currentPage, searchQuery]);

    const handleCreate = async (formData) => {
        try {
            const payload = {
                ...formData,
                priority: PRIORITY_MAP[formData.priority],
                status: 0 // Default to Open
            };
            await api.post('/tickets', payload);
            setShowModal(false);
            fetchTickets();
        } catch (err) {
            alert("Failed to create ticket: " + (err.response?.data?.message || err.message));
        }
    };

    const handleUpdate = async (formData) => {
        if (!editingTicket) return;
        try {
            const payload = {
                ...formData,
                priority: PRIORITY_MAP[formData.priority]
            };
            await api.put(`/tickets/${editingTicket.id}`, payload);
            setShowModal(false);
            setEditingTicket(null);
            fetchTickets();
        } catch (err) {
            alert("Failed to update ticket: " + (err.response?.data?.message || err.message));
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm("Are you sure you want to delete this ticket?")) return;
        try {
            await api.delete(`/tickets/${id}`);
            fetchTickets();
        } catch (err) {
            alert("Failed to delete ticket: " + (err.response?.data?.message || err.message));
        }
    };

    const openCreateModal = () => {
        setEditingTicket(null);
        setShowModal(true);
    };

    const openEditModal = (ticket) => {
        setEditingTicket(ticket);
        setShowModal(true);
    };

    const getStatusColor = (status) => {
        switch (status) {
            case 'Open': return { bg: 'rgba(239, 68, 68, 0.2)', text: '#ef4444' };
            case 'InProgress': return { bg: 'rgba(59, 130, 246, 0.2)', text: '#3b82f6' };
            case 'Resolved': return { bg: 'rgba(34, 197, 94, 0.2)', text: '#22c55e' };
            case 'Closed': return { bg: 'rgba(107, 114, 128, 0.2)', text: '#6b7280' };
            default: return { bg: 'rgba(107, 114, 128, 0.2)', text: '#6b7280' };
        }
    };

    const getPriorityColor = (priority) => {
        switch (priority) {
            case 'Critical': return '#ef4444';
            case 'High': return '#f97316';
            case 'Medium': return '#eab308';
            case 'Low': return '#22c55e';
            default: return '#6b7280';
        }
    };

    return (
        <div style={{ padding: '2rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
                <h1 style={{ fontSize: '2rem', fontWeight: 'bold' }}>Ticket Management</h1>
                <button className="btn btn-primary" onClick={openCreateModal}>
                    <Plus size={20} style={{ marginRight: '0.5rem' }} /> Create Ticket
                </button>
            </div>

            <div className="card" style={{ marginBottom: '1.5rem', display: 'flex', gap: '1rem', alignItems: 'center' }}>
                <Search size={20} color="var(--text-secondary)" />
                <input
                    type="text"
                    className="input"
                    placeholder="Search tickets by title..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    style={{ width: '100%' }}
                />
            </div>

            {error && <div style={{ color: '#ef4444', marginBottom: '1rem' }}>{error}</div>}

            {showModal && (
                <div style={{
                    position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
                    backgroundColor: 'rgba(0,0,0,0.7)', display: 'flex', justifyContent: 'center', alignItems: 'center', zIndex: 1000
                }}>
                    <div style={{ width: '100%', maxWidth: '600px', maxHeight: '90vh', overflowY: 'auto' }}>
                        <TicketForm
                            initialData={editingTicket}
                            onSubmit={editingTicket ? handleUpdate : handleCreate}
                            onCancel={() => setShowModal(false)}
                        />
                    </div>
                </div>
            )}

            {loading ? (
                <p>Loading tickets...</p>
            ) : (
                <>
                    <div className="card" style={{ overflowX: 'auto' }}>
                        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
                            <thead>
                                <tr style={{ borderBottom: '1px solid var(--border-color)' }}>
                                    <th style={{ padding: '1rem' }}>Title</th>
                                    <th style={{ padding: '1rem' }}>Asset</th>
                                    <th style={{ padding: '1rem' }}>Priority</th>
                                    <th style={{ padding: '1rem' }}>Status</th>
                                    <th style={{ padding: '1rem' }}>Assignee</th>
                                    <th style={{ padding: '1rem' }}>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {tickets.map(ticket => {
                                    const statusStyle = getStatusColor(ticket.status);
                                    return (
                                        <tr key={ticket.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                                            <td style={{ padding: '1rem', fontWeight: '500' }}>
                                                <div>{ticket.title}</div>
                                                <div style={{ fontSize: '0.75rem', opacity: 0.8 }}>{new Date(ticket.createdAt).toLocaleDateString()}</div>
                                            </td>
                                            <td style={{ padding: '1rem' }}>Asset ID: {ticket.assetId}</td>
                                            <td style={{ padding: '1rem', fontWeight: 'bold', color: getPriorityColor(ticket.priority) }}>
                                                {ticket.priority}
                                            </td>
                                            <td style={{ padding: '1rem' }}>
                                                <span style={{
                                                    padding: '0.25rem 0.5rem', borderRadius: '4px', fontSize: '0.75rem', fontWeight: 'bold',
                                                    backgroundColor: statusStyle.bg,
                                                    color: statusStyle.text
                                                }}>
                                                    {ticket.status}
                                                </span>
                                            </td>
                                            <td style={{ padding: '1rem' }}>{ticket.assignedTo || 'Unassigned'}</td>
                                            <td style={{ padding: '1rem' }}>
                                                <div style={{ display: 'flex', gap: '0.5rem' }}>
                                                    <button className="btn btn-secondary" style={{ padding: '0.5rem' }} onClick={() => openEditModal(ticket)}>
                                                        <Edit size={16} />
                                                    </button>
                                                    <button className="btn btn-danger" style={{ padding: '0.5rem' }} onClick={() => handleDelete(ticket.id)}>
                                                        <Trash2 size={16} />
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                        {tickets.length === 0 && (
                            <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
                                No tickets found.
                            </div>
                        )}
                    </div>

                    <Pagination
                        currentPage={currentPage}
                        totalPages={totalPages}
                        onPageChange={setCurrentPage}
                    />
                </>
            )}
        </div>
    );
};

export default TicketsPage;
