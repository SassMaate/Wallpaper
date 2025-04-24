<template>
  <div :class="[
    'relative group',
    size === 'large' ? 'h-full' : '',
    size === 'medium' ? 'md:col-span-2' : ''
  ]">
    <div class="absolute inset-0 bg-gradient-to-r from-primary-400/20 via-secondary-400/20 to-accent-400/20 rounded-2xl blur-xl opacity-0 group-hover:opacity-70 transition-opacity duration-500"></div>
    <div :class="[
      'p-8 relative rounded-2xl bg-white/90 dark:bg-gray-800/90 backdrop-blur-sm shadow-lg dark:shadow-none glass transition-all duration-300 group-hover:translate-y-[-5px] overflow-hidden z-10 h-full',
      size === 'large' ? 'p-10' : 'p-8',
      size === 'large' ? 'md:flex md:flex-col md:justify-between' : ''
    ]">
      <div class="absolute inset-0 bg-gradient-to-r from-primary-500/10 via-secondary-500/10 to-accent-500/10 opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
      <div class="absolute -bottom-1 -right-1 w-24 h-24 bg-gradient-to-tl from-primary-500/20 to-secondary-500/0 rounded-full filter blur-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-300 transform group-hover:scale-150"></div>
      
      <div class="relative z-10">
        <div :class="[
          'flex items-center justify-center text-4xl mb-6 bg-gradient-to-r from-primary-500 to-secondary-500 text-white rounded-2xl shadow-md',
          size === 'large' ? 'w-20 h-20 text-5xl' : 'w-16 h-16 text-4xl',
          size === 'medium' ? 'w-18 h-18 text-4xl' : ''
        ]">{{ icon }}</div>
        <h3 :class="[
          'font-display font-bold mb-4 text-gray-900 dark:text-white',
          size === 'large' ? 'text-2xl' : 'text-xl',
          size === 'medium' ? 'text-xl' : ''
        ]">{{ title }}</h3>
        <p :class="[
          'text-gray-600 dark:text-gray-300',
          size === 'large' ? 'text-lg' : '',
        ]">{{ description }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  icon: {
    type: String,
    required: true
  },
  title: {
    type: String,
    required: true
  },
  description: {
    type: String,
    required: true
  },
  size: {
    type: String,
    default: 'normal',
    validator: (value) => ['normal', 'medium', 'large'].includes(value)
  }
});
</script> 