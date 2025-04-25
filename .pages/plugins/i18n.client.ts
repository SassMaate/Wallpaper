import { defineNuxtPlugin } from '#app'

export default defineNuxtPlugin(({ $i18n }) => {
  // Only works on client side
  if (typeof window === 'undefined') return
  
  // Get language preference from localStorage when page loads
  const savedLocale = localStorage.getItem('user-locale')

  const supportedLocales = ['bg', 'cs', 'da', 'de', 'el', 'en', 'es', 'fi', 'fr', 'hi', 'hu', 'id', 'it', 'ja', 'ko', 'nl', 'no', 'pl', 'pt', 'ro', 'ru', 'sk', 'sv', 'th', 'tr', 'uk', 'vi', 'zh']
  
  // Set HTML lang attribute based on current locale
  const updateHtmlLang = (locale: string) => {
    // Get the iso code from the locale (which should be in format 'fr-FR', 'en-US', etc.)
    const isoMap: Record<string, string> = {
      'bg': 'bg-BG',
      'cs': 'cs-CZ',
      'da': 'da-DK', 
      'de': 'de-DE',
      'el': 'el-GR',
      'en': 'en-GB',
      'es': 'es-ES',
      'fi': 'fi-FI',
      'fr': 'fr-FR',
      'hi': 'hi-IN',
      'hu': 'hu-HU',
      'id': 'id-ID',
      'it': 'it-IT',
      'ja': 'ja-JP',
      'ko': 'ko-KR',
      'nl': 'nl-NL',
      'no': 'no-NO',
      'pl': 'pl-PL',
      'pt': 'pt-BR',
      'ro': 'ro-RO',
      'ru': 'ru-RU',
      'sk': 'sk-SK',
      'sv': 'sv-SE',
      'th': 'th-TH',
      'tr': 'tr-TR',
      'uk': 'uk-UA',
      'vi': 'vi-VN',
      'zh': 'zh-CN'
    }
    
    // Get the language code part (e.g., 'en' from 'en-US')
    const langCode = locale.split('-')[0] || locale;
    
    // Set the HTML lang attribute
    document.documentElement.setAttribute('lang', langCode);
  }
  
  // Watch for locale changes and update HTML lang
  // @ts-ignore - Function signature may depend on i18n version
  $i18n.onLanguageSwitched = (oldLocale: string, newLocale: string) => {
    updateHtmlLang(newLocale);
  }
  
  // If there is a saved language and it's a supported language
  if (savedLocale && supportedLocales.includes(savedLocale)) {
    // Set the language after the page loads (let's add a delay)
    setTimeout(() => {
      // @ts-ignore - Ignore TypeScript error
      $i18n.setLocale(savedLocale)
      // Update HTML lang attribute for the initial load
      updateHtmlLang(savedLocale)
    }, 10)
  } else {
    // Update HTML lang based on current locale
    // @ts-ignore - i18n object properties may depend on version
    const currentLocale = $i18n.locale?.value || 'en';
    updateHtmlLang(currentLocale)
  }
}) 