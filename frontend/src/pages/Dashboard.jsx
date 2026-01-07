import { useEffect, useState } from 'react';
import api from '../services/api';
import { Box, Ticket, AlertCircle } from 'lucide-react';

const StatCard = ({ title, value, icon: Icon, color }) => (
    <div className="card">
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
            <div style={{
                width: '48px', height: '48px', borderRadius: '12px',
                backgroundColor: `rgba(${color}, 0.1)`, color: `rgb(${color})`,
                display: 'flex', alignItems: 'center', justifyContent: 'center'
            }}>
                <Icon size={24} />
            </div>
            <div>
                <p style={{ color: 'var(--text-secondary)', fontSize: '0.875rem' }}>{title}</p>
                <p style={{ fontSize: '1.5rem', fontWeight: 'bold', margin: 0 }}>{value}</p>
            </div>
        </div>
    </div>
);

const Dashboard = () => {
    const [stats, setStats] = useState({ assets: 0, tickets: 0, myTickets: 0 });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchStats = async () => {
            try {
                const [assetsRes, ticketsRes, myTicketsRes] = await Promise.all([
                    api.get('/assets?pageSize=1'),
                    api.get('/tickets?pageSize=1'),
                    api.get('/tickets?ownOnly=true&pageSize=1')
                ]);

                setStats({
                    assets: assetsRes.data.totalCount || 0,
                    tickets: ticketsRes.data.totalCount || 0,
                    myTickets: myTicketsRes.data.totalCount || 0
                });
            } catch (error) {
                console.error("Failed to fetch dashboard stats", error);
            } finally {
                setLoading(false);
            }
        };
        fetchStats();
    }, []);

    return (
        <div>
            <h2 style={{ fontSize: '1.5rem', fontWeight: 'bold', marginBottom: '1.5rem' }}>Dashboard Overview</h2>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '1.5rem' }}>
                <StatCard title="Total Assets" value={loading ? '-' : stats.assets} icon={Box} color="59, 130, 246" />
                <StatCard title="Total Tickets" value={loading ? '-' : stats.tickets} icon={Ticket} color="16, 185, 129" />
                <StatCard title="My Tickets" value={loading ? '-' : stats.myTickets} icon={AlertCircle} color="245, 158, 11" />
            </div>

            {}
            <div className="card" style={{ marginTop: '2rem' }}>
                <h3 style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '1rem' }}>Welcome to LabTrack Lite</h3>
                <p style={{ color: 'var(--text-secondary)' }}>
                    Select an option from the sidebar to manage laboratory assets, view tickets, or ask the chatbot for assistance.
                </p>
            </div>
        </div>
    );
};

export default Dashboard;
