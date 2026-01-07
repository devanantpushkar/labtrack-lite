import React, { useState, useRef, useEffect } from 'react';
import api from '../services/api';
import { Send, Bot, User, Loader } from 'lucide-react';

const ChatbotPage = () => {
    const [messages, setMessages] = useState([
        { id: 1, text: "Hello! I am the LabTrack AI. You can ask me about assets, tickets, or system status.", sender: 'bot' }
    ]);
    const [input, setInput] = useState('');
    const [loading, setLoading] = useState(false);
    const messagesEndRef = useRef(null);

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    };

    useEffect(() => {
        scrollToBottom();
    }, [messages]);

    const handleSend = async (e) => {
        e.preventDefault();
        if (!input.trim()) return;

        const userMessage = { id: Date.now(), text: input, sender: 'user' };
        setMessages(prev => [...prev, userMessage]);
        setInput('');
        setLoading(true);

        try {
            const response = await api.post('/chatbot/query', { query: userMessage.text });
            const botMessage = {
                id: Date.now() + 1,
                text: response.data.message,
                sender: 'bot',
                queryType: response.data.queryType
            };
            setMessages(prev => [...prev, botMessage]);
        } catch (err) {
            const errorMessage = {
                id: Date.now() + 1,
                text: "Sorry, I'm having trouble connecting to the server.",
                sender: 'bot',
                isError: true
            };
            setMessages(prev => [...prev, errorMessage]);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ padding: '2rem', height: 'calc(100vh - 80px)', display: 'flex', flexDirection: 'column' }}>
            <h1 style={{ fontSize: '2rem', fontWeight: 'bold', marginBottom: '1rem' }}>AI Assistant</h1>

            <div className="card" style={{
                flex: 1,
                display: 'flex',
                flexDirection: 'column',
                padding: '0',
                overflow: 'hidden',
                maxHeight: '700px'
            }}>
                {}
                <div style={{
                    flex: 1,
                    overflowY: 'auto',
                    padding: '1.5rem',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: '1rem',
                    backgroundColor: 'var(--background-secondary)'
                }}>
                    {messages.map(msg => (
                        <div key={msg.id} style={{
                            display: 'flex',
                            justifyContent: msg.sender === 'user' ? 'flex-end' : 'flex-start',
                            gap: '0.75rem'
                        }}>
                            {msg.sender === 'bot' && (
                                <div style={{
                                    width: '32px', height: '32px', borderRadius: '50%', backgroundColor: '#6366f1',
                                    display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0
                                }}>
                                    <Bot size={18} color="white" />
                                </div>
                            )}

                            <div style={{
                                padding: '1rem',
                                borderRadius: '12px',
                                maxWidth: '70%',
                                backgroundColor: msg.sender === 'user' ? '#4f46e5' : 'var(--background-primary)',
                                color: msg.sender === 'user' ? 'white' : 'var(--text-primary)',
                                border: msg.sender === 'bot' ? '1px solid var(--border-color)' : 'none',
                                borderTopLeftRadius: msg.sender === 'bot' ? '2px' : '12px',
                                borderTopRightRadius: msg.sender === 'user' ? '2px' : '12px'
                            }}>
                                <p style={{ margin: 0, whiteSpace: 'pre-wrap', lineHeight: '1.5' }}>{msg.text}</p>
                            </div>

                            {msg.sender === 'user' && (
                                <div style={{
                                    width: '32px', height: '32px', borderRadius: '50%', backgroundColor: '#4b5563',
                                    display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0
                                }}>
                                    <User size={18} color="white" />
                                </div>
                            )}
                        </div>
                    ))}
                    {loading && (
                        <div style={{ display: 'flex', gap: '0.75rem' }}>
                            <div style={{
                                width: '32px', height: '32px', borderRadius: '50%', backgroundColor: '#6366f1',
                                display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0
                            }}>
                                <Bot size={18} color="white" />
                            </div>
                            <div style={{
                                padding: '1rem',
                                borderRadius: '12px',
                                borderTopLeftRadius: '2px',
                                backgroundColor: 'var(--background-primary)',
                                border: '1px solid var(--border-color)'
                            }}>
                                <Loader size={20} className="spin" style={{ opacity: 0.6 }} />
                            </div>
                        </div>
                    )}
                    <div ref={messagesEndRef} />
                </div>

                {}
                <div style={{ padding: '1rem', borderTop: '1px solid var(--border-color)', backgroundColor: 'var(--card-bg)' }}>
                    <form onSubmit={handleSend} style={{ display: 'flex', gap: '0.75rem' }}>
                        <input
                            type="text"
                            className="input"
                            value={input}
                            onChange={(e) => setInput(e.target.value)}
                            placeholder="Ask about assets, tickets, or status..."
                            style={{ flex: 1 }}
                            autoFocus
                        />
                        <button type="submit" className="btn btn-primary" disabled={loading || !input.trim()}>
                            <Send size={20} />
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default ChatbotPage;
