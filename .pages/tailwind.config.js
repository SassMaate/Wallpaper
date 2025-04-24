/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    "./components/**/*.{js,vue,ts}",
    "./layouts/**/*.vue",
    "./pages/**/*.vue",
    "./plugins/**/*.{js,ts}",
    "./app.vue",
  ],
  theme: {
    screens: {
      'xs': '480px',
      'sm': '640px',
      'md': '768px',
      'lg': '1024px',
      'xl': '1280px',
      '2xl': '1536px',
    },
    extend: {
      colors: {
        primary: {
          50: '#eefbf9',
          100: '#d6f5f3',
          200: '#ade7e4',
          300: '#75d3d1',
          400: '#41b9b9',
          500: '#249f9f',
          600: '#1e8384',
          700: '#1c696c',
          800: '#1b5558',
          900: '#1a4749',
          950: '#0a2b2d',
        },
        secondary: {
          50: '#f6f8fd',
          100: '#edf0fd',
          200: '#dbe1fa',
          300: '#bdc6f4',
          400: '#98a5ec',
          500: '#7c86e2',
          600: '#6366d3',
          700: '#5151b9',
          800: '#434495',
          900: '#393c78',
          950: '#232244',
        },
        accent: {
          50: '#fef6ee',
          100: '#fdead7',
          200: '#f9d1ad',
          300: '#f5b17b',
          400: '#f0894a',
          500: '#eb6b2a',
          600: '#d94d1f',
          700: '#b5391c',
          800: '#922e1d',
          900: '#78291c',
          950: '#40130c',
        },
        neutral: {
          50: '#f8f9fa',
          100: '#f1f3f5',
          200: '#e9ecef',
          300: '#dee2e6',
          400: '#ced4da',
          500: '#adb5bd',
          600: '#868e96',
          700: '#495057',
          800: '#343a40',
          900: '#212529',
          950: '#111215',
        }
      },
      animation: {
        'gradient-text': 'gradient 5s ease infinite',
        'float': 'float 10s ease-in-out infinite',
        'pulse-slow': 'pulse 4s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'shimmer': 'shimmer 2s linear infinite',
        'slide-up': 'slideUp 0.5s ease-out',
      },
      keyframes: {
        gradient: {
          '0%': { backgroundPosition: '0% 50%' },
          '50%': { backgroundPosition: '100% 50%' },
          '100%': { backgroundPosition: '0% 50%' },
        },
        float: {
          '0%': { transform: 'translateY(0px) translateX(0px)' },
          '33%': { transform: 'translateY(-10px) translateX(10px)' },
          '66%': { transform: 'translateY(10px) translateX(-10px)' },
          '100%': { transform: 'translateY(0px) translateX(0px)' },
        },
        shimmer: {
          '0%': { backgroundPosition: '-1000px 0' },
          '100%': { backgroundPosition: '1000px 0' },
        },
        slideUp: {
          '0%': { transform: 'translateY(20px)', opacity: 0 },
          '100%': { transform: 'translateY(0)', opacity: 1 },
        }
      },
      boxShadow: {
        'neumorphic-light': '10px 10px 20px #d1d9e6, -10px -10px 20px #ffffff',
        'neumorphic-dark': '10px 10px 20px #131a29, -10px -10px 20px #1e2a3b',
        'glass': '0 8px 32px 0 rgba(31, 38, 135, 0.37)',
      },
      borderRadius: {
        'xl': '1rem',
        '2xl': '1.5rem',
        '3xl': '2rem',
      },
      fontFamily: {
        'sans': ['Inter', 'ui-sans-serif', 'system-ui', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'Helvetica Neue', 'sans-serif'],
        'display': ['Poppins', 'sans-serif'],
        'mono': ['Fira Code', 'monospace'],
      },
      backdropFilter: {
        'none': 'none',
        'blur': 'blur(20px)',
      },
    },
  },
  plugins: [
    function ({ addUtilities }) {
      const newUtilities = {
        '.bg-gradient-text': {
          background: 'linear-gradient(90deg, #249f9f, #6366d3, #eb6b2a)',
          '-webkit-background-clip': 'text',
          'background-clip': 'text',
          'color': 'transparent',
          'background-size': '300% 100%',
        },
        '.glass': {
          backgroundColor: 'rgba(255, 255, 255, 0.08)',
          backdropFilter: 'blur(8px)',
          border: '1px solid rgba(255, 255, 255, 0.1)',
        },
        '.dark .glass': {
          backgroundColor: 'rgba(0, 0, 0, 0.15)',
          border: '1px solid rgba(255, 255, 255, 0.05)',
        },
        '.hide-scrollbar': {
          '-ms-overflow-style': 'none',
          'scrollbar-width': 'none',
        },
        '.hide-scrollbar::-webkit-scrollbar': {
          display: 'none',
        },
      }
      addUtilities(newUtilities)
    }
  ],
}