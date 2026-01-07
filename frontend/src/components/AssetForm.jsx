import React, { useState, useEffect } from 'react';

const STATUS_OPTIONS = ['Available', 'InUse', 'Maintenance', 'Retired'];

const AssetForm = ({ initialData, onSubmit, onCancel }) => {
    const [formData, setFormData] = useState({
        name: '',
        description: '',
        category: '',
        location: '',
        status: 'Available',
        qrCode: ''
    });

    useEffect(() => {
        if (initialData) {
            setFormData({
                name: initialData.name || '',
                description: initialData.description || '',
                category: initialData.category || '',
                location: initialData.location || '',
                status: initialData.status || 'Available',
                qrCode: initialData.qrCode || ''
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
                {initialData ? 'Edit Asset' : 'Add New Asset'}
            </h2>

            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                <div className="form-group">
                    <label className="label" htmlFor="asset-name">Asset Name *</label>
                    <input
                        id="asset-name"
                        type="text"
                        name="name"
                        className="input"
                        value={formData.name}
                        onChange={handleChange}
                        required
                        autoFocus
                    />
                </div>

                <div className="form-group">
                    <label className="label" htmlFor="asset-description">Description</label>
                    <textarea
                        id="asset-description"
                        name="description"
                        className="input"
                        value={formData.description}
                        onChange={handleChange}
                        rows="3"
                    />
                </div>

                <div className="form-group">
                    <label className="label" htmlFor="asset-category">Category</label>
                    <input
                        id="asset-category"
                        type="text"
                        name="category"
                        className="input"
                        value={formData.category}
                        onChange={handleChange}
                        placeholder="e.g., Microscope, Centrifuge"
                    />
                </div>

                <div className="form-group">
                    <label className="label" htmlFor="asset-location">Location</label>
                    <input
                        id="asset-location"
                        type="text"
                        name="location"
                        className="input"
                        value={formData.location}
                        onChange={handleChange}
                        placeholder="e.g., Lab 101"
                    />
                </div>

                <div className="form-group">
                    <label className="label" htmlFor="asset-status">Status</label>
                    <select
                        id="asset-status"
                        name="status"
                        className="input"
                        value={formData.status}
                        onChange={handleChange}
                    >
                        {STATUS_OPTIONS.map(opt => (
                            <option key={opt} value={opt}>{opt}</option>
                        ))}
                    </select>
                </div>

                <div className="form-group">
                    <label className="label" htmlFor="asset-qrcode">QR Code / Tag ID</label>
                    <input
                        id="asset-qrcode"
                        type="text"
                        name="qrCode"
                        className="input"
                        value={formData.qrCode}
                        onChange={handleChange}
                        required
                        placeholder="Unique Asset ID"
                    />
                </div>

                <div style={{ display: 'flex', gap: '1rem', marginTop: '1rem' }}>
                    <button type="submit" className="btn btn-primary" style={{ flex: 1 }}>
                        {initialData ? 'Update Asset' : 'Create Asset'}
                    </button>
                    <button type="button" className="btn btn-secondary" onClick={onCancel} style={{ flex: 1 }}>
                        Cancel
                    </button>
                </div>
            </form>
        </div>
    );
};

export default AssetForm;
