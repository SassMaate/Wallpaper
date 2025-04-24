<template>
  <section id="gallery" class="py-28 relative overflow-hidden">
    <!-- Background Elements -->
    <div class="absolute inset-0 bg-neutral-100 dark:bg-neutral-950">
      <div class="absolute inset-0 opacity-30 dark:opacity-20">
        <div class="absolute top-1/3 left-1/4 w-[500px] h-[500px] rounded-full bg-gradient-to-r from-accent-200 to-accent-300 dark:from-accent-900 dark:to-accent-800 blur-3xl opacity-30"></div>
        <div class="absolute bottom-1/3 right-1/4 w-[400px] h-[400px] rounded-full bg-gradient-to-l from-secondary-200 to-secondary-300 dark:from-secondary-900 dark:to-secondary-800 blur-3xl opacity-30"></div>
      </div>
    </div>
    
    <div class="container mx-auto px-6 relative">
      <!-- Section Header -->
      <div class="text-center max-w-3xl mx-auto mb-16" data-aos="fade-up">
        <span class="inline-block px-3 py-1 rounded-full bg-secondary-100 dark:bg-secondary-900/30 text-secondary-600 dark:text-secondary-400 text-sm font-medium mb-4 glass">
          {{ $t('gallery.badge') }}
        </span>
        
        <h2 class="text-4xl md:text-5xl font-display font-bold mb-6 text-neutral-900 dark:text-white">
          <span class="bg-gradient-text animate-gradient-text">{{ $t('gallery.title') }}</span>
        </h2>
        
        <p class="text-lg text-neutral-700 dark:text-neutral-300">
          {{ $t('gallery.subtitle') }}
        </p>
      </div>
      
      <!-- Main Screenshot -->
      <div class="mb-20">
        <div 
          class="relative group aspect-video rounded-3xl overflow-hidden shadow-2xl cursor-pointer" 
          data-aos="fade-up"
          @click="openLightbox(currentImageIndex)"
        >
          <!-- Main Image -->
          <img
            :src="currentImage"
            :alt="`Sucrose - ${currentImageIndex + 1}`"
            class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-105"
          >
          
          <!-- Shadow Effect -->
          <div class="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
          
          <!-- View Button -->
          <div class="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-300">
            <div class="bg-white/20 dark:bg-black/30 backdrop-blur-md rounded-full p-4 glass transform transition duration-300 hover:scale-110">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>
          
          <!-- Info Label -->
          <div class="absolute bottom-6 left-6 right-6 p-4 bg-black/30 backdrop-blur-md rounded-xl glass opacity-0 group-hover:opacity-100 transition-opacity duration-300 transform group-hover:translate-y-0 translate-y-4">
            <h3 class="text-white text-lg font-medium truncate">{{ getImageTitle(currentImageIndex) }}</h3>
          </div>
        </div>
      </div>
      
      <!-- Lightbox Modal -->
      <div 
        v-if="showLightbox" 
        class="fixed inset-0 bg-black/95 z-50 flex items-center justify-center"
        @click.self="closeLightbox"
      >
        <!-- Close Button -->
        <button 
          @click="closeLightbox"
          class="absolute top-6 right-6 text-white bg-black/30 p-2 rounded-full backdrop-blur-sm hover:bg-white/20 transition-colors z-10"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
        
        <!-- Previous Button -->
        <button 
          @click="prevImage"
          class="absolute left-6 top-1/2 transform -translate-y-1/2 text-white bg-black/30 p-3 rounded-full backdrop-blur-sm hover:bg-white/20 transition-colors group z-10"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 transform group-hover:-translate-x-1 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        
        <!-- Next Button -->
        <button 
          @click="nextImage"
          class="absolute right-6 top-1/2 transform -translate-y-1/2 text-white bg-black/30 p-3 rounded-full backdrop-blur-sm hover:bg-white/20 transition-colors group z-10"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 transform group-hover:translate-x-1 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
          </svg>
        </button>
        
        <!-- Lightbox Image -->
        <div class="max-h-[90vh] max-w-[90vw] relative">
          <div class="rounded-2xl overflow-hidden glass border border-white/10">
            <img 
              :src="images[lightboxIndex]"
              :alt="`Sucrose - ${lightboxIndex + 1}`"
              class="max-h-[80vh] max-w-[90vw] object-contain"
            >
          </div>
          
          <!-- Lightbox Info Bar -->
          <div class="absolute bottom-0 left-0 right-0 p-4 bg-black/70 backdrop-blur-sm rounded-b-2xl">
            <h3 class="text-white text-lg font-medium">{{ getImageTitle(lightboxIndex) }}</h3>
          </div>
        </div>
        
        <!-- Lightbox Thumbnails -->
        <div class="absolute bottom-8 left-1/2 transform -translate-x-1/2 hide-scrollbar overflow-x-auto whitespace-nowrap max-w-[80vw]">
          <div class="flex gap-2 justify-center">
            <div
              v-for="(image, index) in images"
              :key="index"
              @click="lightboxIndex = index"
              :class="[
                'w-16 h-10 rounded-md overflow-hidden cursor-pointer transition-all duration-300 border-2',
                lightboxIndex === index ? 'border-primary-500 scale-110' : 'border-transparent opacity-50 hover:opacity-100'
              ]"
            >
              <img :src="image" :alt="`Thumbnail ${index + 1}`" class="w-full h-full object-cover">
            </div>
          </div>
        </div>
      </div>
      
      <!-- Gallery Slider -->
      <div class="relative" data-aos="fade-up" data-aos-delay="100">
        <h3 class="text-2xl font-display font-bold mb-6 text-neutral-900 dark:text-white text-center md:text-left">
          {{ $t('gallery.collection') }}
        </h3>
        
        <div class="flex items-center">
          <!-- Left Navigation Button -->
          <button 
            class="z-10 flex-shrink-0 bg-white/10 dark:bg-black/50 hover:bg-white/20 dark:hover:bg-black/70 text-neutral-700 dark:text-white p-4 rounded-full glass shadow-lg transform transition duration-300 hover:scale-110 hover:-translate-x-1"
            @click="slideGallery('left')"
          >
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          
          <!-- Slider Container -->
          <div 
            class="flex-1 overflow-x-auto snap-x snap-mandatory hide-scrollbar mx-4 py-4" 
            ref="sliderRef"
          >
            <div class="flex gap-6">
              <div 
                v-for="(image, index) in images" 
                :key="index"
                class="flex-shrink-0 w-80 aspect-video rounded-2xl overflow-hidden cursor-pointer group snap-start relative"
                @click="selectImage(index)"
              >
                <!-- Thumbnail Image -->
                <img 
                  :src="image"
                  :alt="`Sucrose - ${index + 1}`"
                  class="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
                >
                
                <!-- Shadow Layer -->
                <div class="absolute inset-0 bg-gradient-to-t from-black/70 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
                
                <!-- Selection Indicator -->
                <div class="absolute inset-0 ring-4 ring-primary-500 ring-offset-4 ring-offset-white dark:ring-offset-neutral-950 rounded-2xl opacity-0 transition-opacity duration-300" :class="{ 'opacity-100': currentImageIndex === index }"></div>
                
                <!-- Title -->
                <div class="absolute bottom-0 left-0 right-0 p-3 bg-black/50 backdrop-blur-sm transform translate-y-full group-hover:translate-y-0 transition-transform duration-300">
                  <h4 class="text-white font-medium truncate text-sm">{{ getImageTitle(index) }}</h4>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Right Navigation Button -->
          <button 
            class="z-10 flex-shrink-0 bg-white/10 dark:bg-black/50 hover:bg-white/20 dark:hover:bg-black/70 text-neutral-700 dark:text-white p-4 rounded-full glass shadow-lg transform transition duration-300 hover:scale-110 hover:translate-x-1"
            @click="slideGallery('right')"
          >
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </div>
      </div>
      
      <!-- Pagination Dots -->
      <div class="flex justify-center mt-8">
        <div class="flex space-x-2">
          <button
            v-for="(_, index) in images"
            :key="index"
            @click="selectImage(index)"
            :class="[
              'w-3 h-3 rounded-full transition-all duration-300',
              currentImageIndex === index ? 'bg-primary-500 w-6' : 'bg-neutral-300 dark:bg-neutral-600 hover:bg-neutral-400 dark:hover:bg-neutral-500'
            ]"
          ></button>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

// Image gallery settings
const images = [
  '/images/Preview.png',
  '/images/Store.png',
  '/images/Alves-Town.png',
  '/images/Fluid-Simulation.png',
  '/images/Neo-Matrix-1.png',
  '/images/Media-Parallax.png',
  '/images/Living-Room.png',
  '/images/Periodic-Table.png',
  '/images/Inquisition.png',
  '/images/Simple-System.png'
];

// Image titles (should be i18n in real project)
const imageTitles = {
  0: 'preview',
  1: 'store',
  2: 'alves_town',
  3: 'fluid_simulation',
  4: 'neo_matrix',
  5: 'media_parallax',
  6: 'living_room',
  7: 'periodic_table',
  8: 'inquisition',
  9: 'simple_system'
};

const getImageTitle = (index) => {
  const key = imageTitles[index];
  return key ? t(`gallery.images.${key}`) : `Sucrose Wallpaper ${index + 1}`;
};

const currentImageIndex = ref(0);
const imageLoading = ref(false);
const currentImage = computed(() => images[currentImageIndex.value]);

// Lightbox settings
const showLightbox = ref(false);
const lightboxIndex = ref(0);

// Slider reference
const sliderRef = ref(null);

// Select image
const selectImage = (index) => {
  currentImageIndex.value = index;
};

// Lightbox functions
const openLightbox = (index) => {
  lightboxIndex.value = index;
  showLightbox.value = true;
  document.body.classList.add('overflow-hidden');
  
  // Hide scroll to top button
  const scrollToTopButton = document.getElementById('scrollToTop');
  if (scrollToTopButton) {
    scrollToTopButton.classList.add('opacity-0', 'invisible');
  }
};

const closeLightbox = () => {
  showLightbox.value = false;
  document.body.classList.remove('overflow-hidden');
  
  // Show scroll to top button again (if scroll position is appropriate)
  const scrollToTopButton = document.getElementById('scrollToTop');
  if (scrollToTopButton && window.scrollY > 250) {
    scrollToTopButton.classList.remove('opacity-0', 'invisible');
  }
};

const nextImage = () => {
  lightboxIndex.value = (lightboxIndex.value + 1) % images.length;
};

const prevImage = () => {
  lightboxIndex.value = (lightboxIndex.value - 1 + images.length) % images.length;
};

// Gallery scroll function
const slideGallery = (direction) => {
  if (!sliderRef.value) return;
  
  const scrollAmount = sliderRef.value.clientWidth * 0.8; // 80% of viewport
  
  if (direction === 'left') {
    sliderRef.value.scrollBy({
      left: -scrollAmount,
      behavior: 'smooth'
    });
  } else {
    sliderRef.value.scrollBy({
      left: scrollAmount,
      behavior: 'smooth'
    });
  }
};

// Keyboard navigation support
const handleKeyDown = (e) => {
  if (showLightbox.value) {
    if (e.key === 'Escape') {
      closeLightbox();
    } else if (e.key === 'ArrowLeft') {
      prevImage();
    } else if (e.key === 'ArrowRight') {
      nextImage();
    }
  } else {
    if (e.key === 'ArrowLeft') {
      slideGallery('left');
    } else if (e.key === 'ArrowRight') {
      slideGallery('right');
    }
  }
};

onMounted(() => {
  document.addEventListener('keydown', handleKeyDown);
});

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeyDown);
});
</script> 