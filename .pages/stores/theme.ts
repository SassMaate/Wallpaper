import { defineStore } from 'pinia';

export const useThemeStore = defineStore('theme', {
  state: () => ({
    theme: 'system',
    isDarkMode: false
  }),
  
  getters: {
    isDark: (state) => state.isDarkMode,
    currentTheme: (state) => state.theme
  },
  
  actions: {
    setTheme(theme: 'light' | 'dark' | 'system') {
      this.theme = theme;
      
      if (theme === 'system') {
        this.setSystemTheme();
      } else {
        this.isDarkMode = theme === 'dark';
        this.applyTheme();
        if (typeof window !== 'undefined') {
          localStorage.setItem('theme', theme);
        }
      }
    },
    
    setSystemTheme() {
      if (typeof window !== 'undefined') {
        const isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        this.isDarkMode = isDark;
        this.applyTheme();
        localStorage.setItem('theme', 'system');
      }
    },
    
    applyTheme() {
      if (typeof window !== 'undefined' && typeof document !== 'undefined') {
        if (this.isDarkMode) {
          document.documentElement.classList.add('dark');
        } else {
          document.documentElement.classList.remove('dark');
        }
      }
    },
    
    initTheme() {
      if (typeof window !== 'undefined') {
        // Get theme setting from localStorage or use 'system' as default
        const savedTheme = localStorage.getItem('theme') as 'light' | 'dark' | 'system' || 'system';
        this.theme = savedTheme;
        
        if (savedTheme === 'system') {
          this.setSystemTheme();
        } else {
          this.isDarkMode = savedTheme === 'dark';
          this.applyTheme();
        }
        
        // Listen for system theme changes
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        const handleChange = (e: MediaQueryListEvent) => {
          if (this.theme === 'system') {
            this.isDarkMode = e.matches;
            this.applyTheme();
          }
        };
        
        // Different methods for old and new browsers
        if (mediaQuery.addEventListener) {
          mediaQuery.addEventListener('change', handleChange);
        } else if ('addListener' in mediaQuery) {
          // @ts-ignore - Use addListener method for older browsers for TypeScript
          mediaQuery.addListener(handleChange);
        }
      }
    }
  }
}); 