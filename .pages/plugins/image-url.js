// Plugin to handle image URLs correctly regardless of i18n route changes

export default defineNuxtPlugin((nuxtApp) => {
  const config = useRuntimeConfig();
  const baseURL = config.public.baseURL;
  
  // Function to build correct image URLs
  const buildImageUrl = (imagePath) => {
    // Remove any leading slash from the image path
    const cleanPath = imagePath.replace(/^\//, '');
    return `${baseURL}/${cleanPath}`;
  };
  
  // Provide the function to all components
  nuxtApp.provide('buildImageUrl', buildImageUrl);
}); 