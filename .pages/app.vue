<template>
  <div :class="{ 'dark': isDark }">
    <div class="bg-white dark:bg-gray-900 min-h-screen">
      <NuxtLayout>
        <NuxtPage />
      </NuxtLayout>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, watch } from 'vue';
import { useThemeStore } from '~/stores/theme';
import { useI18n } from 'vue-i18n';

const themeStore = useThemeStore();
const isDark = computed(() => themeStore.isDark);
const { locale } = useI18n();

// Watch for language changes and save to localStorage
watch(locale, (newLocale) => {
  if (typeof window !== 'undefined') {
    localStorage.setItem('user-locale', newLocale);
  }
});

// Initialize theme settings when page loads
onMounted(() => {
  themeStore.initTheme();
});
</script>