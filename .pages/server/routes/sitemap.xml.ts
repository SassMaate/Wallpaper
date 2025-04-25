import { defineEventHandler } from 'h3'

export default defineEventHandler(async (event) => {
  // Set your site URL here
  const baseURL = 'https://taiizor.github.io/Sucrose'
  
  // Define only the main page for single-page site structure
  // Sections within the page (features, gallery, download) are accessed via anchors
  // so they don't need to be defined as separate URLs
  const routes = [
    { url: '/', changefreq: 'weekly', priority: 1.0 },
  ]

  // Supported languages
  const languages = [
    'bg', 'cs', 'da', 'de', 'el', 'en', 'es', 'fi', 'fr', 
    'hi', 'hu', 'id', 'it', 'ja', 'ko', 'nl', 'no', 'pl', 
    'pt', 'ro', 'ru', 'sk', 'sv', 'th', 'tr', 'uk', 'vi', 'zh'
  ]

  // Current date
  const currentDate = new Date().toISOString()

  // XML header
  let xml = '<?xml version="1.0" encoding="UTF-8"?>\n'
  xml += '<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:xhtml="http://www.w3.org/1999/xhtml">\n'

  // Create sitemap entry for home page and language versions
  for (const route of routes) {
    // URL for default language (en)
    xml += '  <url>\n'
    xml += `    <loc>${baseURL}${route.url}</loc>\n`
    xml += `    <lastmod>${currentDate}</lastmod>\n`
    xml += `    <changefreq>${route.changefreq}</changefreq>\n`
    xml += `    <priority>${route.priority.toFixed(1)}</priority>\n`
    
    // Add xhtml:link for each language alternative
    for (const lang of languages) {
      const langPath = lang === 'en' ? route.url : `/${lang}${route.url}`
      xml += `    <xhtml:link rel="alternate" hreflang="${lang}" href="${baseURL}${langPath}" />\n`
    }
    
    // Add x-default for default
    xml += `    <xhtml:link rel="alternate" hreflang="x-default" href="${baseURL}${route.url}" />\n`
    xml += '  </url>\n'
    
    // Create URLs for other languages
    for (const lang of languages) {
      if (lang === 'en') continue; // English was already handled as the main URL
      
      xml += '  <url>\n'
      xml += `    <loc>${baseURL}/${lang}</loc>\n`
      xml += `    <lastmod>${currentDate}</lastmod>\n`
      xml += `    <changefreq>${route.changefreq}</changefreq>\n`
      xml += `    <priority>${route.priority.toFixed(1)}</priority>\n`
      
      // Add xhtml:link for each language alternative
      for (const altLang of languages) {
        const altLangPath = altLang === 'en' ? '/' : `/${altLang}`
        xml += `    <xhtml:link rel="alternate" hreflang="${altLang}" href="${baseURL}${altLangPath}" />\n`
      }
      
      // Add x-default for default
      xml += `    <xhtml:link rel="alternate" hreflang="x-default" href="${baseURL}/" />\n`
      xml += '  </url>\n'
    }
  }

  xml += '</urlset>'

  // Set HTTP headers
  event.node.res.setHeader('Content-Type', 'application/xml')
  
  return xml
}) 