/** @type {import('tailwindcss').Config} */
export default {
    darkMode: 'class',
    content: [
      "./index.html",
      "./src/**/*.{js,ts,jsx,tsx}",
    ],
    theme: {
      extend: {
        colors: {
          primary: {
            DEFAULT: '#2563eb',
            dark: '#1d4ed8',
          },
          secondary: {
            DEFAULT: '#475569',
            dark: '#334155',
          },
        },
        fontFamily: {
          sans: ['Inter var', 'sans-serif'],
        },
      },
    },
    plugins: [],
  }