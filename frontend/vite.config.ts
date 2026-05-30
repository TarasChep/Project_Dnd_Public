import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    strictPort: true, // Впаде з помилкою, якщо порт 3000 зайнятий, замість переходу на 3001
  }
})