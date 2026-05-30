import { useEffect, useRef, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { api } from '../api/axios';
import { useAuthStore } from '../store/authStore';

export default function OAuthCallback() {
    const { provider } = useParams<{ provider: string }>();
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const login = useAuthStore((state) => state.login);
    
    const [error, setError] = useState<string | null>(null);
    const hasRequested = useRef(false);

    useEffect(() => {
        const code = searchParams.get('code');
        
        if (!code) {
            setError('Authorization code not found in URL.');
            return;
        }

        if (!provider) {
            setError('Provider is missing.');
            return;
        }

        // Блокуємо подвійний запит від React Strict Mode
        if (hasRequested.current) return;
        hasRequested.current = true;

        const exchangeToken = async () => {
            try {
                // Формуємо Redirect URI динамічно на основі поточного порту фронтенду
                const redirectUri = `${window.location.origin}/auth/${provider}/callback`;
                
                // Звертаємося до твого бекенду
                const response = await api.post(`/auth/oauth/${provider}/callback`, {
                    code,
                    redirectUri
                });

                // Витягуємо токен (переконайся, що твій бекенд віддає його саме в полі token)
                const token = response.data.token || response.data;
                
                if (token && typeof token === 'string') {
                    login(token); // Зберігаємо в Zustand + localStorage
                    navigate('/', { replace: true }); // Викидаємо на дашборд
                } else {
                    setError('Backend did not return a valid token.');
                }
            } catch (err: any) {
                console.error('OAuth exchange error:', err);
                setError(err.response?.data?.message || 'Authentication failed. Check console.');
            }
        };

        exchangeToken();
    }, [provider, searchParams, navigate, login]);

    if (error) {
        return (
            <div style={{ color: 'red', textAlign: 'center', marginTop: '50px' }}>
                <h2>Authentication Error</h2>
                <p>{error}</p>
                <button onClick={() => navigate('/login')}>Back to Login</button>
            </div>
        );
    }

    return (
        <div style={{ textAlign: 'center', marginTop: '50px' }}>
            <h2>Processing authentication for {provider}...</h2>
            <p>Please wait, contacting backend...</p>
        </div>
    );
}