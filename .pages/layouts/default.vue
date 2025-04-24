<template>
  <div class="flex flex-col min-h-screen">
    <header class="fixed w-full z-50 transition-all duration-300" :class="{ 'scrolled': isScrolled }">
      <div class="absolute inset-0 bg-white/90 dark:bg-gradient-to-r dark:from-primary-900/80 dark:via-gray-900/90 dark:to-primary-800/80 backdrop-blur-md opacity-0 transition-opacity duration-300" :class="{ 'opacity-100': isScrolled }" id="headerBackground"></div>
      <nav class="container mx-auto px-6 py-4 relative">
        <div class="flex items-center justify-between">
          <div class="flex items-center space-x-2">
            <img src="/images/Logo.svg" alt="Sucrose Logo" class="w-8 h-8 rounded-lg">
            <span class="text-2xl font-bold bg-gradient-text dark:bg-gradient-text cursor-default hidden xs:inline-block">Sucrose Wallpaper Engine</span>
          </div>
          
          <!-- Desktop Menu -->
          <div class="hidden md:flex items-center space-x-8">
            <a href="#features" class="text-gray-800 dark:text-white hover:text-primary-600 dark:hover:text-primary-400 transition-colors">{{ $t('nav.features') }}</a>
            <a href="#gallery" class="text-gray-800 dark:text-white hover:text-primary-600 dark:hover:text-primary-400 transition-colors">{{ $t('nav.gallery') }}</a>
            <a href="#download" class="text-gray-800 dark:text-white hover:text-primary-600 dark:hover:text-primary-400 transition-colors">{{ $t('nav.download') }}</a>
            
            <div class="flex items-center space-x-4 pl-4 border-l border-gray-400 dark:border-gray-600">
              <!-- Language Selector -->
              <div class="relative">
                <button @click.stop="toggleLanguageDropdown" class="flex items-center space-x-1 text-gray-800 dark:text-white hover:text-primary-600 dark:hover:text-primary-400">
                  <span>{{ currentLocale.name }}</span>
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                  </svg>
                </button>
                
                <div v-if="isLanguageDropdownOpen" class="absolute right-0 mt-2 w-72 bg-white dark:bg-gray-800 rounded-md shadow-lg overflow-hidden z-20">
                  <div class="p-2">
                    <div class="mb-2">
                      <input 
                        v-model="languageSearch" 
                        type="text" 
                        :placeholder="$t('search')" 
                        class="w-full px-3 py-1.5 text-sm bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-white rounded border border-gray-200 dark:border-gray-600 focus:outline-none focus:border-primary-500"
                        @click.stop
                      />
                    </div>
                    <div class="max-h-72 overflow-y-auto">
                      <div v-if="!languageSearch" class="mb-2">
                        <div class="text-xs font-medium text-gray-500 dark:text-gray-400 mb-1 px-2">{{ $t('currentLanguage') }}</div>
                        <button
                          :key="currentLocale.code" 
                          @click.stop="changeLocale(currentLocale.code)" 
                          class="flex w-full items-center px-3 py-2 text-sm text-primary-600 dark:text-primary-400 bg-primary-500/10 font-medium rounded-md">
                          {{ currentLocale.name }}
                        </button>
                      </div>
                      <div v-if="!languageSearch && commonLanguages.length" class="mb-2">
                        <div class="text-xs font-medium text-gray-500 dark:text-gray-400 mb-1 px-2">{{ $t('commonLanguages') }}</div>
                        <div class="grid grid-cols-2 gap-1">
                          <button v-for="locale in commonLanguages" :key="locale.code" 
                            @click.stop="changeLocale(locale.code)" 
                            class="flex items-center px-3 py-2 text-sm text-gray-800 dark:text-white hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer rounded-md">
                            {{ locale.name }}
                          </button>
                        </div>
                      </div>
                      <div v-if="filteredLocales.length">
                        <div v-if="!languageSearch" class="text-xs font-medium text-gray-500 dark:text-gray-400 mb-1 px-2">{{ $t('allLanguages') }}</div>
                        <div class="grid grid-cols-2 gap-1">
                          <button v-for="locale in filteredLocales" :key="locale.code" 
                            @click.stop="changeLocale(locale.code)" 
                            class="flex items-center px-3 py-2 text-sm text-gray-800 dark:text-white hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer rounded-md"
                            :class="locale.code === currentLocale.code ? 'bg-primary-500/10 text-primary-600 dark:text-primary-400 font-medium' : ''">
                            {{ locale.name }}
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              
              <!-- Theme Selector -->
              <div class="relative">
                <button @click.stop="toggleThemeDropdown" class="flex items-center space-x-1 text-gray-800 dark:text-white hover:text-primary-600 dark:hover:text-primary-400">
                  <template v-if="currentTheme === 'light'">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                    </svg>
                  </template>
                  <template v-else-if="currentTheme === 'dark'">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                    </svg>
                  </template>
                  <template v-else>
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                    </svg>
                  </template>
                </button>
                
                <div v-if="isThemeDropdownOpen" class="absolute right-0 mt-2 w-40 bg-white dark:bg-gray-800 rounded-md shadow-lg overflow-hidden z-20">
                  <div class="py-1">
                    <button @click.stop="changeTheme('light')" class="flex items-center px-4 py-2 text-sm text-gray-800 dark:text-white hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer w-full text-left">
                      <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                      </svg>
                      {{ $t('theme.light') }}
                    </button>
                    <button @click.stop="changeTheme('dark')" class="flex items-center px-4 py-2 text-sm text-gray-800 dark:text-white hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer w-full text-left">
                      <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                      </svg>
                      {{ $t('theme.dark') }}
                    </button>
                    <button @click.stop="changeTheme('system')" class="flex items-center px-4 py-2 text-sm text-gray-800 dark:text-white hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer w-full text-left">
                      <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                      </svg>
                      {{ $t('theme.system') }}
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Mobile Menu Button -->
          <div class="md:hidden">
            <button @click="toggleMobileMenu" class="text-white p-2 focus:outline-none">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" v-if="!isMobileMenuOpen">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
              </svg>
              <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" v-else>
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
        
        <!-- Mobile Menu -->
        <div v-show="isMobileMenuOpen" class="md:hidden mt-4 py-4 bg-white/95 dark:bg-gray-800/90 backdrop-blur-md rounded-lg transition-all duration-300 overflow-hidden shadow-lg" 
          :class="{ 'max-h-[80vh]': isMobileMenuOpen, 'max-h-0': !isMobileMenuOpen }">
          <div class="flex flex-col space-y-4 px-4">
            <a href="#features" class="text-gray-800 dark:text-white hover:text-primary-600 dark:hover:text-primary-400 transition-colors py-2 border-b border-gray-200 dark:border-gray-700" @click="closeMobileMenu">{{ $t('nav.features') }}</a>
            <a href="#gallery" class="text-gray-800 dark:text-white hover:text-primary-600 dark:hover:text-primary-400 transition-colors py-2 border-b border-gray-200 dark:border-gray-700" @click="closeMobileMenu">{{ $t('nav.gallery') }}</a>
            <a href="#download" class="text-gray-800 dark:text-white hover:text-primary-600 dark:hover:text-primary-400 transition-colors py-2 border-b border-gray-200 dark:border-gray-700" @click="closeMobileMenu">{{ $t('nav.download') }}</a>
            
            <div class="pt-2 border-t border-gray-200 dark:border-gray-700">
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-2">{{ $t('language') }}</p>
              <div class="mb-2">
                <input 
                  v-model="mobileLanguageSearch" 
                  type="text" 
                  :placeholder="$t('search')" 
                  class="w-full px-3 py-1.5 text-sm bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-white rounded border border-gray-200 dark:border-gray-600 focus:outline-none focus:border-primary-500"
                />
              </div>
              <div class="max-h-48 overflow-y-auto pb-2">
                <div v-if="!mobileLanguageSearch" class="mb-2">
                  <div class="text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">{{ $t('currentLanguage') }}</div>
                  <button 
                    :key="currentLocale.code" 
                    @click="changeLocale(currentLocale.code)"
                    class="px-3 py-1 text-sm rounded-full bg-primary-500 text-white">
                    {{ currentLocale.name }}
                  </button>
                </div>
                <div v-if="!mobileLanguageSearch && commonLanguages.length" class="mb-2">
                  <div class="text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">{{ $t('commonLanguages') }}</div>
                  <div class="flex flex-wrap gap-2">
                    <button v-for="locale in commonLanguages" :key="locale.code" 
                      @click="changeLocale(locale.code)"
                      class="px-3 py-1 text-sm rounded-full bg-gray-100 text-gray-800 hover:bg-gray-200 dark:bg-gray-700 dark:text-white dark:hover:bg-gray-600">
                      {{ locale.name }}
                    </button>
                  </div>
                </div>
                <div v-if="filteredMobileLocales.length">
                  <div v-if="!mobileLanguageSearch" class="text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">{{ $t('allLanguages') }}</div>
                  <div class="flex flex-wrap gap-2">
                    <button v-for="locale in filteredMobileLocales" :key="locale.code" 
                      @click="changeLocale(locale.code)"
                      class="px-3 py-1 text-sm rounded-full" 
                      :class="locale.code === currentLocale.code ? 'bg-primary-500 text-white' : 'bg-gray-100 text-gray-800 hover:bg-gray-200 dark:bg-gray-700 dark:text-white dark:hover:bg-gray-600'">
                      {{ locale.name }}
                    </button>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="pt-2 pb-2 border-t border-gray-200 dark:border-gray-700">
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-2">{{ $t('theme.title') }}</p>
              <div class="flex flex-wrap gap-2">
                <button @click="changeTheme('light')" 
                  class="flex items-center px-3 py-1 text-sm rounded-full" 
                  :class="currentTheme === 'light' ? 'bg-primary-500 text-white' : 'bg-gray-100 text-gray-800 hover:bg-gray-200 dark:bg-gray-700 dark:text-white dark:hover:bg-gray-600'">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                  </svg>
                  {{ $t('theme.light') }}
                </button>
                <button @click="changeTheme('dark')" 
                  class="flex items-center px-3 py-1 text-sm rounded-full" 
                  :class="currentTheme === 'dark' ? 'bg-primary-500 text-white' : 'bg-gray-100 text-gray-800 hover:bg-gray-200 dark:bg-gray-700 dark:text-white dark:hover:bg-gray-600'">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                  </svg>
                  {{ $t('theme.dark') }}
                </button>
                <button @click="changeTheme('system')" 
                  class="flex items-center px-3 py-1 text-sm rounded-full" 
                  :class="currentTheme === 'system' ? 'bg-primary-500 text-white' : 'bg-gray-100 text-gray-800 hover:bg-gray-200 dark:bg-gray-700 dark:text-white dark:hover:bg-gray-600'">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                  </svg>
                  {{ $t('theme.system') }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </nav>
    </header>

    <main class="flex-grow">
      <slot />
    </main>

    <footer class="py-12 text-center text-gray-400 dark:text-gray-500 bg-gray-100 dark:bg-gray-800">
      <div class="container mx-auto px-6">
        <p>{{ $t('footer.copyright', { year: new Date().getFullYear() }) }}</p>
      </div>
    </footer>

    <!-- Scroll to Top Button -->
    <button id="scrollToTop" @click="scrollToTop" 
      class="fixed bottom-8 right-8 bg-primary-600 hover:bg-primary-700 text-white p-3 rounded-full shadow-lg transition-all transform hover:scale-110 z-50"
      :class="{ 'opacity-0 invisible': !showScrollTopButton, 'opacity-100': showScrollTopButton }">
      <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7" />
      </svg>
    </button>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useThemeStore } from '~/stores/theme';

// Theme
const themeStore = useThemeStore();
const currentTheme = computed(() => themeStore.currentTheme);
const isThemeDropdownOpen = ref(false);

// Language
const { t, locale, locales } = useI18n();
const currentLocale = computed(() => {
  return locales.value.find(l => l.code === locale.value);
});

// Common languages (more frequently used)
const commonLanguageCodes = ['de', 'en', 'es', 'fr', 'ja', 'ko', 'ru', 'tr', 'zh'];
const commonLanguages = computed(() => {
  return locales.value
    .filter(l => commonLanguageCodes.includes(l.code) && l.code !== locale.value)
    .sort((a, b) => a.name.localeCompare(b.name));
});

const isLanguageDropdownOpen = ref(false);
const languageSearch = ref('');
const mobileLanguageSearch = ref('');

const filteredLocales = computed(() => {
  const allLocales = languageSearch.value ? locales.value : locales.value.filter(l => !commonLanguageCodes.includes(l.code) || l.code === locale.value);
  
  if (!languageSearch.value) {
    return allLocales
      .filter(l => l.code !== locale.value)
      .sort((a, b) => a.name.localeCompare(b.name));
  }
  
  const search = languageSearch.value.toLowerCase();
  return allLocales.filter(locale => 
    locale.name.toLowerCase().includes(search) || 
    locale.code.toLowerCase().includes(search)
  ).sort((a, b) => {
    // Prioritize exact matches
    const aExactMatch = locale.name.toLowerCase() === search || locale.code.toLowerCase() === search;
    const bExactMatch = locale.name.toLowerCase() === search || locale.code.toLowerCase() === search;
    
    if (aExactMatch && !bExactMatch) return -1;
    if (!aExactMatch && bExactMatch) return 1;
    
    return a.name.localeCompare(b.name);
  });
});

const filteredMobileLocales = computed(() => {
  const allLocales = mobileLanguageSearch.value ? locales.value : locales.value.filter(l => !commonLanguageCodes.includes(l.code) || l.code === locale.value);
  
  if (!mobileLanguageSearch.value) {
    return allLocales
      .filter(l => l.code !== locale.value)
      .sort((a, b) => a.name.localeCompare(b.name));
  }
  
  const search = mobileLanguageSearch.value.toLowerCase();
  return allLocales.filter(locale => 
    locale.name.toLowerCase().includes(search) || 
    locale.code.toLowerCase().includes(search)
  ).sort((a, b) => a.name.localeCompare(b.name));
});

// Mobile menu
const isMobileMenuOpen = ref(false);

// Scroll
const isScrolled = ref(false);
const showScrollTopButton = ref(false);

const toggleThemeDropdown = () => {
  isThemeDropdownOpen.value = !isThemeDropdownOpen.value;
  isLanguageDropdownOpen.value = false;
};

const toggleLanguageDropdown = () => {
  isLanguageDropdownOpen.value = !isLanguageDropdownOpen.value;
  isThemeDropdownOpen.value = false;
};

const toggleMobileMenu = () => {
  isMobileMenuOpen.value = !isMobileMenuOpen.value;
};

const closeMobileMenu = () => {
  isMobileMenuOpen.value = false;
};

const changeTheme = (theme) => {
  themeStore.setTheme(theme);
  isThemeDropdownOpen.value = false;
};

const changeLocale = (code) => {
  // Simply change the locale and save it, then refresh the page
  locale.value = code;
  
  // Save language preference to localStorage
  if (typeof window !== 'undefined') {
    localStorage.setItem('user-locale', code);
    
    // Refresh the page - simple and effective
    window.location.reload();
  }
  
  // Close dropdowns
  isLanguageDropdownOpen.value = false;
  closeMobileMenu();
};

const handleScroll = () => {
  isScrolled.value = window.scrollY > 100;
  showScrollTopButton.value = window.scrollY > 250;
};

const scrollToTop = () => {
  window.scrollTo({
    top: 0,
    behavior: 'smooth'
  });
};

// Handle clicks
const handleClickOutside = (event) => {
  if (isThemeDropdownOpen.value || isLanguageDropdownOpen.value) {
    // Click on language search input
    const isLanguageSearchClick = event.target.closest('input[type="text"]');
    if (isLanguageSearchClick) return;
    
    isThemeDropdownOpen.value = false;
    isLanguageDropdownOpen.value = false;
  }
};

// Control dropdowns with window click event
watch(() => isThemeDropdownOpen.value || isLanguageDropdownOpen.value, (isOpen) => {
  if (isOpen) {
    setTimeout(() => {
      window.addEventListener('click', handleClickOutside);
    }, 0);
  } else {
    window.removeEventListener('click', handleClickOutside);
  }
});

onMounted(() => {
  window.addEventListener('scroll', handleScroll);
  handleScroll(); // Check status on initial load
});

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll);
  window.removeEventListener('click', handleClickOutside);
});
</script>

<style scoped>
header.scrolled {
  @apply shadow-lg;
}

/* Mobile menu transition animation */
.max-h-0 {
  max-height: 0;
  overflow: hidden;
}

@media (max-width: 767px) {
  .overflow-y-auto {
    -webkit-overflow-scrolling: touch;
  }
  
  /* Additional styles for language and theme lists in mobile menu */
  .flex-wrap {
    display: flex;
    flex-wrap: wrap;
    width: 100%;
  }
}
</style> 