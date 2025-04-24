<template>
  <section class="min-h-screen flex items-center justify-center relative overflow-hidden bg-gradient-to-b from-neutral-50 to-neutral-100 dark:from-neutral-900 dark:to-neutral-950">
    <!-- Decorative Elements -->
    <div class="absolute inset-0 z-0">
      <!-- Background Gradient Circles -->
      <div class="absolute top-1/4 left-1/4 w-96 h-96 bg-primary-400/30 dark:bg-primary-600/20 rounded-full filter blur-3xl"></div>
      <div class="absolute bottom-1/4 right-1/4 w-96 h-96 bg-secondary-400/30 dark:bg-secondary-600/20 rounded-full filter blur-3xl"></div>
      <div class="absolute top-1/2 right-1/3 w-64 h-64 bg-accent-400/20 dark:bg-accent-600/20 rounded-full filter blur-3xl"></div>
    </div>
    
    <!-- Background Pattern -->
    <div class="absolute inset-0 bg-grid-pattern opacity-5 z-10"></div>
    
    <!-- Background Image with Modern Overlay -->
    <div class="absolute inset-0 z-5 opacity-10 dark:opacity-20">
      <img :src="'images/Promo.png'" alt="Background" class="w-full h-full object-cover">
    </div>
    
    <!-- Content -->
    <div class="container mx-auto px-6 relative z-30 flex flex-col lg:flex-row items-center">
      <!-- Text Content -->
      <div class="w-full lg:w-1/2 text-center lg:text-left mb-12 lg:mb-0" data-aos="fade-right">
        <div class="inline-block px-3 py-1 rounded-full bg-primary-100 dark:bg-primary-900/30 text-primary-600 dark:text-primary-400 text-sm font-medium mb-6 glass">
          {{ $t('hero.tagline') }}
        </div>
      
        <h1 class="text-5xl lg:text-6xl xl:text-7xl font-display font-bold mb-8 leading-tight">
          <span class="bg-gradient-text animate-gradient-text">{{ $t('hero.title') }}</span>
        </h1>
        
        <p class="text-xl text-neutral-700 dark:text-neutral-300 mb-10 max-w-2xl mx-auto lg:mx-0">
          {{ $t('hero.subtitle') }}
        </p>
        
        <div class="flex flex-col sm:flex-row gap-6 justify-center lg:justify-start items-center">
          <a 
            href="https://apps.microsoft.com/detail/XP8JGPBHTJGLCQ?launch=true&mode=full" 
            target="_blank" 
            class="relative group overflow-hidden rounded-full"
          >
            <div class="absolute inset-0 bg-gradient-to-r from-primary-500 via-secondary-500 to-accent-500 opacity-100 group-hover:opacity-90 transition-opacity"></div>
            <div class="absolute inset-0.5 bg-white dark:bg-neutral-950 rounded-full group-hover:inset-1 transition-all duration-300"></div>
            <div class="relative flex items-center justify-center gap-2 py-4 px-8 font-bold text-neutral-900 dark:text-white group-hover:text-primary-500 transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 24 24" fill="currentColor">
                <path d="M11.5 2.5v9h-9v-9h9zm0 10v9h-9v-9h9zm1-10h9v9h-9v-9zm0 10h9v9h-9v-9z" />
              </svg>
              {{ $t('hero.download_ms') }}
            </div>
          </a>
          
          <a 
            href="https://github.com/Taiizor/Sucrose?tab=readme-ov-file#download" 
            target="_blank" 
            class="relative group overflow-hidden"
          >
            <div class="absolute inset-0 bg-gradient-to-r from-neutral-200 to-neutral-300 dark:from-neutral-700 dark:to-neutral-800 rounded-full opacity-100 group-hover:opacity-90 transition-opacity"></div>
            <div class="absolute inset-0.5 bg-white dark:bg-neutral-950 rounded-full group-hover:inset-1 transition-all duration-300"></div>
            <div class="relative flex items-center justify-center gap-2 py-4 px-8 font-bold text-neutral-800 dark:text-neutral-300 group-hover:text-accent-500 transition-colors">
              <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path fill-rule="evenodd" d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z" clip-rule="evenodd" />
              </svg>
              {{ $t('hero.download_github') }}
            </div>
          </a>
        </div>
      </div>
      
      <!-- Hero Image -->
      <div class="w-full lg:w-1/2" data-aos="fade-left" data-aos-delay="200">
        <div class="relative mx-auto max-w-md lg:max-w-full">
          <!-- Decorative Elements for Image -->
          <div class="absolute -top-4 -left-4 w-24 h-24 bg-primary-500/30 rounded-full blur-xl"></div>
          <div class="absolute -bottom-4 -right-4 w-32 h-32 bg-secondary-500/30 rounded-full blur-xl"></div>
          
          <!-- Image Container with Glassmorphism -->
          <div class="relative rounded-2xl overflow-hidden shadow-2xl border border-white/20 dark:border-white/5 glass">
            <div class="aspect-video w-full">
              <iframe 
                src="https://www.youtube.com/embed/ZW-ndasFRUU" 
                title="Sucrose Video"
                class="w-full h-full rounded-2xl"
                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" 
                allowfullscreen
              ></iframe>
            </div>
            
            <!-- Reflection Overlay -->
            <div class="absolute inset-0 bg-gradient-to-t from-white/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none"></div>
          </div>
          
          <!-- Feature Badge -->
          <div class="absolute top-4 -right-4 bg-accent-500 text-white px-4 py-2 rounded-lg shadow-lg rotate-12 transform transition duration-300 hover:rotate-0 hover:scale-110">
            <span class="font-bold">!</span>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Scroll Indicator -->
    <div class="absolute inset-x-0 bottom-8 flex justify-center items-center z-50 animate-bounce pointer-events-none">
      <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10 text-neutral-500 dark:text-neutral-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 14l-7 7m0 0l-7-7m7 7V3" />
      </svg>
    </div>
  </section>
</template>

<script setup>
import { useI18n } from 'vue-i18n';
import { onMounted } from 'vue';

const { t } = useI18n();

// Make sure the image is loaded
onMounted(() => {
  // Preload the image
  const img = new Image();
  img.src = 'images/Promo.png';
});
</script>

<style scoped>
.bg-grid-pattern {
  background-image: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.2'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
}
</style> 