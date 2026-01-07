import React, { useState, useEffect } from 'react';
import api from '../services/api';

const PRIORITY_OPTIONS = ['Low', 'Medium', 'High', 'Critical'];

const TicketForm = ({ initialData, onSubmit, onCancel }) => {
    const [assets, setAssets] = useState([]);
    const [formData, setFormData] = useState({
        title: '',
        description: '',
        assetId: '',
        priority: 'Medium'
    });

    useEffect(() => {
        const fetchAssets = async () => {
            try {
                const response = await api.get('/assets?pageSize=100');
                setAssets(response.data.items);
            } catch (err) {
                console.error("Failed to load assets for dropdown", err);
            }
        };
        fetchAssets();

        if (initialData) {
            setFormData({
                title: initialData.title || '',
                description: initialData.description || '',
                assetId: initialData.assetId || '',
                priority: initialData.priority || 'Medium' // Input is string if mapped correctly in parent
            });
        }
    }, [initialData]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        onSubmit(formData);
    };

    return (
        <div className="card" style={{ maxWidth: '600px', margin: '0 auto' }}>
            <h2 style={{ marginBottom: '1.5rem', color: 'var(--text-primary)' }}>
                {initialData ? 'Edit Ticket' : 'Create New Ticket'}
            </h2>

            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                <div className="form-group">
                    <label className="label" htmlFor="ticket-title">Title *</label>
                    <input
                        id="ticket-title"
                        type="text"
                        name="title"
                        className="input"
                        value={formData.title}
                        onChange={handleChange}
                        required
                        placeholder="Brief summary of the issue"
                        autoFocus
                    />
                </div>

                <div className="form-group">
                    <label className="label" htmlFor="ticket-description">Description *</label>
                    <textarea
                        id="ticket-description"
                        name="description"
                        className="input"
                        value={formData.description}
                        onChange={handleChange}
                        required
                        rows="4"
                        placeholder="Detailed description of the problem..."
                    />
                </div>

                <div className="form-group">
                    <label className="label" htmlFor="ticket-asset">Asset *</label>
                    <select
                        id="ticket-asset"
                        name="assetId"
                        className="input"
                        value={formData.assetId}
                        onChange={handleChange}
                        required
                        disabled={!!initialData}
                    >
                        <option value="">Select an Asset</option>
                        {assets.map(asset => (
                            <option key={asset.id} value={asset.id}>
                                {asset.name} ({asset.qrCode})
                            </option>
                        ))}
                    </select>
                </div>

                <div className="form-group">
                    <label className="label" htmlFor="ticket-priority">Priority</label>
                    <select
                        id="ticket-priority"
                        name="priority"
                        className="input"
                        value={formData.priority}
                        onChange={handleChange}
                    >
                        {PRIORITY_OPTIONS.map(opt => (
                            <option key={opt} value={opt}>{opt}</option>
                        ))}
                    </select>
                </div>

                <div style={{ display: 'flex', gap: '1rem', marginTop: '1rem' }}>
                    <button type="submit" className="btn btn-primary" style={{ flex: 1 }}>
                        {initialData ? 'Update Ticket' : 'Create Ticket'}
                    </button>
                    <button type="button" className="btn btn-secondary" onClick={onCancel} style={{ flex: 1 }}>
                        Cancel
                    </button>
                </div>
            </form>
        </div>
    );
};

export default TicketForm;
