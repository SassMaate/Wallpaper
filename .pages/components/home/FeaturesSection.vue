<template>
  <section id="features" class="py-28 relative overflow-hidden">
    <!-- Background Elements -->
    <div class="absolute inset-0 bg-neutral-50 dark:bg-neutral-900">
      <div class="absolute inset-0 opacity-20 dark:opacity-10">
        <div class="absolute -top-40 -right-40 w-[600px] h-[600px] rounded-full bg-gradient-to-br from-primary-200 to-primary-400 dark:from-primary-900 dark:to-primary-700 blur-3xl"></div>
        <div class="absolute -bottom-40 -left-40 w-[600px] h-[600px] rounded-full bg-gradient-to-tr from-secondary-200 to-secondary-400 dark:from-secondary-900 dark:to-secondary-700 blur-3xl"></div>
      </div>
    </div>
    
    <div class="container mx-auto px-6 relative">
      <!-- Section Header -->
      <div class="text-center max-w-3xl mx-auto mb-20" data-aos="fade-up">
        <span class="inline-block px-3 py-1 rounded-full bg-primary-100 dark:bg-primary-900/30 text-primary-600 dark:text-primary-400 text-sm font-medium mb-4 glass">
          {{ $t('features.badge') }}
        </span>
        
        <h2 class="text-4xl md:text-5xl font-display font-bold mb-6 text-neutral-900 dark:text-white">
          <span class="bg-gradient-text animate-gradient-text">{{ $t('features.title') }}</span>
        </h2>
        
        <p class="text-lg text-neutral-700 dark:text-neutral-300">
          {{ $t('features.subtitle') }}
        </p>
      </div>
      
      <!-- Features Grid with Masonry Layout -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
        <!-- Customizable Themes -->
        <div class="md:col-span-2 lg:col-span-1 md:row-span-2 lg:row-span-1" data-aos="fade-up" data-aos-delay="100">
          <FeatureCard 
            icon="🎨"
            :title="$t('features.customizable.title')"
            :description="$t('features.customizable.description')"
            size="large"
          />
        </div>
        
        <!-- Performance Optimized -->
        <div data-aos="fade-up" data-aos-delay="200">
          <FeatureCard 
            icon="⚡"
            :title="$t('features.performance.title')"
            :description="$t('features.performance.description')"
          />
        </div>
        
        <!-- Multi-Screen Support -->
        <div data-aos="fade-up" data-aos-delay="300">
          <FeatureCard 
            icon="🖥️"
            :title="$t('features.multiscreen.title')"
            :description="$t('features.multiscreen.description')"
          />
        </div>
        
        <!-- Audio Responsive -->
        <div class="md:col-span-2 lg:col-span-1" data-aos="fade-up" data-aos-delay="400">
          <FeatureCard 
            icon="🎧"
            :title="$t('features.audio.title')"
            :description="$t('features.audio.description')"
            size="medium"
          />
        </div>
        
        <!-- Web Integration -->
        <div data-aos="fade-up" data-aos-delay="500">
          <FeatureCard 
            icon="🌐"
            :title="$t('features.web.title')"
            :description="$t('features.web.description')"
          />
        </div>
        
        <!-- Auto Switching -->
        <div data-aos="fade-up" data-aos-delay="600">
          <FeatureCard 
            icon="🔄"
            :title="$t('features.switching.title')"
            :description="$t('features.switching.description')"
          />
        </div>
        
        <!-- System Integration -->
        <div data-aos="fade-up" data-aos-delay="700">
          <FeatureCard 
            icon="📊"
            :title="$t('features.system.title')"
            :description="$t('features.system.description')"
          />
        </div>
        
        <!-- Community Sharing -->
        <div data-aos="fade-up" data-aos-delay="800">
          <FeatureCard 
            icon="🤝"
            :title="$t('features.community.title')"
            :description="$t('features.community.description')"
          />
        </div>
        
        <!-- Completely Free -->
        <div data-aos="fade-up" data-aos-delay="900">
          <FeatureCard 
            icon="💫"
            :title="$t('features.free.title')"
            :description="$t('features.free.description')"
          />
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { useI18n } from 'vue-i18n';
import FeatureCard from '~/components/utils/FeatureCard.vue';

const { t } = useI18n();
</script> 