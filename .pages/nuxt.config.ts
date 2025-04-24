// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  ssr: true, // For client-side rendering
  compatibilityDate: '2025-04-25',
  devtools: { enabled: true },
  modules: [
    '@nuxtjs/tailwindcss',
    '@nuxtjs/color-mode',
    '@nuxtjs/i18n',
    '@pinia/nuxt'
  ],
  tailwindcss: {
    cssPath: '~/assets/css/main.css',
    configPath: '~/tailwind.config.js',
    exposeConfig: true,
    viewer: true
  },
  colorMode: {
    classSuffix: '',
    fallback: 'light',
    preference: 'system',
    classPrefix: '',
    storageKey: 'color-mode'
  },
  i18n: {
    lazy: true,
    strategy: 'prefix_except_default',
    defaultLocale: 'en',
    detectBrowserLanguage: false,
    skipSettingLocaleOnNavigate: false,
    langDir: 'locales/',
    locales: [
      { code: 'bg', name: 'Български', file: 'bg.json', iso: 'bg-BG' },
      { code: 'cs', name: 'Čeština', file: 'cs.json', iso: 'cs-CZ' },
      { code: 'da', name: 'Dansk', file: 'da.json', iso: 'da-DK' },
      { code: 'de', name: 'Deutsch', file: 'de.json', iso: 'de-DE' },
      { code: 'el', name: 'Ελληνικά', file: 'el.json', iso: 'el-GR' },
      { code: 'en', name: 'English', file: 'en.json', iso: 'en-GB' },
      { code: 'es', name: 'Español', file: 'es.json', iso: 'es-ES' },
      { code: 'fi', name: 'Suomi', file: 'fi.json', iso: 'fi-FI' },
      { code: 'fr', name: 'Français', file: 'fr.json', iso: 'fr-FR' },
      { code: 'hi', name: 'हिन्दी', file: 'hi.json', iso: 'hi-IN' },
      { code: 'hu', name: 'Magyar', file: 'hu.json', iso: 'hu-HU' },
      { code: 'id', name: 'Bahasa Indonesia', file: 'id.json', iso: 'id-ID' },
      { code: 'it', name: 'Italiano', file: 'it.json', iso: 'it-IT' },
      { code: 'ja', name: '日本語', file: 'ja.json', iso: 'ja-JP' },
      { code: 'ko', name: '한국어', file: 'ko.json', iso: 'ko-KR' },
      { code: 'nl', name: 'Nederlands', file: 'nl.json', iso: 'nl-NL' },
      { code: 'no', name: 'Norsk', file: 'no.json', iso: 'no-NO' },
      { code: 'pl', name: 'Polski', file: 'pl.json', iso: 'pl-PL' },
      { code: 'pt', name: 'Português', file: 'pt.json', iso: 'pt-BR' },
      { code: 'ro', name: 'Română', file: 'ro.json', iso: 'ro-RO' },
      { code: 'ru', name: 'Русский', file: 'ru.json', iso: 'ru-RU' },
      { code: 'sk', name: 'Slovenčina', file: 'sk.json', iso: 'sk-SK' },
      { code: 'sv', name: 'Svenska', file: 'sv.json', iso: 'sv-SE' },
      { code: 'th', name: 'ไทย', file: 'th.json', iso: 'th-TH' },
      { code: 'tr', name: 'Türkçe', file: 'tr.json', iso: 'tr-TR' },
      { code: 'uk', name: 'Українська', file: 'uk.json', iso: 'uk-UA' },
      { code: 'vi', name: 'Tiếng Việt', file: 'vi.json', iso: 'vi-VN' },
      { code: 'zh', name: '中文', file: 'zh.json', iso: 'zh-CN' }
    ],
    baseUrl: 'https://taiizor.github.io/Sucrose' // Add your site URL here for SEO
  },
  css: [
    '~/assets/css/main.css',
    'aos/dist/aos.css'
  ],
  app: {
    head: {
      htmlAttrs: {
        lang: 'en'
      },
      charset: 'utf-8',
      viewport: 'width=device-width, initial-scale=1',
      title: 'Sucrose Wallpaper Engine',
      meta: [
        { name: 'description', content: 'Sucrose is a versatile wallpaper engine that brings life to your desktop with a wide range of interactive wallpapers.' },
        { name: 'format-detection', content: 'telephone=no' }
      ],
      link: [
        { rel: 'icon', type: 'image/x-icon', href: './favicon.ico' }
      ]
    }
  },
  plugins: [],
  nitro: {
    prerender: {
      routes: ['/sitemap.xml']
    }
  },
  experimental: {
    payloadExtraction: false // To solve loading issues
  },
  vite: {
    optimizeDeps: {
      include: ['vue', 'pinia', 'vue-i18n']
    }
  },
  build: {
    transpile: ['vue-i18n']
  }
})