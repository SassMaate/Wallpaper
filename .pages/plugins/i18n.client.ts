import { defineNuxtPlugin } from '#app'

export default defineNuxtPlugin(({ $i18n }) => {
  // Only works on client side
  if (typeof window === 'undefined') return
  
  // Get language preference from localStorage when page loads
  const savedLocale = localStorage.getItem('user-locale')
  
  // If there is a saved language and it's a supported language
  if (savedLocale && ['bg', 'cs', 'da', 'de', 'el', 'en', 'es', 'fi', 'fr', 'hi', 'hu', 'id', 'it', 'ja', 'ko', 'nl', 'no', 'pl', 'pt', 'ro', 'ru', 'sk', 'sv', 'th', 'tr', 'uk', 'vi', 'zh'].includes(savedLocale)) {
    // Set the language after the page loads (let's add a delay)
    setTimeout(() => {
      // @ts-ignore - Ignore TypeScript error
      $i18n.setLocale(savedLocale)
    }, 10)
  }
}) 