import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
   server: {
    host: "0.0.0.0",
    port: 5174
  },
  plugins: [react()],
})
