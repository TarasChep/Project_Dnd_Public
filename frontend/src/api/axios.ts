import axios from 'axios';
import { useAuthStore } from '../store/authStore';

// Порт твого бекенду (з попередніх налаштувань)
const API_BASE_URL = 'http://localhost:5142/api';

export const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Перехоплювач запитів: додаємо токен
api.interceptors.request.use((config) => {
    const token = useAuthStore.getState().token;
    if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Перехоплювач відповідей: обробка протухлого токена
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            useAuthStore.getState().logout();
            window.location.href = '/login'; // Жорсткий редирект
        }
        return Promise.reject(error);
    }
);