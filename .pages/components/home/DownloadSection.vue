<template>
  <section id="download" class="py-24 bg-gray-100 dark:bg-gray-800">
    <div class="container mx-auto px-6 text-center">
      <h2 class="text-4xl font-bold mb-8 text-gray-900 dark:text-white" data-aos="fade-up">
        {{ $t('download.title') }}
      </h2>
      
      <p class="text-xl text-gray-600 dark:text-gray-300 mb-12 max-w-2xl mx-auto" data-aos="fade-up" data-aos-delay="100">
        {{ $t('download.subtitle') }}
      </p>
      
      <div class="flex flex-col md:flex-row justify-center gap-6" data-aos="fade-up" data-aos-delay="200">
        <a 
          href="https://apps.microsoft.com/detail/XP8JGPBHTJGLCQ?launch=true&mode=full" 
          target="_blank" 
          class="bg-primary-600 hover:bg-primary-700 text-white font-bold py-4 px-8 rounded-full inline-block transition-all transform hover:scale-105 hover:shadow-lg hover:shadow-primary-500/25"
        >
          {{ $t('download.ms_store') }}
        </a>
        
        <a 
          href="https://github.com/Taiizor/Sucrose?tab=readme-ov-file#download" 
          target="_blank" 
          class="bg-gray-800 hover:bg-gray-700 text-white font-bold py-4 px-8 rounded-full inline-block transition-all transform hover:scale-105 border-2 border-primary-500/50 hover:border-primary-500 dark:bg-gray-700 dark:hover:bg-gray-600"
        >
          {{ $t('download.github') }}
        </a>
        
        <a 
          href="https://github.com/Taiizor/Sucrose" 
          target="_blank" 
          class="bg-gray-200 hover:bg-gray-300 text-gray-800 dark:bg-gray-700 dark:hover:bg-gray-600 dark:text-white font-bold py-4 px-8 rounded-full inline-block transition-all transform hover:scale-105"
        >
          {{ $t('download.source') }}
        </a>
      </div>
    </div>
  </section>
</template>

<script setup>
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script> 