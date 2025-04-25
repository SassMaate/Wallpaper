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
  runtimeConfig: {
    public: {
      baseURL: process.env.NUXT_PUBLIC_BASE_URL || '/Sucrose'
    }
  },
  tailwindcss: {
    viewer: true,
    exposeConfig: true,
    cssPath: '~/assets/css/main.css',
    configPath: '~/tailwind.config.js'
  },
  colorMode: {
    classPrefix: '',
    classSuffix: '',
    fallback: 'light',
    preference: 'system',
    storageKey: 'color-mode'
  },
  i18n: {
    lazy: true,
    defaultLocale: 'en',
    langDir: 'locales/',
    detectBrowserLanguage: {
      useCookie: true,
      cookieKey: 'i18n_redirected',
      redirectOn: 'root'
    },
    strategy: 'prefix_except_default',
    skipSettingLocaleOnNavigate: false,
    baseUrl: 'https://taiizor.github.io/Sucrose', // Add your site URL here for SEO
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
    ]
  },
  css: [
    'aos/dist/aos.css',
    '~/assets/css/main.css'
  ],
  app: {
    head: {
      htmlAttrs: {
        lang: 'en'
      },
      charset: 'utf-8',
      title: 'Sucrose Wallpaper Engine',
      viewport: 'width=device-width, initial-scale=1.0',
      meta: [
        { name: 'author', content: 'Taiizor' },
        { name: 'format-detection', content: 'telephone=no' },

        { property: 'og:image:width', content: '512' },
        { property: 'og:image:height', content: '512' },
        { property: 'og:image:type', content: 'image/png' },
        { property: 'og:image', content: '/Sucrose/images/Logo.png' },
        { property: 'og:url', content: 'https://taiizor.github.io/Sucrose' },
        { property: 'og:image:alt', content: 'Sucrose is a versatile wallpaper engine that brings life to your desktop with a wide range of interactive wallpapers.' },

        { property: 'twitter:image', content: '/Sucrose/images/Logo.png' },
        { property: 'twitter:url', content: 'https://taiizor.github.io/Sucrose' },
        { property: 'twitter:image:alt', content: 'Sucrose is a versatile wallpaper engine that brings life to your desktop with a wide range of interactive wallpapers.' }
      ],
      link: [
        { rel: 'canonical', href: 'https://taiizor.github.io/Sucrose' },
        { rel: 'icon', type: 'image/x-icon', href: '/Sucrose/favicon.ico' }
      ],
      script: [
        // Microsoft Clarity
        {
          innerHTML: `
            (function(c, l, a, r, i, t, y) {
                c[a] = c[a] || function() {
                    (c[a].q = c[a].q || []).push(arguments)
                };
                t = l.createElement(r);
                t.async = 1;
                t.src = "https://www.clarity.ms/tag/" + i;
                y = l.getElementsByTagName(r)[0];
                y.parentNode.insertBefore(t, y);
            })(window, document, "clarity", "script", "pl85dw6k1x");
          `,
          type: 'text/javascript'
        },
        // Google Analytics
        {
          src: 'https://www.googletagmanager.com/gtag/js?id=G-5DZ26CZWDD',
          async: true
        },
        {
          innerHTML: `
            window.dataLayer = window.dataLayer || [];
            function gtag() {
                dataLayer.push(arguments);
            }
            gtag('js', new Date());
            gtag('config', 'G-5DZ26CZWDD');
          `,
          type: 'text/javascript'
        },
        // Yandex Metrika
        {
          innerHTML: `
            (function(m, e, t, r, i, k, a) {
                m[i] = m[i] || function() {
                    (m[i].a = m[i].a || []).push(arguments)
                };
                m[i].l = 1 * new Date();
                for (var j = 0; j < document.scripts.length; j++) {
                    if (document.scripts[j].src === r) {
                        return;
                    }
                }
                k = e.createElement(t), a = e.getElementsByTagName(t)[0], k.async = 1, k.src = r, a.parentNode.insertBefore(k, a)
            })
            (window, document, "script", "https://mc.yandex.ru/metrika/tag.js", "ym");
            ym(99351025, "init", {
                webvisor: true,
                clickmap: true,
                trackLinks: true,
                accurateTrackBounce: true
            });
          `,
          type: 'text/javascript'
        }
      ],
      noscript: [
        {
          innerHTML: `<div><img src="https://mc.yandex.ru/watch/99351025" style="position:absolute; left:-9999px;" alt="Yandex Metrika" /></div>`,
          tagPosition: 'bodyClose'
        }
      ]
    },
    baseURL: '/Sucrose/',
    cdnURL: 'https://taiizor.github.io/Sucrose'
  },
  plugins: [],
  nitro: {
    prerender: {
      routes: ['/sitemap.xml']
    }
  },
  experimental: {
    payloadExtraction: true // To solve loading issues
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