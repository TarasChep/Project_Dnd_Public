import { create } from 'zustand';

interface AuthState {
    token: string | null;
    isAuthenticated: boolean;
    login: (token: string) => void;
    logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
    token: localStorage.getItem('jwtToken'),
    isAuthenticated: !!localStorage.getItem('jwtToken'),

    login: (token: string) => {
        localStorage.setItem('jwtToken', token);
        set({ token, isAuthenticated: true });
    },

    logout: () => {
        localStorage.removeItem('jwtToken');
        set({ token: null, isAuthenticated: false });
    },
}));