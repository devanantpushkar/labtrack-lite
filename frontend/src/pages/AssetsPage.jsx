import React, { useState, useEffect } from 'react';
import api from '../services/api';
import Layout from '../components/Layout';
import Pagination from '../components/Pagination';
import AssetForm from '../components/AssetForm';
import { useAuth } from '../context/AuthContext';
import { Plus, Edit, Trash2, Search } from 'lucide-react';

const AssetsPage = () => {
    const { user } = useAuth();
    const [assets, setAssets] = useState([]);
    const [loading, setLoading] = useState(true);
    const [totalPages, setTotalPages] = useState(1);
    const [currentPage, setCurrentPage] = useState(1);
    const [searchQuery, setSearchQuery] = useState('');

    const [showModal, setShowModal] = useState(false);
    const [editingAsset, setEditingAsset] = useState(null);
    const [error, setError] = useState(null);

    const isAdminOrEngineer = user?.role === 'Admin' || user?.role === 'Engineer';
    const isAdmin = user?.role === 'Admin';

    const STATUS_MAP = { 'Available': 0, 'InUse': 1, 'Maintenance': 2, 'Retired': 3 };
    const STATUS_REVERSE_MAP = ['Available', 'InUse', 'Maintenance', 'Retired'];

    const fetchAssets = async () => {
        setLoading(true);
        try {
            const params = {
                pageNumber: currentPage,
                pageSize: 10,
                search: searchQuery
            };
            const response = await api.get('/assets', { params });
            const mappedItems = response.data.items.map(item => ({
                ...item,
                status: STATUS_REVERSE_MAP[item.status] || item.status // Map int to string
            }));
            setAssets(mappedItems);
            setTotalPages(response.data.totalPages);
            setError(null);
        } catch (err) {
            console.error("Failed to fetch assets", err);
            setError("Failed to load assets. Please try again.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchAssets();
    }, [currentPage, searchQuery]);

    const handleCreate = async (formData) => {
        try {
            const payload = { ...formData, status: STATUS_MAP[formData.status] };
            await api.post('/assets', payload);
            setShowModal(false);
            fetchAssets();
        } catch (err) {
            alert("Failed to create asset: " + (err.response?.data?.message || err.message));
        }
    };

    const handleUpdate = async (formData) => {
        if (!editingAsset) return;
        try {
            const payload = { ...formData, status: STATUS_MAP[formData.status] };
            await api.put(`/assets/${editingAsset.id}`, payload);
            setShowModal(false);
            setEditingAsset(null);
            fetchAssets();
        } catch (err) {
            alert("Failed to update asset: " + (err.response?.data?.message || err.message));
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm("Are you sure you want to delete this asset?")) return;
        try {
            await api.delete(`/assets/${id}`);
            fetchAssets();
        } catch (err) {
            alert("Failed to delete asset: " + (err.response?.data?.message || err.message));
        }
    };

    const openCreateModal = () => {
        setEditingAsset(null);
        setShowModal(true);
    };

    const openEditModal = (asset) => {
        setEditingAsset(asset);
        setShowModal(true);
    };

    return (
        <div style={{ padding: '2rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
                <h1 style={{ fontSize: '2rem', fontWeight: 'bold' }}>Assets Management</h1>
                {isAdminOrEngineer && (
                    <button className="btn btn-primary" onClick={openCreateModal}>
                        <Plus size={20} style={{ marginRight: '0.5rem' }} /> Add Asset
                    </button>
                )}
            </div>

            <div className="card" style={{ marginBottom: '1.5rem', display: 'flex', gap: '1rem', alignItems: 'center' }}>
                <Search size={20} color="var(--text-secondary)" />
                <input
                    type="text"
                    className="input"
                    placeholder="Search by name, category, or QR code..."
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
                        <AssetForm
                            initialData={editingAsset}
                            onSubmit={editingAsset ? handleUpdate : handleCreate}
                            onCancel={() => setShowModal(false)}
                        />
                    </div>
                </div>
            )}

            {loading ? (
                <p>Loading assets...</p>
            ) : (
                <>
                    <div className="card" style={{ overflowX: 'auto' }}>
                        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
                            <thead>
                                <tr style={{ borderBottom: '1px solid var(--border-color)' }}>
                                    <th style={{ padding: '1rem' }}>Name</th>
                                    <th style={{ padding: '1rem' }}>Details</th>
                                    <th style={{ padding: '1rem' }}>Status</th>
                                    <th style={{ padding: '1rem' }}>Location</th>
                                    <th style={{ padding: '1rem' }}>QR Code</th>
                                    <th style={{ padding: '1rem' }}>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {assets.map(asset => (
                                    <tr key={asset.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                                        <td style={{ padding: '1rem', fontWeight: '500' }}>{asset.name}</td>
                                        <td style={{ padding: '1rem', fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                                            <div>{asset.category}</div>
                                            <div style={{ fontSize: '0.75rem', opacity: 0.8 }}>{asset.description}</div>
                                        </td>
                                        <td style={{ padding: '1rem' }}>
                                            <span style={{
                                                padding: '0.25rem 0.5rem', borderRadius: '4px', fontSize: '0.75rem', fontWeight: 'bold',
                                                backgroundColor:
                                                    asset.status === 'Available' ? 'rgba(34, 197, 94, 0.2)' :
                                                        asset.status === 'InUse' ? 'rgba(59, 130, 246, 0.2)' :
                                                            asset.status === 'Maintenance' ? 'rgba(234, 179, 8, 0.2)' : 'rgba(239, 68, 68, 0.2)',
                                                color:
                                                    asset.status === 'Available' ? '#22c55e' :
                                                        asset.status === 'InUse' ? '#3b82f6' :
                                                            asset.status === 'Maintenance' ? '#eab308' : '#ef4444'
                                            }}>
                                                {asset.status}
                                            </span>
                                        </td>
                                        <td style={{ padding: '1rem' }}>{asset.location}</td>
                                        <td style={{ padding: '1rem', fontFamily: 'monospace' }}>{asset.qrCode}</td>
                                        <td style={{ padding: '1rem' }}>
                                            <div style={{ display: 'flex', gap: '0.5rem' }}>
                                                {isAdminOrEngineer && (
                                                    <button className="btn btn-secondary" style={{ padding: '0.5rem' }} onClick={() => openEditModal(asset)}>
                                                        <Edit size={16} />
                                                    </button>
                                                )}
                                                {isAdmin && (
                                                    <button className="btn btn-danger" style={{ padding: '0.5rem' }} onClick={() => handleDelete(asset.id)}>
                                                        <Trash2 size={16} />
                                                    </button>
                                                )}
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                        {assets.length === 0 && (
                            <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
                                No assets found.
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

export default AssetsPage;
