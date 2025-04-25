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

import HeroSection from '~/components/home/HeroSection.vue';
import GallerySection from '~/components/home/GallerySection.vue';
import DownloadSection from '~/components/home/DownloadSection.vue';
import FeaturesSection from '~/components/home/FeaturesSection.vue';

// SEO meta tags
const { t, locale } = useI18n();

// Set SEO meta tags
useHead({
  title: t('seo.global.title'),
  meta: [
    { httpEquiv: 'X-UA-Compatible', content: 'IE=edge' },
    { name: 'keywords', content: t('seo.global.keywords') },
    { name: 'description', content: t('seo.global.description') },
    
    // Open Graph meta tags
    { property: 'og:locale', content: locale.value },
    { property: 'og:type', content: t(`seo.global.og_type`) },
    { property: 'og:title', content: t(`seo.global.og_title`) },
    { property: 'og:site_name', content: t(`seo.global.og_site_name`) },
    { property: 'og:description', content: t(`seo.global.og_description`) },

    // Twitter meta tags
    { name: 'twitter:site', content: '@Taiizor' },
    { name: 'twitter:creator', content: '@Taiizor' },
    { name: 'twitter:title', content: t(`seo.global.og_title`) },
    { name: 'twitter:card', content: t(`seo.global.twitter_card`) },
    { name: 'twitter:description', content: t(`seo.global.og_description`) },

    { name: 'language', content: locale.value },
    { name: 'robots', content: 'index, follow' }
  ]
});
</script> 