<template>
  <div>
    <!-- Hero Section -->
    <HeroSection />
    
    <!-- Features Section -->
    <FeaturesSection />
    
    <!-- Gallery Section -->
    <GallerySection />
    
    <!-- Download Section -->
    <DownloadSection />
  </div>
</template>

<script setup>
import { useHead } from '#app';
import { useI18n } from 'vue-i18n';
import { computed } from 'vue';

import HeroSection from '~/components/home/HeroSection.vue';
import GallerySection from '~/components/home/GallerySection.vue';
import DownloadSection from '~/components/home/DownloadSection.vue';
import FeaturesSection from '~/components/home/FeaturesSection.vue';

// SEO meta tags
const { t, locale } = useI18n();

// Page type
const pageType = 'home'; 

// Title and description
const title = computed(() => t(`seo.pages.${pageType}.title`));
const description = computed(() => t(`seo.pages.${pageType}.description`));

// Set SEO meta tags
useHead({
  title,
  meta: [
    { name: 'description', content: description },
    { httpEquiv: 'X-UA-Compatible', content: 'IE=edge' },
    { name: 'keywords', content: t('seo.global.keywords') },
    
    // Open Graph meta tags
    { property: 'og:title', content: title },
    { property: 'og:type', content: 'website' },
    { property: 'og:locale', content: locale.value },
    { property: 'og:description', content: description },
    { property: 'og:site_name', content: 'Sucrose Wallpaper Engine' },

    // Twitter meta tags
    { name: 'twitter:title', content: title },
    { name: 'twitter:site', content: '@Taiizor' },
    { name: 'twitter:creator', content: '@Taiizor' },
    { name: 'twitter:description', content: description },
    { name: 'twitter:card', content: 'summary_large_image' },

    { name: 'language', content: locale.value },
    { name: 'robots', content: 'index, follow' }
  ]
});
</script> 